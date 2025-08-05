using System.Text.Json;
using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
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
    private readonly IEnumerable<IProfileMigration> _sortedMigrations = profileMigrations.Sort();

    public SptProfile HandlePendingMigrations(JsonObject profile)
    {
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

        var ranMigrations = new List<IProfileMigration>();

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
}
