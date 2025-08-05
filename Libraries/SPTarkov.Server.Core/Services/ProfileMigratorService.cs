using System.Text.Json;
using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Migration;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class ProfileMigratorService(
    IEnumerable<IProfileMigration> profileMigrations,
    TimeUtil timeUtil,
    ISptLogger<ProfileMigratorService> logger
)
{
    private IEnumerable<AbstractProfileMigration> _sortedMigrations = [];

    public SptProfile HandlePendingMigrations(JsonObject profile)
    {
        // On the initial run, begin sorting our migrations
        // This will sort it so that any non-prerequisite migrations go first
        // And then all the prerequisite ones.
        if (!_sortedMigrations.Any())
        {
            _sortedMigrations = SortMigrations();
        }

        var profileId = profile["info"]?["id"]?.GetValue<string>();

        // Profile is due for a wipe or a reset, do not continue here.
        if (
            profile["characters"]?["pmc"]?["Info"] == null
            || profile["characters"]?["scav"]?["Info"] == null
            || (profile["info"]?["wipe"]?.GetValue<bool>() == true)
        )
        {
            return profile.Deserialize<SptProfile>(JsonUtil.JsonSerializerOptionsNoIndent)
                ?? throw new InvalidOperationException($"Could not deserialize the profile: {profileId}");
        }

        var ranMigrations = new List<AbstractProfileMigration>();

        foreach (var profileMigration in _sortedMigrations)
        {
            if (profileMigration.CanMigrate(profile, ranMigrations))
            {
                logger.Warning($"{profileId} has a pending profile migration: {profileMigration.MigrationName}");

                var migratedProfile = profileMigration.Migrate(profile);

                if (migratedProfile is not null)
                {
                    profile = migratedProfile;

                    ranMigrations.Add(profileMigration);
                }
            }
        }

        SptProfile sptReadyProfile;

        try
        {
            sptReadyProfile =
                profile.Deserialize<SptProfile>(JsonUtil.JsonSerializerOptionsNoIndent)
                ?? throw new InvalidOperationException($"Could not deserialize the profile.");
        }
        catch (Exception ex)
        {
            logger.Critical($"Could not load profile: {profileId}");
            logger.Critical(ex.ToString());

            if (ex.StackTrace is not null)
            {
                logger.Critical(ex.StackTrace);
            }

            // Throw here, immediately stops execution of the server upon detecting a messed up profile
            throw;
        }

        foreach (var ranMigration in ranMigrations)
        {
            if (ranMigration.PostMigrate(sptReadyProfile))
            {
                logger.Success($"{profileId} successfully ran profile migration: {ranMigration.MigrationName}");

                if (sptReadyProfile.SptData!.Migrations is null)
                {
                    sptReadyProfile.SptData.Migrations = [];
                }

                sptReadyProfile.SptData.Migrations.Add(ranMigration.MigrationName, timeUtil.GetTimeStamp());
            }
        }

        return sptReadyProfile;
    }

    protected IEnumerable<AbstractProfileMigration> SortMigrations()
    {
        var sortedMigrations = new List<AbstractProfileMigration>();
        var visitedMigrations = new Dictionary<Type, bool>();
        var migrationDict = profileMigrations.Cast<AbstractProfileMigration>().ToDictionary(m => m.GetType());

        foreach (var migration in profileMigrations.Cast<AbstractProfileMigration>())
        {
            VisitMigrationForSort(migration, migrationDict, visitedMigrations, sortedMigrations);
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
            throw new InvalidOperationException($"Cycle detected in migration prerequisites involving: {migrationType.Name}");
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
            VisitMigrationForSort(prereqMigration, migrationTypeDictionary, visitedTypeDictionary, sortedMigrations);
        }

        // Done visiting, mark it as fully visited and add it to the sorted migrations
        visitedTypeDictionary[migrationType] = true;
        sortedMigrations.Add(migration);
    }
}
