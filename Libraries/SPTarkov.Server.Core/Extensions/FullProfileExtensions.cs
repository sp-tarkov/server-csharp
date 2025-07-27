using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Extensions
{
    public static class FullProfileExtensions
    {
        /// <summary>
        ///     Add a list of suit ids to a profiles suit list, no duplicates
        /// </summary>
        /// <param name="fullProfile">Profile to add clothing to</param>
        /// <param name="clothingIds">Clothing Ids to add to profile</param>
        public static void AddSuitsToProfile(
            this SptProfile fullProfile,
            IEnumerable<MongoId> clothingIds
        )
        {
            fullProfile.CustomisationUnlocks ??= [];

            foreach (var suitId in clothingIds)
            {
                if (
                    !fullProfile.CustomisationUnlocks.Exists(customisation =>
                        customisation.Id == suitId
                    )
                )
                {
                    // Clothing item doesn't exist in profile, add it
                    fullProfile.CustomisationUnlocks.Add(
                        new CustomisationStorage
                        {
                            Id = suitId,
                            Source = CustomisationSource.UNLOCKED_IN_GAME,
                            Type = CustomisationType.SUITE,
                        }
                    );
                }
            }
        }

        /// <summary>
        ///     Add customisations to game profiles based on game edition
        /// </summary>
        /// <param name="fullProfile">Profile to add customisations to</param>
        public static void AddCustomisationUnlocksToProfile(this SptProfile fullProfile)
        {
            // Some game versions have additional customisation unlocks
            var gameEdition = fullProfile.GetGameEdition();

            switch (gameEdition)
            {
                case GameEditions.EDGE_OF_DARKNESS:
                    // Gets EoD tags
                    fullProfile.CustomisationUnlocks.Add(
                        new CustomisationStorage
                        {
                            Id = "6746fd09bafff85008048838",
                            Source = CustomisationSource.DEFAULT,
                            Type = CustomisationType.DOG_TAG,
                        }
                    );

                    fullProfile.CustomisationUnlocks.Add(
                        new CustomisationStorage
                        {
                            Id = "67471938bafff850080488b7",
                            Source = CustomisationSource.DEFAULT,
                            Type = CustomisationType.DOG_TAG,
                        }
                    );

                    break;
                case GameEditions.UNHEARD:
                    // Gets EoD+Unheard tags
                    fullProfile.CustomisationUnlocks.Add(
                        new CustomisationStorage
                        {
                            Id = "6746fd09bafff85008048838",
                            Source = CustomisationSource.DEFAULT,
                            Type = CustomisationType.DOG_TAG,
                        }
                    );

                    fullProfile.CustomisationUnlocks.Add(
                        new CustomisationStorage
                        {
                            Id = "67471938bafff850080488b7",
                            Source = CustomisationSource.DEFAULT,
                            Type = CustomisationType.DOG_TAG,
                        }
                    );

                    fullProfile.CustomisationUnlocks.Add(
                        new CustomisationStorage
                        {
                            Id = "67471928d17d6431550563b5",
                            Source = CustomisationSource.DEFAULT,
                            Type = CustomisationType.DOG_TAG,
                        }
                    );

                    fullProfile.CustomisationUnlocks.Add(
                        new CustomisationStorage
                        {
                            Id = "6747193f170146228c0d2226",
                            Source = CustomisationSource.DEFAULT,
                            Type = CustomisationType.DOG_TAG,
                        }
                    );

                    // Unheard Clothing (Cultist Hood)
                    fullProfile.CustomisationUnlocks.Add(
                        new CustomisationStorage
                        {
                            Id = "666841a02537107dc508b704",
                            Source = CustomisationSource.DEFAULT,
                            Type = CustomisationType.SUITE,
                        }
                    );

                    // Unheard background
                    fullProfile.CustomisationUnlocks.Add(
                        new CustomisationStorage
                        {
                            Id = "675850ba33627edb710b0592",
                            Source = CustomisationSource.DEFAULT,
                            Type = CustomisationType.ENVIRONMENT,
                        }
                    );

                    break;
            }

            var prestigeLevel = fullProfile?.CharacterData?.PmcData?.Info?.PrestigeLevel;

            if (prestigeLevel is not null)
            {
                if (prestigeLevel >= 1)
                {
                    fullProfile.CustomisationUnlocks.Add(
                        new CustomisationStorage
                        {
                            Id = "674dbf593bee1152d407f005",
                            Source = CustomisationSource.DEFAULT,
                            Type = CustomisationType.DOG_TAG,
                        }
                    );
                }

                if (prestigeLevel >= 2)
                {
                    fullProfile.CustomisationUnlocks.Add(
                        new CustomisationStorage
                        {
                            Id = "675dcfea7ae1a8792107ca99",
                            Source = CustomisationSource.DEFAULT,
                            Type = CustomisationType.DOG_TAG,
                        }
                    );
                }
            }

            // Dev profile additions
            if (fullProfile.ProfileInfo.Edition.ToLowerInvariant().Contains("developer"))
            // CyberTark background
            {
                fullProfile.CustomisationUnlocks.Add(
                    new CustomisationStorage
                    {
                        Id = "67585108def253bd97084552",
                        Source = CustomisationSource.DEFAULT,
                        Type = CustomisationType.ENVIRONMENT,
                    }
                );
            }
        }

        /// <summary>
        ///     Get the game edition of a profile chosen on creation in Launcher
        /// </summary>
        public static string GetGameEdition(this SptProfile fullProfile)
        {
            var edition = fullProfile.CharacterData?.PmcData?.Info?.GameVersion;
            if (edition is not null)
            {
                return edition;
            }

            // Edge case - profile not created yet, fall back to what launcher has set
            var launcherEdition = fullProfile.ProfileInfo.Edition;
            switch (launcherEdition.ToLowerInvariant())
            {
                case "edge of darkness":
                    return GameEditions.EDGE_OF_DARKNESS;
                case "unheard":
                    return GameEditions.UNHEARD;
                default:
                    return GameEditions.STANDARD;
            }
        }

        /// <summary>
        ///     Add the given number of extra repeatable quests for the given type of repeatable to the users profile
        /// </summary>
        /// <param name="fullProfile">Profile to add the extra repeatable to</param>
        /// <param name="repeatableId">The ID of the type of repeatable to increase</param>
        /// <param name="rewardValue">The number of extra repeatables to add</param>
        public static void AddExtraRepeatableQuest(
            this SptProfile fullProfile,
            MongoId repeatableId,
            double rewardValue
        )
        {
            fullProfile.SptData.ExtraRepeatableQuests ??= new Dictionary<MongoId, double>();

            if (!fullProfile.SptData.ExtraRepeatableQuests.TryAdd(repeatableId, 0))
            {
                fullProfile.SptData.ExtraRepeatableQuests[repeatableId] += rewardValue;
            }
        }

        /// <summary>
        ///     Is the provided session id for a developer account
        /// </summary>
        /// <param name="fullProfile">Profile to check</param>
        /// <returns>True if account is developer</returns>
        public static bool IsDeveloperAccount(this SptProfile fullProfile)
        {
            return fullProfile?.ProfileInfo?.Edition?.ToLowerInvariant().StartsWith("spt developer")
                ?? false;
        }
    }
}
