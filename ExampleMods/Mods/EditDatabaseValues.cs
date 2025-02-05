using SptCommon.Annotations;
using Core.Models.Eft.Hideout;
using Core.Models.Enums;
using Core.Models.Utils;
using Core.Services;

namespace ExampleMods.Mods;

[Injectable]
public class EditDatabaseValues
{
    private readonly DatabaseService _databaseService;

    public EditDatabaseValues(
        DatabaseService databaseService)
    {
        _databaseService = databaseService;

        Run();
    }

    public void Run()
    {
        // When SPT starts, it stores all the data found in (SPT_Data\Server\database) in memory
        // We can use the '_databaseService' we injected to access this data, this includes files from EFT and SPT

        // Lets edit some globals settings to make the game easier
        EditGlobals();

        // Lets edit the BTR to have the christmas tarcola skin
        EditBtr();

        // Let's edit the hideout so it's easier to upgrade the lavatory
        EditHideout();

        // Lets edit the default scav to 
        EditScavSettings();

        // Lets edit Customs
        EditCustoms();
    }

    private void EditGlobals()
    {
        // Let's edit settings in the GLOBALS file (database/globals.json)
        var globals = _databaseService.GetGlobals();

        // Let's edit the scav cooldown to be 1 second
        globals.Configuration.SavagePlayCooldown = 1;


        // Now lets try editing the ragfair unlock level, lets get the ragfair settings first
        var ragfairSettings = globals.Configuration.RagFair;

        // Lets set the level you need to be to access flea to be 1
        ragfairSettings.MinUserLevel = 1;


        // Now lets increase the number of offers you can have listed at one time
        // The max is stored in a list, different flea ratings give different offer amounts

        // We loop over all the settings, setting all of them to be 20
        foreach (var offerCountSettings in ragfairSettings.MaxActiveOfferCount) offerCountSettings.Count = 20;
    }

    private void EditBtr()
    {
        // BTR setting can be found in the GLOBALS file too
        var globals = _databaseService.GetGlobals();

        // We get the BTR settings from globals first
        var btrSettings = globals.Configuration.BTRSettings;

        // Let's get the settings for woods specifically, we use 'tryGetValue' for this, the settings will be stored in 'woodsBtrSettings'
        btrSettings.MapsConfigs.TryGetValue("Woods", out var woodsBtrSettings);

        // Lets set the BTR to use the christmas skin
        woodsBtrSettings.BtrSkin = "Tarcola";
    }

    private void EditHideout()
    {
        // Hideout data can be found in (SPT_Data\Server\database\hideout)
        var hideout = _databaseService.GetHideout();

        // We want the areas, they're stored in a list
        var hideoutAreas = hideout.Areas;

        // We find the toilet, we use 'firstOrDefault', if we cant find the lavatory, 'lavatoryArea' will be null
        var lavatoryArea = hideoutAreas.FirstOrDefault(area => area.Type == HideoutAreas.LAVATORY);


        // Now we have the toilet, we can find the requirements to craft, all data is stored by stage
        var toiletStages = lavatoryArea.Stages;

        // Stages are stored in a dictionary, a dictionary has a 'key' and a 'value'
        // In this case, the 'key' is the upgrade stage, e.g. "1", or "2"
        // We reference to each stage as a 'stageKvP' this means 'Key value Pair', every key has a value (key = stage number, value = data for that stage)
        foreach (var stageKvP in toiletStages)
        {
            // while we're here, we can make the stages craft really fast (60 seconds)
            stageKvP.Value.ConstructionTime = 60;

            // Let's get the stage requirements, they're a list
            var stageRequirements = stageKvP.Value.Requirements;

            // We empty the requirements out, now it can be built straight away
            stageRequirements.Clear();
        }
    }

    private void EditScavSettings()
    {
        var bots = _databaseService.GetBots();

        // Same as the above example, we use 'TryGetValue' to get the 'assault' bot (assault is the internal name for scavs)
        bots.Types.TryGetValue("assault", out var assaultBot);

        // Let's make the chance to get a good backpack really high
        assaultBot.BotInventory.Equipment.TryGetValue(EquipmentSlots.Backpack, out var backPacks);

        // We access the backpacks dictionary by key directly using square brackets, we use ItemTpl to get the items ID
        // Alternately, we could have typed backPacks["59e763f286f7742ee57895da"] and done the same thing, ItemTpl makes it easier to read
        backPacks[ItemTpl.BACKPACK_PILGRIM_TOURIST] = 999999;


        // Now lets make them always have an M4A1
        assaultBot.BotInventory.Equipment.TryGetValue(EquipmentSlots.FirstPrimaryWeapon, out var primaryWeapons);

        // We edit the weight value (pick chance) that is already there to be massive, making the item more likely to be picked
        primaryWeapons[ItemTpl.ASSAULTRIFLE_COLT_M4A1_556X45_ASSAULT_RIFLE] = 999999;


        // Now lets make them always have the first name of Gary
        // We start by removing all the existing names
        assaultBot.FirstNames.Clear();

        // We add the new name Gary, very menacing
        assaultBot.FirstNames.Add("Gary");
    }

    private void EditCustoms()
    {
        // Let's get all the maps (called locations)
        var locations = _databaseService.GetLocations();

        // Customs is called 'bigmap' in eft
        var customs = locations.Bigmap;

        // Lets get the exits and make them all 100% chance to appear
        var exits = customs.Base.Exits;

        // They're stored as a list so we can loop over them
        foreach (var exit in exits)
        {
            // I can't remember which one is used, you'd assume ChancePVE is used in pve, but this is BSG we're dealing with
            // So we set both
            exit.Chance = 100;
            exit.ChancePVE = 100;
        }


        // Lets try editing the airdrops on customs to be better
        var airdropSettings = customs.Base.AirdropParameters;

        // They're stored in an array but there's only one bunch of settings, it means we have to get the first item from the list,
        // An alternate way to access the first item is done by using square brackets with the 'index' of the item we want,
        // indexes start at 0 so we want to type "[0]" to access the first item in the list,
        var actualAirdropSettings = airdropSettings.First();

        // Make it spawn 100%
        actualAirdropSettings.PlaneAirdropChance = 1; // Number between 0 and 1

        // Make it spawn as early as start of raid
        actualAirdropSettings.PlaneAirdropStartMin = 1;


        // Let's make bosses spawn 100% of the time

        // We get all the bosses, they're stored in a list
        var bosses = customs.Base.BossLocationSpawn;

        // Let's get Reshala, we use "FirstOrDefault" and look for the first boss with the name "bossBully"
        var reshala = bosses.FirstOrDefault(boss => boss.BossName == "bossBully");

        // Set him to 100%
        reshala.BossChance = 100;
    }
}
