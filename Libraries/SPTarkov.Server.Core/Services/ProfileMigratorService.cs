using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Migration;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Services
{
    [Injectable(InjectionType.Singleton)]
    public class ProfileMigratorService(
        IEnumerable<IProfileMigration> profileMigrations,
        TimeUtil timeUtil,
        ISptLogger<ProfileMigratorService> logger
    )
    {
        private IEnumerable<AbstractProfileMigration> _sortedMigrations = [];

        public JsonObject HandlePendingMigrations(JsonObject profile)
        {
            // On the initial run, begin sorting our migrations
            // This will sort it so that any non prerequisite migrations go first
            // And then all of the prerequisite ones.
            if (!_sortedMigrations.Any())
            {
                _sortedMigrations = SortMigrations();
            }

            // Profile is due for a wipe or a reset, do not continue here.
            if (
                profile["characters"]?["pmc"]?["Info"] == null
                || profile["characters"]?["scav"]?["Info"] == null
                || (profile["info"]?["wipe"]?.GetValue<bool>() == true)
            )
            {
                return profile;
            }

            var profileId = profile["info"]?["id"]?.GetValue<string>();

            foreach (var profileMigration in _sortedMigrations)
            {
                if (profileMigration.CanMigrate(profile))
                {
                    logger.Warning(
                        $"{profileId} has a pending profile migration: {profileMigration.MigrationName}"
                    );

                    var migratedProfile = profileMigration.Migrate(profile);

                    if (migratedProfile is not null)
                    {
                        SetCompletedMigration(profile, profileMigration.MigrationName);

                        profile = migratedProfile;

                        logger.Success(
                            $"{profileId} successfully ran profile migration: {profileMigration.MigrationName}"
                        );
                    }
                }
            }

            return profile;
        }

        protected void SetCompletedMigration(JsonObject profile, string migrationName)
        {
            var profileMigrations = profile["spt"]["migrations"] as JsonObject;

            profileMigrations[migrationName] = JsonValue.Create(timeUtil.GetTimeStamp());
        }

        protected IEnumerable<AbstractProfileMigration> SortMigrations()
        {
            var sortedMigrations = new List<AbstractProfileMigration>();
            var visitedMigrations = new Dictionary<Type, bool>();
            var migrationDict = profileMigrations
                .Cast<AbstractProfileMigration>()
                .ToDictionary(m => m.GetType());

            foreach (var migration in profileMigrations.Cast<AbstractProfileMigration>())
            {
                VisitMigrationForSort(
                    migration,
                    migrationDict,
                    visitedMigrations,
                    sortedMigrations
                );
            }

            return sortedMigrations;
        }

        protected void VisitMigrationForSort(
            AbstractProfileMigration migration,
            Dictionary<Type, AbstractProfileMigration> migrationTypeDictionary,
            Dictionary<Type, bool> visitedTypeDictionary,
            List<AbstractProfileMigration> sortedMigrations
        )
        {
            var migrationType = migration.GetType();

            if (visitedTypeDictionary.TryGetValue(migrationType, out var isVisited))
            {
                if (isVisited)
                {
                    return;
                }

                // Big error, two migrations should never depend on one another
                throw new InvalidOperationException(
                    $"Cycle detected in migration prerequisites involving: {migrationType.Name}"
                );
            }

            // Mark the current migration type for visiting
            visitedTypeDictionary[migrationType] = false;

            foreach (var prerequisiteType in migration.PrerequisiteMigrations)
            {
                if (!migrationTypeDictionary.TryGetValue(prerequisiteType, out var prereqMigration))
                {
                    continue;
                }

                // Visit the next prerequisite
                VisitMigrationForSort(
                    prereqMigration,
                    migrationTypeDictionary,
                    visitedTypeDictionary,
                    sortedMigrations
                );
            }

            // Done visiting, mark it as fully visited and add it to the sorted migrations
            visitedTypeDictionary[migrationType] = true;
            sortedMigrations.Add(migration);
        }
    }
}
