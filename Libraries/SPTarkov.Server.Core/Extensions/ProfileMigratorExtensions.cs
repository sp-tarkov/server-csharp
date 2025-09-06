using SPTarkov.Server.Core.Migration;

namespace SPTarkov.Server.Core.Extensions;

public static class ProfileMigratorExtensions
{
    /// <summary>
    /// Sorts the profile migrations in dependency order, ensuring that each migration appears
    /// after all of its prerequisite migrations.
    /// </summary>
    /// <param name="profileMigrations">The collection of profile migrations to sort.</param>
    /// <returns>A topologically sorted list of migrations.</returns>
    public static IEnumerable<IProfileMigration> Sort(this IEnumerable<IProfileMigration> profileMigrations)
    {
        var sortedMigrations = new List<IProfileMigration>();
        var visitedMigrations = new Dictionary<Type, bool>();
        var migrationDict = profileMigrations.ToDictionary(m => m.GetType());

        foreach (var migration in profileMigrations)
        {
            VisitMigrationForSort(migration, migrationDict, visitedMigrations, sortedMigrations);
        }

        return sortedMigrations;
    }

    internal static void VisitMigrationForSort(
        IProfileMigration migration,
        Dictionary<Type, IProfileMigration> migrationTypeDictionary,
        Dictionary<Type, bool> visitedTypeDictionary,
        List<IProfileMigration> sortedMigrations
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
