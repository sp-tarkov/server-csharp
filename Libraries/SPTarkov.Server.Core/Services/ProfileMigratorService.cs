using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Migration;
using SPTarkov.Server.Core.Models.Eft.Profile;
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
        public SptProfile HandlePendingMigrations(SptProfile profile)
        {
            // Profile is due for a wipe or a reset, do not continue here.
            if (
                profile.CharacterData?.PmcData?.Info is null
                || profile.CharacterData?.ScavData?.Info is null
                || profile.ProfileInfo?.IsWiped == true
            )
            {
                return profile;
            }

            foreach (AbstractProfileMigration profileMigration in profileMigrations)
            {
                if (profileMigration.CanMigrate(profile))
                {
                    logger.Warning(
                        $"{profile.ProfileInfo!.ProfileId} Has a pending profile migration: {profileMigration.MigrationName}"
                    );

                    var migratedProfile = profileMigration.Migrate(profile);

                    if (migratedProfile is not null)
                    {
                        migratedProfile.SptData!.Migrations![profileMigration.MigrationName] =
                            timeUtil.GetTimeStamp();
                        profile = migratedProfile;

                        logger.Success(
                            $"{profile.ProfileInfo!.ProfileId} has successfully ran profile migration: {profileMigration.MigrationName}"
                        );
                    }
                }
            }

            return profile;
        }
    }
}
