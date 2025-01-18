using Core.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Hideout;
using Core.Models.Eft.ItemEvent;
using Core.Models.Enums;
using Core.Models.Utils;
using Core.Services;
using Core.Utils;

namespace Core.Helpers;

[Injectable]
public class HideoutHelper(
    ISptLogger<HideoutHelper> _logger,
    TimeUtil _timeUtil,
    LocalisationService _localisationService
)
{
    /// <summary>
    /// Add production to profiles' Hideout.Production array
    /// </summary>
    /// <param name="profileData">Profile to add production to</param>
    /// <param name="productionRequest">Production request</param>
    /// <param name="sessionId">Session id</param>
    /// <returns>client response</returns>
    public void RegisterProduction(
        PmcData profileData,
        HideoutSingleProductionStartRequestData productionRequest,
        string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// This convenience function initializes new Production Object
    /// with all the constants.
    /// </summary>
    public void InitProduction(
        string recipeId,
        int productionTime,
        bool needFuelForAllProductionTime)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Is the provided object a Production type
    /// </summary>
    /// <param name="productive"></param>
    /// <returns></returns>
    public bool IsProductionType(Productive productive)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Apply bonus to player profile given after completing hideout upgrades
    /// </summary>
    /// <param name="profileData">Profile to add bonus to</param>
    /// <param name="bonus">Bonus to add to profile</param>
    public void ApplyPlayerUpgradesBonuses(PmcData profileData, Bonus bonus)
    {
        // Handle additional changes some bonuses need before being added
        var bonusToAdd = new Bonus();
        switch (bonus.Type)
        {
            case BonusType.StashSize:
            {
                // Find stash item and adjust tpl to new tpl from bonus
                var stashItem = profileData.Inventory.Items.FirstOrDefault((x) => x.Id == profileData.Inventory.Stash);
                if (stashItem is null)
                {
                    _logger.Warning(_localisationService.GetText("hideout-unable_to_apply_stashsize_bonus_no_stash_found", profileData.Inventory.Stash));
                }

                stashItem.Template = bonus.TemplateId;

                break;
            }
            case BonusType.MaximumEnergyReserve:
                // Amend max energy in profile
                profileData.Health.Energy.Maximum += bonus.Value;
                break;
            case BonusType.TextBonus:
                // Delete values before they're added to profile
                bonus.IsPassive = null;
                bonus.IsProduction = null;
                bonus.IsVisible = null;
                break;
        }

        // Add bonus to player bonuses array in profile
        // EnergyRegeneration, HealthRegeneration, RagfairCommission, ScavCooldownTimer, SkillGroupLevelingBoost, ExperienceRate, QuestMoneyReward etc
        _logger.Debug($"Adding bonus: {bonus.Type} to profile, value: {bonus.Value}");
        profileData.Bonuses.Add(bonus);
    }

    /// <summary>
    /// Process a players hideout, update areas that use resources + increment production timers
    /// </summary>
    /// <param name="sessionId">Session id</param>
    public void UpdatePlayerHideout(string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get various properties that will be passed to hideout update-related functions
    /// </summary>
    /// <param name="profileData">Player profile</param>
    /// <returns>Properties</returns>
    protected (int btcFarmCGs, bool isGeneratorOn, bool waterCollectorHasFilter) GetHideoutProperties(PmcData profileData)
    {
        throw new NotImplementedException();
    }

    protected bool DoesWaterCollectorHaveFilter(BotHideoutArea waterCollector)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Iterate over productions and update their progress timers
    /// </summary>
    /// <param name="profileData">Profile to check for productions and update</param>
    /// <param name="hideoutProperties">Hideout properties</param>
    protected void UpdateProductionTimers(
        PmcData profileData,
        (int btcFarmCGs, bool isGeneratorOn, bool waterCollectorHasFilter) hideoutProperties)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Is a craft from a particular hideout area
    /// </summary>
    /// <param name="craft">Craft to check</param>
    /// <param name="hideoutType">Type to check craft against</param>
    /// <returns>True if it is from that area</returns>
    protected bool IsCraftOfType(Production craft, HideoutAreas hideoutType)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Has the craft completed
    /// Ignores bitcoin farm/cultist circle as they're continuous crafts
    /// </summary>
    /// <param name="craft">Craft to check</param>
    /// <returns>True when craft is complete</returns>
    protected bool IsCraftComplete(Production craft)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Update progress timer for water collector
    /// </summary>
    /// <param name="pmcData">profile to update</param>
    /// <param name="productionId">id of water collection production to update</param>
    /// <param name="hideoutProperties">Hideout properties</param>
    protected void UpdateWaterCollectorProductionTimer(
        PmcData pmcData,
        string productionId,
        Dictionary<string, object> hideoutProperties)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Update a productions progress value based on the amount of time that has passed
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="prodId">Production id being crafted</param>
    /// <param name="recipe">Recipe data being crafted</param>
    /// <param name="hideoutProperties"></param>
    protected void UpdateProductionProgress(
        PmcData pmcData,
        string prodId,
        HideoutProduction recipe,
        Dictionary<string, object> hideoutProperties)
    {
        throw new NotImplementedException();
    }

    protected void UpdateCultistCircleCraftProgress(PmcData pmcData, string prodId)
    {
        throw new NotImplementedException();
    }

    protected void FlagCultistCircleCraftAsComplete(Productive production)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Check if a productions progress value matches its corresponding recipes production time value
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="prodId">Production id</param>
    /// <param name="recipe">Recipe being crafted</param>
    /// <returns>progress matches productionTime from recipe</returns>
    protected bool DoesProgressMatchProductionTime(PmcData pmcData, string prodId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Update progress timer for scav case
    /// </summary>
    /// <param name="pmcData">Profile to update</param>
    /// <param name="productionId">Id of scav case production to update</param>
    protected void UpdateScavCaseProductionTimer(PmcData pmcData, string productionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Iterate over hideout areas that use resources (fuel/filters etc) and update associated values
    /// </summary>
    /// <param name="sessionID">Session id</param>
    /// <param name="pmcData">Profile to update areas of</param>
    /// <param name="hideoutProperties">hideout properties</param>
    protected void UpdateAreasWithResources(
        string sessionID,
        PmcData pmcData,
        Dictionary<string, object> hideoutProperties)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Decrease fuel from generator slots based on amount of time since last time this occurred
    /// </summary>
    /// <param name="generatorArea">Hideout area</param>
    /// <param name="pmcData">Player profile</param>
    /// <param name="isGeneratorOn">Is the generator turned on since last update</param>
    protected void UpdateFuel(BotHideoutArea generatorArea, PmcData pmcData, bool isGeneratorOn)
    {
        throw new NotImplementedException();
    }

    protected void UpdateWaterCollector(
        string sessionId,
        PmcData pmcData,
        BotHideoutArea area,
        Dictionary<string, object> hideoutProperties)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get craft time and make adjustments to account for dev profile + crafting skill level
    /// </summary>
    /// <param name="playerProfile">Player profile making craft</param>
    /// <param name="recipeId">Recipe being crafted</param>
    /// <param name="applyHideoutManagementBonus">Should the hideout management bonus be applied to the calculation</param>
    /// <returns>Items craft time with bonuses subtracted</returns>
    public double GetAdjustedCraftTimeWithSkills(
        PmcData playerProfile,
        string recipeId,
        bool applyHideoutManagementBonus = false)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Adjust water filter objects resourceValue or delete when they reach 0 resource
    /// </summary>
    /// <param name="waterFilterArea">Water filter area to update</param>
    /// <param name="production">Production object</param>
    /// <param name="isGeneratorOn">Is generator enabled</param>
    /// <param name="playerProfile">Player profile</param>
    protected void UpdateWaterFilters(
        BotHideoutArea waterFilterArea,
        Production production,
        bool isGeneratorOn,
        PmcData playerProfile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get an adjusted water filter drain rate based on time elapsed since last run,
    /// handle edge case when craft time has gone on longer than total production time
    /// </summary>
    /// <param name="secondsSinceServerTick">Time passed</param>
    /// <param name="totalProductionTime">Total time collecting water</param>
    /// <param name="productionProgress">How far water collector has progressed</param>
    /// <param name="baseFilterDrainRate">Base drain rate</param>
    /// <returns>Drain rate (adjusted)</returns>
    protected double GetTimeAdjustedWaterFilterDrainRate(
        double secondsSinceServerTick,
        double totalProductionTime,
        double productionProgress,
        double baseFilterDrainRate)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the water filter drain rate based on hideout bonuses player has
    /// </summary>
    /// <param name="playerProfile">Player profile</param>
    /// <returns>Drain rate</returns>
    protected double GetWaterFilterDrainRate(PmcData playerProfile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the production time in seconds for the desired production
    /// </summary>
    /// <param name="prodId">Id, e.g. Water collector id</param>
    /// <returns>Seconds to produce item</returns>
    protected double GetTotalProductionTimeSeconds(string prodId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create a upd object using passed in parameters
    /// </summary>
    /// <param name="stackCount"></param>
    /// <param name="resourceValue"></param>
    /// <param name="resourceUnitsConsumed"></param>
    /// <returns>Upd</returns>
    protected Upd GetAreaUpdObject(
        int stackCount,
        double resourceValue,
        int resourceUnitsConsumed,
        bool isFoundInRaid)
    {
        throw new NotImplementedException();
    }

    protected void UpdateAirFilters(BotHideoutArea airFilterArea, PmcData playerProfile, bool isGeneratorOn)
    {
        throw new NotImplementedException();
    }

    protected void UpdateBitcoinFarm(
        PmcData playerProfile,
        Productive btcProduction,
        int btcFarmCGs,
        bool isGeneratorOn)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add bitcoin object to btc production products array and set progress time
    /// </summary>
    /// <param name="btcProd">Bitcoin production object</param>
    /// <param name="coinCraftTimeSeconds">Time to craft a bitcoin</param>
    protected void AddBtcToProduction(Production btcProd, double coinCraftTimeSeconds)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get number of ticks that have passed since hideout areas were last processed, reduced when generator is off
    /// </summary>
    /// <param name="playerProfile">Player profile</param>
    /// <param name="isGeneratorOn">Is the generator on for the duration of elapsed time</param>
    /// <param name="recipe">Hideout production recipe being crafted we need the ticks for</param>
    /// <returns>Amount of time elapsed in seconds</returns>
    protected double GetTimeElapsedSinceLastServerTick(
        PmcData playerProfile,
        bool isGeneratorOn,
        HideoutProduction recipe = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a count of how many possible BTC can be gathered by the profile
    /// </summary>
    /// <param name="profileData">Profile to look up</param>
    /// <returns>Coin slot count</returns>
    protected int GetBTCSlots(PmcData profileData)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a count of how many additional bitcoins player hideout can hold with elite skill
    /// </summary>
    protected int GetEliteSkillAdditionalBitcoinSlotCount()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// HideoutManagement skill gives a consumption bonus the higher the level
    /// 0.5% per level per 1-51, (25.5% at max)
    /// </summary>
    /// <param name="profileData">Profile to get hideout consumption level from</param>
    /// <returns>Consumption bonus</returns>
    protected double GetHideoutManagementConsumptionBonus(PmcData profileData)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a multiplier based on player's skill level and value per level
    /// </summary>
    /// <param name="profileData">Player profile</param>
    /// <param name="skill">Player skill from profile</param>
    /// <param name="valuePerLevel">Value from globals.config.SkillsSettings - `PerLevel`</param>
    /// <returns>Multiplier from 0 to 1</returns>
    protected double GetSkillBonusMultipliedBySkillLevel(PmcData profileData, SkillTypes skill, double valuePerLevel)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// </summary>
    /// <param name="profileData">Player profile</param>
    /// <param name="productionTime">Time to complete hideout craft in seconds</param>
    /// <param name="skill">Skill bonus to get reduction from</param>
    /// <param name="amountPerLevel">Skill bonus amount to apply</param>
    /// <returns>Seconds to reduce craft time by</returns>
    public double GetSkillProductionTimeReduction(
        PmcData profileData,
        double productionTime,
        SkillTypes skill,
        double amountPerLevel)
    {
        throw new NotImplementedException();
    }

    public bool IsProduction(Productive productive)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gather crafted BTC from hideout area and add to inventory
    /// Reset production start timestamp if hideout area at full coin capacity
    /// </summary>
    /// <param name="profileData">Player profile</param>
    /// <param name="request">Take production request</param>
    /// <param name="sessionId">Session id</param>
    /// <param name="output">Output object to update</param>
    public void GetBTC(
        PmcData profileData,
        HideoutTakeProductionRequestData request,
        string sessionId,
        ItemEventRouterResponse output)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Upgrade hideout wall from starting level to interactable level if necessary stations have been upgraded
    /// </summary>
    /// <param name="profileData">Profile to upgrade wall in</param>
    public void UnlockHideoutWallInProfile(PmcData profileData)
    {
        var profileHideoutAreas = profileData.Hideout.Areas;
        var waterCollector = profileHideoutAreas.FirstOrDefault((x) => x.Type == HideoutAreas.WATER_COLLECTOR);
        var medStation = profileHideoutAreas.FirstOrDefault((x) => x.Type == HideoutAreas.MEDSTATION);
        var wall = profileHideoutAreas.FirstOrDefault((x) => x.Type == HideoutAreas.EMERGENCY_WALL);

        // No collector or med station, skip
        if ((waterCollector is null && medStation is null))
        {
            return;
        }

        // If med-station > level 1 AND water collector > level 1 AND wall is level 0
        if (waterCollector?.Level >= 1 && medStation?.Level >= 1 && wall?.Level <= 0)
        {
            wall.Level = 3;
        }
    }

    /// <summary>
    /// Hideout improvement is flagged as complete
    /// </summary>
    /// <param name="improvement">hideout improvement object</param>
    /// <returns>true if complete</returns>
    protected bool HideoutImprovementIsComplete(HideoutImprovement improvement)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Iterate over hideout improvements not completed and check if they need to be adjusted
    /// </summary>
    /// <param name="profileData">Profile to adjust</param>
    public void SetHideoutImprovementsToCompleted(PmcData profileData)
    {
        foreach (var improvementId in profileData.Hideout.Improvements)
        {
            if (!profileData.Hideout.Improvements.TryGetValue(improvementId.Key, out var improvementDetails))
            {
                continue;
            }

            if (improvementDetails.Completed == false && improvementDetails.ImproveCompleteTimestamp < _timeUtil.GetTimeStamp()
               )
            {
                improvementDetails.Completed = true;
            }
        }
    }

    /// <summary>
    /// Add/remove bonus combat skill based on number of dogtags in place of fame hideout area
    /// </summary>
    /// <param name="profileData">Player profile</param>
    public void ApplyPlaceOfFameDogtagBonus(PmcData profileData)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Calculate the raw dogtag combat skill bonus for place of fame based on number of dogtags
    /// Reverse engineered from client code
    /// </summary>
    /// <param name="profileData">Player profile</param>
    /// <param name="activeDogtags">Active dogtags in place of fame dogtag slots</param>
    /// <returns>Combat bonus</returns>
    protected double GetDogtagCombatSkillBonusPercent(PmcData profileData, List<Item> activeDogtags)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// The wall pollutes a profile with various temp buffs/debuffs,
    /// Remove them all
    /// </summary>
    /// <param name="wallAreaData">Hideout area data</param>
    /// <param name="profileData">Player profile</param>
    public void RemoveHideoutWallBuffsAndDebuffs(HideoutArea wallAreaData, PmcData profileData)
    {
        throw new NotImplementedException();
    }
}
