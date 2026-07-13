using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Helpers.Commerce;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Services.Profile;
using Range = SemanticVersioning.Range;

namespace SPTarkov.Server.Core.Migration.Migrations._3._11;

[Injectable]
public sealed class ThreeTenToThreeEleven(
    TradersTable traderTable,
    TemplateTable templateTable,
    // Yes, referencing the helpers directly causes a circular dependency. Too bad!
    IServiceProvider serviceProvider
) : AbstractProfileMigration
{
    private const string OldSuiteDataContextKey = "OldSuiteData";

    public string FromVersion
    {
        get { return "~3.10"; }
    }

    public string ToVersion
    {
        get { return "3.11"; }
    }

    public override string MigrationName
    {
        get { return "310x-SPTSharp"; }
    }

    public override IEnumerable<Type> PrerequisiteMigrations
    {
        get { return [typeof(HideoutSeed)]; }
    }

    public override bool CanMigrate(JsonObject profile, IEnumerable<IProfileMigration> previouslyRanMigrations)
    {
        var profileVersion = GetProfileVersion(profile);

        var fromRange = Range.Parse(FromVersion);

        var versionMatches = fromRange.IsSatisfied(profileVersion);

        return versionMatches;
    }

    public override JsonObject? Migrate(JsonObject profile, ProfileMigrationContext context)
    {
        if (profile.TryGetArray(out var suitsArray, "suits"))
        {
            context.Set(OldSuiteDataContextKey, suitsArray.Select(node => node?.GetValue<string>()).Where(suit => suit != null).ToList()!);
        }

        profile.Remove("suits");

        return profile;
    }

    public override bool PostMigrate(SptProfile profile, ProfileMigrationContext context)
    {
        var oldSuiteData = context.Get<List<string>>(OldSuiteDataContextKey, []);

        if (profile.CustomisationUnlocks is null)
        {
            profile.CustomisationUnlocks = [];
            profile.AddCustomisationUnlocksToProfile();
        }

        profile.CharacterData.PmcData.Prestige ??= [];

        if (profile.CharacterData.PmcData.Inventory.HideoutCustomizationStashId is null)
        {
            profile.CharacterData.PmcData.Inventory.HideoutCustomizationStashId = new("676db384777490e23c45b657");

            //Directly injecting CreateProfileService causes a circular dependency which I can't be bothered to fix just for this
            (serviceProvider.GetService(typeof(CreateProfileService)) as CreateProfileService)!.AddMissingInternalContainersToProfile(
                profile.CharacterData.PmcData
            );
        }

        if (profile.CharacterData.PmcData.Hideout.Customization is null)
        {
            profile.CharacterData.PmcData.Hideout.Customization = new Dictionary<string, Models.Common.MongoId>
            {
                { "Wall", new("675844bdf94a97cbbe096f1a") },
                { "Floor", new("6758443ff94a97cbbe096f18") },
                { "Light", new("675fe8abbc3deae49a0b947f") },
                { "Ceiling", new("673b3f977038192ee006aa09") },
                { "ShootingRangeMark", new("67585d416c72998cf60ed85a") },
            };
        }

        if (profile.CharacterData.PmcData.Info.Side == "Bear")
        {
            ProcessBearProfile(profile, oldSuiteData);
        }

        if (profile.CharacterData.PmcData.Info.Side == "Usec")
        {
            ProcessUsecProfile(profile, oldSuiteData);
        }

        if (profile.CharacterData.PmcData.Achievements.Count > 0)
        {
            var achievementsDb = templateTable.Achievements;

            foreach (var achievementId in profile.CharacterData.PmcData.Achievements.Keys)
            {
                var achievementDb = achievementsDb.FirstOrDefault(a => a.Id == achievementId);
                var rewards = achievementDb?.Rewards;

                if (rewards == null)
                {
                    continue;
                }

                // Only hand out the new hideout customization rewards.
                var filteredRewards = rewards.Where(r => r.Type == RewardType.CustomizationDirect).ToList();

                //Directly injecting RewardHelper causes a circular dependency which I can't be bothered to fix just for this
                (serviceProvider.GetService(typeof(RewardHelper)) as RewardHelper)!.ApplyRewards(
                    filteredRewards,
                    CustomisationSource.ACHIEVEMENT,
                    profile,
                    profile.CharacterData.PmcData,
                    achievementId
                );
            }
        }

        return true;
    }

    private void ProcessBearProfile(SptProfile profile, List<string> oldSuiteData)
    {
        // Reset clothing customization back to default as customization changed in 3.11
        profile.CharacterData.PmcData.Customization.Body = new("5cc0858d14c02e000c6bea66");
        profile.CharacterData.PmcData.Customization.Feet = new("5cc085bb14c02e000e67a5c5");
        profile.CharacterData.PmcData.Customization.Hands = new("5cc0876314c02e000c6bea6b");
        profile.CharacterData.PmcData.Customization.DogTag = new("674731c8bafff850080488bb");

        if (profile.CharacterData.PmcData.Info.GameVersion == "edge_of_darkness")
        {
            profile.CharacterData.PmcData.Customization.DogTag = new("6746fd09bafff85008048838");
        }

        if (profile.CharacterData.PmcData.Info.GameVersion == "unheard_edition")
        {
            profile.CharacterData.PmcData.Customization.DogTag = new("67471928d17d6431550563b5");
        }

        foreach (var oldSuite in oldSuiteData)
        {
            // Default Bear clothing, dont need to add this
            if (oldSuite == "5cd946231388ce000d572fe3" || oldSuite == "5cd945d71388ce000a659dfb" || oldSuite == "666841a02537107dc508b704")
            {
                continue;
            }

            var trader = traderTable.GetTrader("5ac3b934156ae10c4430e83c");
            var traderClothing = trader?.Suits?.FirstOrDefault(s => s.SuiteId == oldSuite);

            if (traderClothing != null)
            {
                var clothingToAdd = new CustomisationStorage
                {
                    Id = traderClothing.SuiteId,
                    Source = CustomisationSource.UNLOCKED_IN_GAME,
                    Type = CustomisationType.SUITE,
                };

                profile.CustomisationUnlocks.Add(clothingToAdd);
            }
        }
    }

    private void ProcessUsecProfile(SptProfile profile, List<string> oldSuiteData)
    {
        // Reset clothing customization back to default as customization changed in 3.11
        profile.CharacterData.PmcData.Customization.Body = new("5cde95d97d6c8b647a3769b0"); //Usec default clothing
        profile.CharacterData.PmcData.Customization.Feet = new("5cde95ef7d6c8b04713c4f2d");
        profile.CharacterData.PmcData.Customization.Hands = new("5cde95fa7d6c8b04737c2d13");
        profile.CharacterData.PmcData.Customization.DogTag = new("674731d1170146228c0d222a"); //Usec base dogtag

        if (profile.CharacterData.PmcData.Info.GameVersion == "edge_of_darkness")
        {
            profile.CharacterData.PmcData.Customization.DogTag = new("67471938bafff850080488b7");
        }

        if (profile.CharacterData.PmcData.Info.GameVersion == "unheard_edition")
        {
            profile.CharacterData.PmcData.Customization.DogTag = new("6747193f170146228c0d2226");
        }

        foreach (var oldSuite in oldSuiteData)
        {
            // Default Usec clothing, dont need to add this
            if (oldSuite == "5cde9ec17d6c8b04723cf479" || oldSuite == "5cde9e957d6c8b0474535da7" || oldSuite == "666841a02537107dc508b704")
            {
                continue;
            }

            var trader = traderTable.GetTrader("5ac3b934156ae10c4430e83c");
            var traderClothing = trader?.Suits?.FirstOrDefault(s => s.SuiteId == oldSuite);

            if (traderClothing != null)
            {
                var clothingToAdd = new CustomisationStorage
                {
                    Id = traderClothing.SuiteId,
                    Source = CustomisationSource.UNLOCKED_IN_GAME,
                    Type = CustomisationType.SUITE,
                };

                profile.CustomisationUnlocks.Add(clothingToAdd);
            }
        }
    }
}
