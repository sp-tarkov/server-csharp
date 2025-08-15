using System.Text.Json;
using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Migration;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class ProfileValidatorService(
    IEnumerable<IProfileMigration> profileMigrations,
    ProfileValidatorHelper profileValidatorHelper,
    TimeUtil timeUtil,
    ISptLogger<ProfileValidatorService> logger
)
{
    private readonly IEnumerable<IProfileMigration> _sortedMigrations = profileMigrations.Sort();

    /// <summary>
    /// Migrates and verifies if profiles are compatible
    /// </summary>
    /// <param name="profile">The profile as a <see cref="JsonObject"/> to verify and migrate</param>
    /// <returns>The migrated and validated profile</returns>
    /// <exception cref="InvalidOperationException">Thrown if a profile file cannot be loaded at all</exception>
    public SptProfile MigrateAndValidateProfile(JsonObject profile)
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

        // The initial part of the profile migrations, this allows for fixing up
        // Any incorrect typing that might not allow the profile to load
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

        SptProfile? sptReadyProfile = null;

        try
        {
            sptReadyProfile =
                profile.Deserialize<SptProfile>(JsonUtil.JsonSerializerOptionsNoIndent)
                ?? throw new InvalidOperationException($"Could not deserialize the profile.");

            profileValidatorHelper.CheckForOrphanedModdedData(new Models.Common.MongoId(profileId), sptReadyProfile);
        }
        catch (Exception ex)
        {
            logger.Critical($"Failed to load profile with ID '{profileId}'. The profile will be marked as invalid.");
            logger.Critical(ex.ToString());

            if (ex.StackTrace is not null)
            {
                logger.Critical(ex.StackTrace);
            }

            // If profile has passed deserialization, but caught an exception on CheckForOrphanedModdedItems
            if (sptReadyProfile?.ProfileInfo is not null)
            {
                sptReadyProfile.ProfileInfo.InvalidOrUnloadableProfile = true;
            }
            else
            {
                // Profile couldn't deserialize, make a small 'mock' profile to emulate it.
                sptReadyProfile = new()
                {
                    ProfileInfo = new Info
                    {
                        ProfileId = new Models.Common.MongoId(profileId),
                        Username = profile["info"]?["username"]?.GetValue<string>() ?? "",
                        InvalidOrUnloadableProfile = true,
                    },
                };
            }

            return sptReadyProfile;
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
