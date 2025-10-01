using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Exceptions.Helpers;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Eft.Inventory;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Enums.Hideout;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class HideoutHelper(
    ISptLogger<HideoutHelper> logger,
    TimeUtil timeUtil,
    ServerLocalisationService serverLocalisationService,
    DatabaseService databaseService,
    EventOutputHolder eventOutputHolder,
    HttpResponseUtil httpResponseUtil,
    ProfileHelper profileHelper,
    InventoryHelper inventoryHelper,
    ItemHelper itemHelper,
    ICloner cloner
)
{
    public static readonly MongoId BitcoinProductionId = new("5d5c205bd582a50d042a3c0e");
    public static readonly MongoId WaterCollectorId = new("5d5589c1f934db045e6c5492");

    /// <summary>
    ///     Add production to profiles' Hideout.Production array
    /// </summary>
    /// <param name="pmcData">Profile to add production to</param>
    /// <param name="productionRequest">Production request</param>
    /// <param name="sessionId">Session id</param>
    /// <returns>client response</returns>
    public void RegisterProduction(PmcData pmcData, HideoutSingleProductionStartRequestData productionRequest, MongoId sessionId)
    {
        var recipe = databaseService
            .GetHideout()
            .Production.Recipes?.FirstOrDefault(production => production.Id == productionRequest.RecipeId);

        if (recipe is null)
        {
            logger.Error(serverLocalisationService.GetText("hideout-missing_recipe_in_db", productionRequest.RecipeId));

            httpResponseUtil.AppendErrorToOutput(eventOutputHolder.GetOutput(sessionId));
            return;
        }

        if (pmcData.Hideout is null)
        {
            var message = $"Hideout is null when trying to register production for recipe id `{recipe.Id}`";
            logger.Error(message);
            throw new HideoutHelperException(message);
        }

        // @Important: Here we need to be very exact:
        // - normal recipe: Production time value is stored in attribute "productionType" with small "p"
        // - scav case recipe: Production time value is stored in attribute "ProductionType" with capital "P"
        pmcData.Hideout.Production ??= [];

        var modifiedProductionTime = GetAdjustedCraftTimeWithSkills(pmcData, productionRequest.RecipeId);

        var production = InitProduction(productionRequest.RecipeId, modifiedProductionTime ?? 0, recipe.NeedFuelForAllProductionTime);

        // Store the tools used for this production, so we can return them later
        if (productionRequest.Tools?.Count > 0)
        {
            production.SptRequiredTools = [];

            foreach (var tool in productionRequest.Tools)
            {
                var toolItem = cloner.Clone(pmcData.Inventory?.Items?.FirstOrDefault(x => x.Id == tool.Id));
                if (toolItem is null)
                {
                    logger.Warning($"Unable to find tool item: {tool.Id}");

                    continue;
                }

                // Make sure we only return as many as we took
                toolItem.AddUpd();

                toolItem.Upd!.StackObjectsCount = tool.Count;

                production.SptRequiredTools.Add(
                    new Item
                    {
                        Id = new MongoId(),
                        Template = toolItem.Template,
                        Upd = toolItem.Upd,
                    }
                );
            }
        }

        pmcData.Hideout.Production[productionRequest.RecipeId] = production;
    }

    /// <summary>
    ///     Add production to profiles' Hideout.Production array
    /// </summary>
    /// <param name="pmcData">Profile to add production to</param>
    /// <param name="productionRequest">Production request</param>
    /// <param name="sessionId">Session id</param>
    /// <returns>client response</returns>
    public void RegisterProduction(PmcData pmcData, HideoutContinuousProductionStartRequestData productionRequest, MongoId sessionId)
    {
        if (!productionRequest.RecipeId.HasValue)
        {
            logger.Error("RecipeId sent from client is null, skipping continuous production registration");
            return;
        }

        var recipe = databaseService
            .GetHideout()
            .Production.Recipes?.FirstOrDefault(production => production.Id == productionRequest.RecipeId);
        if (recipe is null)
        {
            logger.Error(serverLocalisationService.GetText("hideout-missing_recipe_in_db", productionRequest.RecipeId));

            httpResponseUtil.AppendErrorToOutput(eventOutputHolder.GetOutput(sessionId));
            return;
        }

        if (pmcData.Hideout is null)
        {
            var message = $"Hideout is null when trying to register production for recipe id `{productionRequest.RecipeId.Value}`";
            logger.Error(message);
            throw new HideoutHelperException(message);
        }

        // @Important: Here we need to be very exact:
        // - normal recipe: Production time value is stored in attribute "productionType" with small "p"
        // - scav case recipe: Production time value is stored in attribute "ProductionType" with capital "P"
        pmcData.Hideout.Production ??= [];

        var modifiedProductionTime = GetAdjustedCraftTimeWithSkills(pmcData, productionRequest.RecipeId.Value);

        var production = InitProduction(productionRequest.RecipeId.Value, modifiedProductionTime ?? 0, recipe.NeedFuelForAllProductionTime);

        pmcData.Hideout.Production[productionRequest.RecipeId.Value] = production;
    }

    /// <summary>
    ///     This convenience function initializes new Production Object
    ///     with all the constants.
    /// </summary>
    public Production InitProduction(MongoId recipeId, double productionTime, bool? needFuelForAllProductionTime)
    {
        return new Production
        {
            Progress = 0,
            InProgress = true,
            RecipeId = recipeId,
            StartTimestamp = timeUtil.GetTimeStamp(),
            ProductionTime = productionTime,
            Products = [],
            GivenItemsInStart = [],
            Interrupted = false,
            NeedFuelForAllProductionTime = needFuelForAllProductionTime, // Used when sending to client
            needFuelForAllProductionTime = needFuelForAllProductionTime, // used when stored in production.json
            SkipTime = 0,
        };
    }

    /// <summary>
    ///     Apply bonus to player profile given after completing hideout upgrades
    /// </summary>
    /// <param name="profileData">Profile to add bonus to</param>
    /// <param name="bonus">Bonus to add to profile</param>
    public void ApplyPlayerUpgradesBonus(PmcData profileData, Bonus bonus)
    {
        // Handle additional changes some bonuses need before being added
        switch (bonus.Type)
        {
            case BonusType.StashSize:
            {
                // Find stash item and adjust tpl to new tpl from bonus
                var stashItem = profileData.Inventory?.Items?.FirstOrDefault(x => x.Id == profileData.Inventory.Stash);
                if (stashItem is null)
                {
                    logger.Error(
                        serverLocalisationService.GetText(
                            "hideout-unable_to_apply_stashsize_bonus_no_stash_found",
                            profileData.Inventory?.Stash
                        )
                    );

                    return;
                }

                if (bonus.TemplateId.HasValue)
                {
                    stashItem.Template = bonus.TemplateId.Value;
                    break;
                }

                logger.Error("Bonus template id is null when trying to apply stash size bonus");
                break;
            }
            case BonusType.MaximumEnergyReserve:
                if (profileData.Health?.Energy is null)
                {
                    const string message = "Profile Energy is null when trying to apply MaximumEnergyReserve";
                    logger.Error(message);
                    throw new HideoutHelperException(message);
                }

                // Amend max energy in profile
                profileData.Health.Energy.Maximum += bonus.Value;
                break;
            case BonusType.TextBonus:
                // Delete values before they're added to profile
                bonus.IsPassive = null;
                bonus.IsProduction = null;
                bonus.IsVisible = null;
                break;
            default:
                if (logger.IsLogEnabled(LogLevel.Debug))
                {
                    logger.Debug($"Unhandled bonus type `{bonus.Type}` when trying to apply player upgrade bonus");
                }
                break;
        }

        if (profileData.Bonuses is null)
        {
            var message = $"Profile bonuses are null when trying to add: {bonus.Type}";
            logger.Error(message);
            throw new HideoutHelperException(message);
        }

        // Add bonus to player bonuses array in profile
        // EnergyRegeneration, HealthRegeneration, RagfairCommission, ScavCooldownTimer, SkillGroupLevelingBoost, ExperienceRate, QuestMoneyReward etc
        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug($"Adding bonus: {bonus.Type} to profile, value: {bonus.Value}");
        }

        profileData.Bonuses.Add(bonus);
    }

    /// <summary>
    ///     Process a players hideout, update areas that use resources + increment production timers
    /// </summary>
    /// <param name="sessionID">Session id</param>
    public void UpdatePlayerHideout(MongoId sessionID)
    {
        var pmcData = profileHelper.GetPmcProfile(sessionID)!;
        var hideoutProperties = GetHideoutProperties(pmcData);

        if (pmcData.Hideout is null)
        {
            const string message = "Hideout is null when trying to update player hideout";
            logger.Error(message);
            throw new HideoutHelperException(message);
        }

        pmcData.Hideout.SptUpdateLastRunTimestamp ??= timeUtil.GetTimeStamp();

        UpdateAreasWithResources(sessionID, pmcData, hideoutProperties);
        UpdateProductionTimers(pmcData, hideoutProperties);
        pmcData.Hideout.SptUpdateLastRunTimestamp = timeUtil.GetTimeStamp();
    }

    /// <summary>
    ///     Get various properties that will be passed to hideout update-related functions
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <returns>Hideout-related values</returns>
    protected HideoutProperties GetHideoutProperties(PmcData pmcData)
    {
        var bitcoinFarm = pmcData.Hideout?.Areas?.FirstOrDefault(area => area.Type == HideoutAreas.BitcoinFarm);
        var bitcoinCount = bitcoinFarm?.Slots?.Count(slot => slot.Items is not null); // Get slots with an item property
        var waterCollector = pmcData.Hideout?.Areas?.FirstOrDefault(area => area.Type == HideoutAreas.WaterCollector);

        var hideoutProperties = new HideoutProperties
        {
            BtcFarmGcs = bitcoinCount,
            IsGeneratorOn = pmcData.Hideout?.Areas?.FirstOrDefault(area => area.Type == HideoutAreas.Generator)?.Active ?? false,
            WaterCollectorHasFilter = DoesWaterCollectorHaveFilter(waterCollector),
        };

        return hideoutProperties;
    }

    /// <summary>
    ///     Does a water collection hideout area have a water filter installed
    /// </summary>
    /// <param name="waterCollector">Hideout area to check</param>
    /// <returns></returns>
    protected bool DoesWaterCollectorHaveFilter(BotHideoutArea? waterCollector)
    {
        // Water collector not built
        if (waterCollector is null)
        {
            return false;
        }

        // Can put filters in from L3
        if (waterCollector.Level == 3)
        // Has filter in at least one slot
        {
            return waterCollector.Slots?.Any(slot => slot.Items is not null) ?? false;
        }

        // No Filter
        return false;
    }

    /// <summary>
    ///     Iterate over productions and update their progress timers
    /// </summary>
    /// <param name="pmcData">Profile to check for productions and update</param>
    /// <param name="hideoutProperties">Hideout properties</param>
    protected void UpdateProductionTimers(PmcData pmcData, HideoutProperties hideoutProperties)
    {
        var recipes = databaseService.GetHideout().Production;

        // Check each production and handle edge cases if necessary
        foreach (var prodId in pmcData.Hideout?.Production ?? [])
        {
            // Pattern matching null or false to shut the compiler up
            if (pmcData.Hideout?.Production?.TryGetValue(prodId.Key, out var craft) is null or false)
            {
                // Craft value is undefined, get rid of it (could be from cancelling craft that needs cleaning up)
                pmcData.Hideout?.Production?.Remove(prodId.Key);

                continue;
            }

            if (craft?.Progress is null)
            {
                logger.Warning(serverLocalisationService.GetText("hideout-craft_has_undefined_progress_value_defaulting", prodId));
                craft!.Progress = 0;
            }

            // Skip processing (Don't skip continuous crafts like bitcoin farm or cultist circle)
            if (craft.IsCraftComplete())
            {
                continue;
            }

            // Some crafts (that need continuous power) can be interrupted
            if (craft.Interrupted.GetValueOrDefault(false))
            {
                continue;
            }

            // Special handling required
            if (craft.IsCraftOfType(HideoutAreas.ScavCase))
            {
                UpdateScavCaseProductionTimer(pmcData, prodId.Key);

                continue;
            }

            if (craft.IsCraftOfType(HideoutAreas.WaterCollector))
            {
                UpdateWaterCollectorProductionTimer(pmcData, prodId.Key, hideoutProperties);

                continue;
            }

            // Continuous craft
            if (craft.IsCraftOfType(HideoutAreas.BitcoinFarm))
            {
                UpdateBitcoinFarm(
                    pmcData,
                    pmcData.Hideout.Production.GetValueOrDefault(prodId.Key),
                    hideoutProperties.BtcFarmGcs,
                    hideoutProperties.IsGeneratorOn
                );

                continue;
            }

            // No recipe, needs special handling
            if (craft.IsCraftOfType(HideoutAreas.CircleOfCultists))
            {
                UpdateCultistCircleCraftProgress(pmcData, prodId.Key);

                continue;
            }

            // Ensure recipe exists before using it in updateProductionProgress()
            var recipe = recipes?.Recipes?.FirstOrDefault(r => r.Id == prodId.Key);
            if (recipe is null)
            {
                logger.Error(serverLocalisationService.GetText("hideout-missing_recipe_for_area", prodId));

                continue;
            }

            UpdateProductionProgress(pmcData, prodId.Key, recipe, hideoutProperties);
        }
    }

    /// <summary>
    ///     Update progress timer for water collector
    /// </summary>
    /// <param name="pmcData">profile to update</param>
    /// <param name="productionId">id of water collection production to update</param>
    /// <param name="hideoutProperties">Hideout properties</param>
    protected void UpdateWaterCollectorProductionTimer(PmcData pmcData, MongoId productionId, HideoutProperties hideoutProperties)
    {
        if (pmcData.Hideout?.Production is null)
        {
            const string message = "Hideout productions are null when trying to update water collector production timer";
            logger.Error(message);
            throw new HideoutHelperException(message);
        }

        if (!pmcData.Hideout.Production.TryGetValue(productionId, out var production) || production is null)
        {
            logger.Error($"production id: {productionId.ToString()} not found in hideout productions, what are we trying to update?");
            return;
        }

        var timeElapsed = GetTimeElapsedSinceLastServerTick(pmcData, hideoutProperties.IsGeneratorOn);

        if (hideoutProperties.WaterCollectorHasFilter)
        {
            production.Progress += timeElapsed;
        }
    }

    /// <summary>
    ///     Update a productions progress value based on the amount of time that has passed
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="prodId">Production id being crafted</param>
    /// <param name="recipe">Recipe data being crafted</param>
    /// <param name="hideoutProperties"></param>
    protected void UpdateProductionProgress(PmcData pmcData, MongoId prodId, HideoutProduction recipe, HideoutProperties hideoutProperties)
    {
        if (pmcData.Hideout?.Production is null)
        {
            const string message = "Hideout productions are null when trying to update production";
            logger.Error(message);
            throw new HideoutHelperException(message);
        }

        // Production is complete, no need to do any calculations
        if (DoesProgressMatchProductionTime(pmcData, prodId))
        {
            return;
        }

        // Get seconds since last hideout update + now
        var timeElapsed = GetTimeElapsedSinceLastServerTick(pmcData, hideoutProperties.IsGeneratorOn, recipe);

        // Increment progress by time passed
        if (!pmcData.Hideout.Production.TryGetValue(prodId, out var production) || production is null)
        {
            logger.Error($"production id: {prodId.ToString()} not found in hideout productions, what are we trying to update?");
            return;
        }

        // Some items NEED power to craft (e.g. DSP)
        if (production.needFuelForAllProductionTime.GetValueOrDefault() && hideoutProperties.IsGeneratorOn)
        {
            production.Progress += timeElapsed;
        }
        else if (!production.needFuelForAllProductionTime.GetValueOrDefault())
        // Increment progress if production does not necessarily need fuel to continue
        {
            production.Progress += timeElapsed;
        }

        // Limit progress to total production time if progress is over (don't run for continuous crafts)
        if (!(recipe.Continuous ?? false))
        // If progress is larger than prod time, return ProductionTime, hard cap the value
        {
            production.Progress = Math.Min(production.Progress ?? 0, production.ProductionTime ?? 0);
        }
    }

    protected void UpdateCultistCircleCraftProgress(PmcData pmcData, MongoId prodId)
    {
        if (pmcData.Hideout?.Production is null)
        {
            const string message = "Hideout productions are null when trying to update cultist progress";
            logger.Error(message);
            throw new HideoutHelperException(message);
        }

        if (!pmcData.Hideout.Production.TryGetValue(prodId, out var production) || production is null)
        {
            logger.Error($"Production id `{prodId.ToString()}` not found in profile, what are we trying to update?");
            return;
        }

        // Check if we're already complete, skip
        if ((production.AvailableForFinish ?? false) && !production.InProgress.GetValueOrDefault(false))
        {
            return;
        }

        // Get seconds since last hideout update
        var timeElapsedSeconds = timeUtil.GetTimeStamp() - pmcData.Hideout.SptUpdateLastRunTimestamp;

        // Increment progress by time passed if progress is less than time needed
        if (production.Progress < production.ProductionTime)
        {
            production.Progress += timeElapsedSeconds;

            // Check if craft is complete
            if (production.Progress >= production.ProductionTime)
            {
                production.FlagCultistCircleCraftAsComplete();
            }

            return;
        }

        // Craft is complete
        production.FlagCultistCircleCraftAsComplete();
    }

    /// <summary>
    ///     Check if a productions progress value matches its corresponding recipes production time value
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="prodId">Production id</param>
    /// <returns>progress matches productionTime from recipe</returns>
    protected bool DoesProgressMatchProductionTime(PmcData pmcData, MongoId prodId)
    {
        if (pmcData.Hideout?.Production is null)
        {
            const string message = "Hideout productions are null when trying to update production";
            logger.Error(message);
            throw new HideoutHelperException(message);
        }

        // Production doesn't exist or progress or production time is null
        if (
            !pmcData.Hideout.Production.TryGetValue(prodId, out var production)
            || production?.Progress is null
            || production.ProductionTime is null
        )
        {
            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug($"ProductionId: {prodId} Trying to match progress to production time that does not exist.");
            }

            return false;
        }

        return production.Progress.Value.Approx(production.ProductionTime.Value);
    }

    /// <summary>
    ///     Update progress timer for scav case
    /// </summary>
    /// <param name="pmcData">Profile to update</param>
    /// <param name="productionId">Id of scav case production to update</param>
    protected void UpdateScavCaseProductionTimer(PmcData pmcData, MongoId productionId)
    {
        if (pmcData.Hideout?.Production is null)
        {
            const string message = "Hideout productions are null when trying to update scav case";
            logger.Error(message);
            throw new HideoutHelperException(message);
        }

        if (!pmcData.Hideout.Production.TryGetValue(productionId, out var production) || production is null)
        {
            logger.Error($"Production id `{productionId.ToString()}` not found in profile, what are we trying to update?");
            return;
        }

        var currentTime = timeUtil.GetTimeStamp();
        var timeElapsed = currentTime - production.StartTimestamp - production.Progress;

        production.Progress += timeElapsed;
    }

    /// <summary>
    ///     Iterate over hideout areas that use resources (fuel/filters etc) and update associated values
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="pmcData">Profile to update areas of</param>
    /// <param name="hideoutProperties">hideout properties</param>
    protected void UpdateAreasWithResources(MongoId sessionId, PmcData pmcData, HideoutProperties hideoutProperties)
    {
        if (pmcData.Hideout is null)
        {
            const string message = "Hideout is null when trying update areas with resources";
            logger.Error(message);
            throw new HideoutHelperException(message);
        }

        var areas = GetAreasWithResourceUse(pmcData.Hideout.Areas ?? []);
        foreach (var area in areas)
        {
            switch (area.Type)
            {
                case HideoutAreas.Generator:
                    if (hideoutProperties.IsGeneratorOn)
                    {
                        UpdateFuel(area, pmcData, hideoutProperties.IsGeneratorOn);
                    }
                    break;
                case HideoutAreas.WaterCollector:
                    UpdateWaterCollector(sessionId, pmcData, area, hideoutProperties);
                    break;
                case HideoutAreas.AirFilteringUnit:
                    if (hideoutProperties.IsGeneratorOn)
                    {
                        UpdateAirFilters(area, pmcData, hideoutProperties.IsGeneratorOn);
                    }
                    break;
                default:
                    if (logger.IsLogEnabled(LogLevel.Debug))
                    {
                        logger.Debug($"Unhandled area type: {area.Type} when trying to update areas with resources");
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Get Hideout areas that consume resources
    /// </summary>
    /// <param name="hideoutAreas">Areas to filter</param>
    /// <returns>Collection of hideout areas</returns>
    protected IEnumerable<BotHideoutArea> GetAreasWithResourceUse(List<BotHideoutArea> hideoutAreas)
    {
        HashSet<HideoutAreas> resourceUseAreas = [HideoutAreas.Generator, HideoutAreas.WaterCollector, HideoutAreas.AirFilteringUnit];
        return hideoutAreas.Where(area => resourceUseAreas.Contains(area.Type));
    }

    /// <summary>
    ///     Decrease fuel from generator slots based on amount of time since last time this occurred
    /// </summary>
    /// <param name="generatorArea">Hideout area</param>
    /// <param name="pmcData">Player profile</param>
    /// <param name="isGeneratorOn">Is the generator turned on since last update</param>
    protected void UpdateFuel(BotHideoutArea generatorArea, PmcData pmcData, bool isGeneratorOn)
    {
        // 1 resource last 14 min 27 sec, 1/14.45/60 = 0.00115
        // 10-10-2021 From wiki, 1 resource last 12 minutes 38 seconds, 1/12.63333/60 = 0.00131
        var fuelUsedSinceLastTick =
            databaseService.GetHideout().Settings.GeneratorFuelFlowRate * GetTimeElapsedSinceLastServerTick(pmcData, isGeneratorOn);

        // Get all fuel consumption bonuses, returns an empty array if none found
        var profileFuelConsomptionBonusSum = pmcData.GetBonusValueFromProfile(BonusType.FuelConsumption);

        // An increase in "bonus" consumption is actually an increase in consumption, so invert this for later use
        var fuelConsumptionBonusRate = -(profileFuelConsomptionBonusSum / 100);

        // An increase in hideout management bonus is a decrease in consumption
        var hideoutManagementConsumptionBonusRate = GetHideoutManagementConsumptionBonus(pmcData);

        var combinedBonus = 1.0 - (fuelConsumptionBonusRate + hideoutManagementConsumptionBonusRate);

        // Sanity check, never let fuel consumption go negative, otherwise it returns fuel to the player
        if (combinedBonus < 0)
        {
            combinedBonus = 0;
        }

        fuelUsedSinceLastTick *= combinedBonus;

        var hasFuelRemaining = false;
        for (var i = 0; i < generatorArea.Slots?.Count; i++)
        {
            double pointsConsumed;

            var generatorSlot = generatorArea.Slots[i];
            if (generatorSlot?.Items is null)
            // No item in slot, skip
            {
                continue;
            }

            var fuelItemInSlot = generatorSlot?.Items.FirstOrDefault();
            if (fuelItemInSlot is null)
            // No item in slot, skip
            {
                continue;
            }

            var fuelRemaining = fuelItemInSlot.Upd?.Resource?.Value;

            switch (fuelRemaining)
            {
                // No fuel left, skip
                case 0:
                    continue;
                // Undefined fuel, fresh fuel item and needs its max fuel amount looked up
                case null:
                {
                    var fuelItemTemplate = itemHelper.GetItem(fuelItemInSlot.Template).Value;
                    pointsConsumed = fuelUsedSinceLastTick ?? 0;
                    fuelRemaining = fuelItemTemplate.Properties.MaxResource - fuelUsedSinceLastTick;
                    break;
                }
                default:
                    // Fuel exists already, deduct fuel from item remaining value
                    pointsConsumed = (double)((fuelItemInSlot.Upd.Resource.UnitsConsumed ?? 0) + fuelUsedSinceLastTick);
                    fuelRemaining -= fuelUsedSinceLastTick;
                    break;
            }

            // Round values to keep accuracy
            fuelRemaining = Math.Round(fuelRemaining * 10000 ?? 0) / 10000;
            pointsConsumed = Math.Round(pointsConsumed * 10000) / 10000;

            // Fuel consumed / 10 is over 1, add hideout management skill point
            if (pmcData is not null && Math.Floor(pointsConsumed / 10) >= 1)
            {
                profileHelper.AddSkillPointsToPlayer(pmcData, SkillTypes.HideoutManagement, 1);
                pointsConsumed -= 10;
            }

            var isFuelItemFoundInRaid = fuelItemInSlot.Upd?.SpawnedInSession ?? false;
            if (fuelRemaining > 0)
            {
                // Deducted all used fuel from this container, clean up and exit loop
                fuelItemInSlot.Upd = GetAreaUpdObject(1, fuelRemaining, pointsConsumed, isFuelItemFoundInRaid);

                if (logger.IsLogEnabled(LogLevel.Debug))
                {
                    logger.Debug($"Profile: {pmcData!.Id} Generator has: {fuelRemaining} fuel left in slot {i + 1}");
                }

                hasFuelRemaining = true;

                break; // Break to avoid updating all the fuel tanks
            }

            fuelItemInSlot.Upd = GetAreaUpdObject(1, 0, 0, isFuelItemFoundInRaid);

            // Ran out of fuel items to deduct fuel from
            fuelUsedSinceLastTick = Math.Abs(fuelRemaining ?? 0);
            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug($"Profile: {pmcData!.Id} Generator ran out of fuel");
            }
        }

        // Out of fuel, flag generator as offline
        if (!hasFuelRemaining)
        {
            generatorArea.Active = false;
        }
    }

    protected void UpdateWaterCollector(MongoId sessionId, PmcData pmcData, BotHideoutArea area, HideoutProperties hideoutProperties)
    {
        // Skip water collector when not level 3 (cant collect until 3)
        if (area.Level != 3)
        {
            return;
        }

        if (!hideoutProperties.WaterCollectorHasFilter)
        {
            return;
        }

        // Canister with purified water craft exists
        if (
            pmcData.Hideout?.Production?.TryGetValue(WaterCollectorId, out var purifiedWaterCraft) is true
            && purifiedWaterCraft?.GetType() == typeof(Production)
        )
        {
            // Update craft time to account for increases in players craft time skill
            purifiedWaterCraft.ProductionTime = GetAdjustedCraftTimeWithSkills(pmcData, purifiedWaterCraft.RecipeId, true);

            UpdateWaterFilters(area, purifiedWaterCraft, hideoutProperties.IsGeneratorOn, pmcData);
        }
        else
        {
            // continuousProductionStart()
            // seem to not trigger consistently
            var recipe = new HideoutSingleProductionStartRequestData
            {
                RecipeId = WaterCollectorId,
                Action = "HideoutSingleProductionStart",
                Items = [],
                Tools = [],
                Timestamp = timeUtil.GetTimeStamp(),
            };

            RegisterProduction(pmcData, recipe, sessionId);
        }
    }

    /// <summary>
    ///     Get craft time and make adjustments to account for dev profile + crafting skill level
    /// </summary>
    /// <param name="pmcData">Player profile making craft</param>
    /// <param name="recipeId">Recipe being crafted</param>
    /// <param name="applyHideoutManagementBonus">Should the hideout management bonus be applied to the calculation</param>
    /// <returns>Items craft time with bonuses subtracted</returns>
    public double? GetAdjustedCraftTimeWithSkills(PmcData pmcData, MongoId recipeId, bool applyHideoutManagementBonus = false)
    {
        var globalSkillsDb = databaseService.GetGlobals().Configuration.SkillsSettings;

        var recipe = databaseService.GetHideout().Production.Recipes?.FirstOrDefault(production => production.Id == recipeId);
        if (recipe is null)
        {
            logger.Error(serverLocalisationService.GetText("hideout-missing_recipe_in_db", recipeId));

            return null;
        }

        var timeReductionSeconds = 0D;

        // Bitcoin farm is excluded from crafting skill cooldown reduction
        if (recipeId != BitcoinProductionId)
        // Seconds to deduct from crafts total time
        {
            timeReductionSeconds += GetSkillProductionTimeReduction(
                pmcData,
                recipe.ProductionTime ?? 0,
                SkillTypes.Crafting,
                globalSkillsDb.Crafting.ProductionTimeReductionPerLevel
            );
        }

        // Some crafts take into account hideout management, e.g. fuel, water/air filters
        if (applyHideoutManagementBonus)
        {
            timeReductionSeconds += GetSkillProductionTimeReduction(
                pmcData,
                recipe.ProductionTime ?? 0,
                SkillTypes.HideoutManagement,
                globalSkillsDb.HideoutManagement.ConsumptionReductionPerLevel
            );
        }

        var modifiedProductionTime = recipe.ProductionTime - timeReductionSeconds;
        if (modifiedProductionTime > 0 && profileHelper.IsDeveloperAccount(pmcData.Id!.Value))
        {
            modifiedProductionTime = 40;
        }

        // Sanity check, don't let anything craft in less than 5 seconds
        if (modifiedProductionTime < 5)
        {
            modifiedProductionTime = 5;
        }

        return modifiedProductionTime;
    }

    /// <summary>
    ///     Adjust water filter objects resourceValue or delete when they reach 0 resource
    /// </summary>
    /// <param name="waterFilterArea">Water filter area to update</param>
    /// <param name="production">Production object</param>
    /// <param name="isGeneratorOn">Is generator enabled</param>
    /// <param name="pmcData">Player profile</param>
    protected void UpdateWaterFilters(BotHideoutArea waterFilterArea, Production production, bool isGeneratorOn, PmcData pmcData)
    {
        var filterDrainRate = GetWaterFilterDrainRate(pmcData);
        var craftProductionTime = GetTotalProductionTimeSeconds(WaterCollectorId);
        var secondsSinceServerTick = GetTimeElapsedSinceLastServerTick(pmcData, isGeneratorOn);

        filterDrainRate = GetTimeAdjustedWaterFilterDrainRate(
            secondsSinceServerTick ?? 0,
            craftProductionTime,
            production.Progress ?? 0,
            filterDrainRate
        );

        // Check progress against the productions craft time (don't use base time as it doesn't include any time bonuses profile has)
        if (production.Progress > production.ProductionTime)
        // Craft is complete nothing to do
        {
            return;
        }

        // Check all slots that take water filters until we find one with filter in it
        for (var i = 0; i < waterFilterArea.Slots?.Count; i++)
        {
            // No water filter in slot, skip
            if (waterFilterArea.Slots[i].Items is null)
            {
                continue;
            }

            var waterFilterItemInSlot = waterFilterArea.Slots[i].Items?.FirstOrDefault();
            if (waterFilterItemInSlot is null)
            {
                logger.Warning($"Could not find water filter in slot index `{i}` when trying to update water filters");
                continue;
            }

            // How many units of filter are left
            var resourceValue = waterFilterItemInSlot.Upd?.Resource?.Value;
            double pointsConsumed;
            if (resourceValue is null)
            {
                // Missing, is new filter, add default and subtract usage
                resourceValue = 100 - filterDrainRate;
                pointsConsumed = filterDrainRate;
            }
            else
            {
                pointsConsumed = (waterFilterItemInSlot.Upd?.Resource?.UnitsConsumed ?? 0) + filterDrainRate;
                resourceValue -= filterDrainRate;
            }

            // Round to get values to 3dp
            resourceValue = Math.Round(resourceValue * 1000 ?? 0) / 1000;
            pointsConsumed = Math.Round(pointsConsumed * 1000) / 1000;

            // Check units consumed for possible increment of hideout mgmt skill point
            if (pmcData is not null && Math.Floor(pointsConsumed / 10) >= 1)
            {
                profileHelper.AddSkillPointsToPlayer(pmcData, SkillTypes.HideoutManagement, 1);
                pointsConsumed -= 10;
            }

            // Filter has some fuel left in it after our adjustment
            if (resourceValue > 0)
            {
                var isWaterFilterFoundInRaid = waterFilterItemInSlot.Upd?.SpawnedInSession ?? false;

                // Set filters consumed amount
                waterFilterItemInSlot.Upd = GetAreaUpdObject(1, resourceValue, pointsConsumed, isWaterFilterFoundInRaid);
                if (logger.IsLogEnabled(LogLevel.Debug))
                {
                    logger.Debug($"Water filter has: {resourceValue} units left in slot {i + 1}");
                }

                break; // Break here to avoid iterating other filters now we're done
            }

            // Filter ran out / used up
            waterFilterArea.Slots[i].Items = null;
            // Update remaining resources to be subtracted
            filterDrainRate = Math.Abs(resourceValue ?? 0);
        }
    }

    /// <summary>
    ///     Get an adjusted water filter drain rate based on time elapsed since last run,
    ///     handle edge case when craft time has gone on longer than total production time
    /// </summary>
    /// <param name="secondsSinceServerTick">Time passed</param>
    /// <param name="totalProductionTime">Total time collecting water</param>
    /// <param name="productionProgress">How far water collector has progressed</param>
    /// <param name="baseFilterDrainRate">Base drain rate</param>
    /// <returns>Drain rate (adjusted)</returns>
    protected static double GetTimeAdjustedWaterFilterDrainRate(
        long secondsSinceServerTick,
        double totalProductionTime,
        double productionProgress,
        double baseFilterDrainRate
    )
    {
        var drainTimeSeconds =
            secondsSinceServerTick > totalProductionTime
                ? totalProductionTime - productionProgress // More time passed than prod time, get total minus the current progress
                : secondsSinceServerTick;

        // Multiply base drain rate by time passed
        return baseFilterDrainRate * drainTimeSeconds;
    }

    /// <summary>
    ///     Get the water filter drain rate based on hideout bonuses player has
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <returns>Drain rate</returns>
    protected double GetWaterFilterDrainRate(PmcData pmcData)
    {
        var globalSkillsDb = databaseService.GetGlobals().Configuration.SkillsSettings;

        // 100 resources last 8 hrs 20 min, 100/8.33/60/60 = 0.00333
        const double filterDrainRate = 0.00333d;

        var hideoutManagementConsumptionBonus = pmcData.GetSkillBonusMultipliedBySkillLevel(
            SkillTypes.HideoutManagement,
            globalSkillsDb.HideoutManagement.ConsumptionReductionPerLevel
        );
        var craftSkillTimeReductionMultiplier = pmcData.GetSkillBonusMultipliedBySkillLevel(
            SkillTypes.Crafting,
            globalSkillsDb.Crafting.CraftTimeReductionPerLevel
        );

        // Never let bonus become 0
        var reductionBonus =
            hideoutManagementConsumptionBonus + craftSkillTimeReductionMultiplier == 0
                ? 1
                : 1 - (hideoutManagementConsumptionBonus + craftSkillTimeReductionMultiplier);

        return filterDrainRate * reductionBonus;
    }

    /// <summary>
    ///     Get the production time in seconds for the desired production
    /// </summary>
    /// <param name="prodId">Id, e.g. Water collector id</param>
    /// <returns>Seconds to produce item</returns>
    protected double GetTotalProductionTimeSeconds(MongoId prodId)
    {
        return databaseService.GetHideout().Production.Recipes?.FirstOrDefault(prod => prod.Id == prodId)?.ProductionTime ?? 0;
    }

    /// <summary>
    ///     Create an upd object using passed in parameters
    /// </summary>
    /// <param name="stackCount"></param>
    /// <param name="resourceValue"></param>
    /// <param name="resourceUnitsConsumed"></param>
    /// <param name="isFoundInRaid"></param>
    /// <returns>Upd</returns>
    protected Upd GetAreaUpdObject(double stackCount, double? resourceValue, double resourceUnitsConsumed, bool isFoundInRaid)
    {
        return new Upd
        {
            StackObjectsCount = stackCount,
            Resource = new UpdResource { Value = resourceValue, UnitsConsumed = resourceUnitsConsumed },
            SpawnedInSession = isFoundInRaid,
        };
    }

    protected void UpdateAirFilters(BotHideoutArea airFilterArea, PmcData pmcData, bool isGeneratorOn)
    {
        // 300 resources last 20 hrs, 300/20/60/60 = 0.00416
        // 10-10-2021 from WIKI (https://escapefromtarkov.fandom.com/wiki/FP-100_filter_absorber)
        //   Lasts for 17 hours 38 minutes and 49 seconds (23 hours 31 minutes and 45 seconds with elite hideout management skill),
        //   300/17.64694/60/60 = 0.004722
        var filterDrainRate =
            databaseService.GetHideout().Settings.AirFilterUnitFlowRate * GetTimeElapsedSinceLastServerTick(pmcData, isGeneratorOn);

        // Hideout management resource consumption bonus:
        var hideoutManagementConsumptionBonus = 1.0 - GetHideoutManagementConsumptionBonus(pmcData);
        filterDrainRate *= hideoutManagementConsumptionBonus;

        for (var i = 0; i < airFilterArea.Slots?.Count; i++)
        {
            if (airFilterArea.Slots[i].Items is null)
            {
                continue;
            }

            var resourceValue = airFilterArea.Slots[i].Items?[0].Upd?.Resource is not null
                ? airFilterArea.Slots[i].Items?[0].Upd?.Resource?.Value
                : null;

            double pointsConsumed;
            if (resourceValue is null)
            {
                resourceValue = 300 - filterDrainRate;
                pointsConsumed = filterDrainRate ?? 0;
            }
            else
            {
                pointsConsumed = (airFilterArea.Slots[i].Items?[0].Upd?.Resource?.UnitsConsumed ?? 0) + filterDrainRate ?? 0;
                resourceValue -= filterDrainRate;
            }

            resourceValue = Math.Round(resourceValue * 10000 ?? 0) / 10000;
            pointsConsumed = Math.Round(pointsConsumed * 10000) / 10000;

            // check unit consumed for increment skill point
            if (pmcData is not null && Math.Floor(pointsConsumed / 10) >= 1)
            {
                profileHelper.AddSkillPointsToPlayer(pmcData, SkillTypes.HideoutManagement, 1);
                pointsConsumed -= 10;
            }

            if (resourceValue > 0)
            {
                airFilterArea.Slots[i].Items![0].Upd = new Upd
                {
                    StackObjectsCount = 1,
                    Resource = new UpdResource { Value = resourceValue, UnitsConsumed = pointsConsumed },
                };
                if (logger.IsLogEnabled(LogLevel.Debug))
                {
                    logger.Debug($"Air filter: {resourceValue} filter left on slot {i + 1}");
                }

                break; // Break here to avoid updating all filters
            }

            airFilterArea.Slots[i].Items = null;
            // Update remaining resources to be subtracted
            filterDrainRate = Math.Abs(resourceValue ?? 0);
        }
    }

    /// <summary>
    /// Increment bitcoin farm progress
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="btcProduction">Hideout btc craft</param>
    /// <param name="btcFarmCGs"></param>
    /// <param name="isGeneratorOn">Is hideout generator powered</param>
    protected void UpdateBitcoinFarm(PmcData pmcData, Production? btcProduction, int? btcFarmCGs, bool isGeneratorOn)
    {
        if (btcProduction is null)
        {
            logger.Error(serverLocalisationService.GetText("hideout-bitcoin_craft_missing"));

            return;
        }

        var isBtcProd = btcProduction.GetType() == typeof(Production);
        if (!isBtcProd)
        {
            return;
        }

        // The wiki has a wrong formula!
        // Do not change unless you validate it with the Client code files!
        // This formula was found on the client files:
        // *******************************************************
        /*
                public override int InstalledSuppliesCount
             {
              get
              {
               return this.int_1;
              }
              protected set
              {
               if (this.int_1 === value)
                        {
                            return;
                        }
                        this.int_1 = value;
                        base.Single_0 = ((this.int_1 === 0) ? 0f : (1f + (float)(this.int_1 - 1) * this.float_4));
                    }
                }
            */
        // **********************************************************
        // At the time of writing this comment, this was GClass1667
        // To find it in case of weird results, use DNSpy and look for usages on class AreaData
        // Look for a GClassXXXX that has a method called "InitDetails" and the only parameter is the AreaData
        // That should be the bitcoin farm production. To validate, try to find the snippet below:
        /*
                protected override void InitDetails(AreaData data)
                {
                    base.InitDetails(data);
                    this.gclass1678_1.Type = EDetailsType.Farming;
                }
            */
        // Needs power to function
        if (!isGeneratorOn)
        // Return with no changes
        {
            return;
        }

        var coinSlotCount = GetBTCSlots(pmcData);

        // Full of bitcoins, halt progress
        if (btcProduction.Products?.Count >= coinSlotCount)
        {
            // Set progress to 0
            btcProduction.Progress = 0;

            return;
        }

        var bitcoinProdData = databaseService
            .GetHideout()
            .Production.Recipes?.FirstOrDefault(production => production.Id == BitcoinProductionId);

        if (bitcoinProdData is null)
        {
            logger.Error("Bitcoin production data is null when trying to update bitcoin farm");
            return;
        }

        // BSG finally fixed their settings, they now get loaded from the settings and used in the client
        var adjustedCraftTime =
            (profileHelper.IsDeveloperAccount(pmcData.SessionId!.Value) ? 40 : bitcoinProdData.ProductionTime)
            / (1 + (btcFarmCGs - 1) * databaseService.GetHideout().Settings.GpuBoostRate);

        // The progress should be adjusted based on the GPU boost rate, but the target is still the base productionTime
        var timeMultiplier = bitcoinProdData.ProductionTime / adjustedCraftTime;
        var timeElapsedSeconds = GetTimeElapsedSinceLastServerTick(pmcData, isGeneratorOn);
        btcProduction.Progress += Math.Floor(timeElapsedSeconds * timeMultiplier ?? 0);

        while (btcProduction.Progress >= bitcoinProdData.ProductionTime)
        {
            if (btcProduction.Products?.Count < coinSlotCount)
            // Has space to add a coin to production rewards
            {
                AddBtcToProduction(btcProduction, bitcoinProdData.ProductionTime ?? 0);
            }
            else
            // Filled up bitcoin storage
            {
                btcProduction.Progress = 0;
            }
        }

        btcProduction.StartTimestamp = timeUtil.GetTimeStamp();
    }

    /// <summary>
    ///     Add bitcoin object to btc production products array and set progress time
    /// </summary>
    /// <param name="btcProd">Bitcoin production object</param>
    /// <param name="coinCraftTimeSeconds">Time to craft a bitcoin</param>
    protected void AddBtcToProduction(Production btcProd, double coinCraftTimeSeconds)
    {
        btcProd.Products?.Add(
            new Item
            {
                Id = new MongoId(),
                Template = ItemTpl.BARTER_PHYSICAL_BITCOIN,
                Upd = new Upd { StackObjectsCount = 1 },
            }
        );

        // Deduct time spent crafting from progress
        btcProd.Progress -= coinCraftTimeSeconds;
    }

    /// <summary>
    ///     Get number of ticks that have passed since hideout areas were last processed, reduced when generator is off
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="isGeneratorOn">Is the generator on for the duration of elapsed time</param>
    /// <param name="recipe">Hideout production recipe being crafted we need the ticks for</param>
    /// <returns>Amount of time elapsed in seconds</returns>
    protected long? GetTimeElapsedSinceLastServerTick(PmcData pmcData, bool isGeneratorOn, HideoutProduction? recipe = null)
    {
        if (pmcData.Hideout is null)
        {
            const string message = "Pmc Hideout is null when trying get last elapsed server tick";
            logger.Error(message);
            throw new HideoutHelperException(message);
        }

        // Reduce time elapsed (and progress) when generator is off
        var timeElapsed = timeUtil.GetTimeStamp() - pmcData.Hideout.SptUpdateLastRunTimestamp;

        if (recipe is not null)
        {
            var hideoutArea = databaseService.GetHideout().Areas.FirstOrDefault(area => area.Type == recipe.AreaType);
            if (!(hideoutArea?.NeedsFuel ?? false))
            // e.g. Lavatory works at 100% when power is on / off
            {
                return timeElapsed;
            }
        }

        if (!isGeneratorOn && timeElapsed.HasValue)
        {
            timeElapsed = (long)(timeElapsed * databaseService.GetHideout().Settings.GeneratorSpeedWithoutFuel!.Value);
        }

        return timeElapsed;
    }

    /// <summary>
    ///     Get a count of how much possible BTC can be gathered by the profile
    /// </summary>
    /// <param name="pmcData">Profile to look up</param>
    /// <returns>Coin slot count</returns>
    protected double GetBTCSlots(PmcData pmcData)
    {
        var bitcoinProductions = databaseService
            .GetHideout()
            .Production.Recipes?.FirstOrDefault(production => production.Id == BitcoinProductionId);
        var productionSlots = bitcoinProductions?.ProductionLimitCount ?? 3; // Default to 3 if none found
        var hasManagementSkillSlots = profileHelper.HasEliteSkillLevel(SkillTypes.HideoutManagement, pmcData);
        var managementSlotsCount = GetEliteSkillAdditionalBitcoinSlotCount() ?? 2;

        return productionSlots + (hasManagementSkillSlots ? managementSlotsCount : 0);
    }

    /// <summary>
    ///     Get a count of how many additional bitcoins player hideout can hold with elite skill
    /// </summary>
    protected double? GetEliteSkillAdditionalBitcoinSlotCount()
    {
        return databaseService.GetGlobals().Configuration.SkillsSettings.HideoutManagement.EliteSlots.BitcoinFarm.Container;
    }

    /// <summary>
    ///     HideoutManagement skill gives a consumption bonus the higher the level
    ///     0.5% per level per 1-51, (25.5% at max)
    /// </summary>
    /// <param name="pmcData">Profile to get hideout consumption level from</param>
    /// <returns>Consumption bonus</returns>
    protected double? GetHideoutManagementConsumptionBonus(PmcData pmcData)
    {
        var hideoutManagementSkill = pmcData.GetSkillFromProfile(SkillTypes.HideoutManagement);
        if (hideoutManagementSkill is null || hideoutManagementSkill.Progress == 0)
        {
            return 0;
        }

        // If the level is 51 we need to round it at 50 so on elite you dont get 25.5%
        // at level 1 you already get 0.5%, so it goes up until level 50. For some reason the wiki
        // says that it caps at level 51 with 25% but as per dump data that is incorrect apparently
        var roundedLevel = Math.Floor(hideoutManagementSkill.Progress / 100);
        roundedLevel = roundedLevel.Approx(51d) ? roundedLevel - 1 : roundedLevel;

        return roundedLevel
            * databaseService.GetGlobals().Configuration.SkillsSettings.HideoutManagement.ConsumptionReductionPerLevel
            / 100;
    }

    /// <summary>
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="productionTime">Time to complete hideout craft in seconds</param>
    /// <param name="skill">Skill bonus to get reduction from</param>
    /// <param name="amountPerLevel">Skill bonus amount to apply</param>
    /// <returns>Seconds to reduce craft time by</returns>
    public double GetSkillProductionTimeReduction(PmcData pmcData, double productionTime, SkillTypes skill, double amountPerLevel)
    {
        var skillTimeReductionMultiplier = pmcData.GetSkillBonusMultipliedBySkillLevel(skill, amountPerLevel);

        return productionTime * skillTimeReductionMultiplier;
    }

    /// <summary>
    ///     Gather crafted BTC from hideout area and add to inventory
    ///     Reset production start timestamp if hideout area at full coin capacity
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="request">Take production request</param>
    /// <param name="sessionId">Session id</param>
    /// <param name="output">Output object to update</param>
    public void GetBTC(PmcData pmcData, HideoutTakeProductionRequestData request, MongoId sessionId, ItemEventRouterResponse output)
    {
        if (pmcData.Hideout?.Production is null)
        {
            const string message = "Hideout productions are null when trying to retrieve bitcoin productions";
            logger.Error(message);
            throw new HideoutHelperException(message);
        }

        if (pmcData.Hideout?.Production?.TryGetValue(BitcoinProductionId, out var bitcoinCraft) is null or false)
        {
            logger.Error("Bitcoin production does not exist when trying to retrieve bitcoin productions");
            return;
        }

        // Get how many coins were crafted and ready to pick up
        var craftedCoinCount = bitcoinCraft?.Products?.Count;
        if (bitcoinCraft is null || craftedCoinCount is null)
        {
            var errorMsg = serverLocalisationService.GetText("hideout-no_bitcoins_to_collect");
            logger.Error(errorMsg);

            httpResponseUtil.AppendErrorToOutput(output, errorMsg);

            return;
        }

        List<List<Item>> itemsToAdd = [];
        for (var index = 0; index < craftedCoinCount; index++)
        {
            itemsToAdd.Add(
                [
                    new Item
                    {
                        Id = new MongoId(),
                        Template = ItemTpl.BARTER_PHYSICAL_BITCOIN,
                        Upd = new Upd { StackObjectsCount = 1 },
                    },
                ]
            );
        }

        // Create request for what we want to add to stash
        var addItemsRequest = new AddItemsDirectRequest
        {
            ItemsWithModsToAdd = itemsToAdd,
            FoundInRaid = true,
            UseSortingTable = false,
            Callback = null,
        };

        // Add FiR coins to player inventory
        inventoryHelper.AddItemsToStash(sessionId, addItemsRequest, pmcData, output);
        if (output.Warnings?.Count > 0)
        {
            return;
        }

        // Is at max capacity + we collected all coins - reset production start time
        var coinSlotCount = GetBTCSlots(pmcData);
        if (bitcoinCraft.Products?.Count >= coinSlotCount)
        // Set start to now
        {
            bitcoinCraft.StartTimestamp = timeUtil.GetTimeStamp();
        }

        // Remove crafted coins from production in profile now they've been collected
        // Can only collect all coins, not individually
        bitcoinCraft.Products = [];
    }

    /// <summary>
    ///     Hideout improvement is flagged as complete
    /// </summary>
    /// <param name="improvement">hideout improvement object</param>
    /// <returns>true if complete</returns>
    protected bool HideoutImprovementIsComplete(HideoutImprovement improvement)
    {
        return improvement.Completed ?? false;
    }

    /// <summary>
    ///     Iterate over hideout improvements not completed and check if they need to be adjusted
    /// </summary>
    /// <param name="profileData">Profile to adjust</param>
    public void SetHideoutImprovementsToCompleted(PmcData profileData)
    {
        foreach (var improvementId in profileData.Hideout?.Improvements ?? [])
        {
            if (profileData.Hideout?.Improvements?.TryGetValue(improvementId.Key, out var improvementDetails) is null or false)
            {
                continue;
            }

            if (improvementDetails?.Completed == false && improvementDetails.ImproveCompleteTimestamp < timeUtil.GetTimeStamp())
            {
                improvementDetails.Completed = true;
            }
        }
    }

    /// <summary>
    ///     Add/remove bonus combat skill based on number of dogtags in place of fame hideout area
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    public void ApplyPlaceOfFameDogtagBonus(PmcData pmcData)
    {
        var fameAreaProfile = pmcData.Hideout?.Areas?.FirstOrDefault(area => area.Type == HideoutAreas.PlaceOfFame);
        if (fameAreaProfile is null)
        {
            logger.Error("Could not locate fame area in profile when trying to apply dogtag bonus");
            return;
        }

        // Get hideout area 16 bonus array
        var fameAreaDb = databaseService.GetHideout().Areas.FirstOrDefault(area => area.Type == HideoutAreas.PlaceOfFame);
        if (fameAreaDb is null)
        {
            logger.Error("Could not locate fame area in database when trying to apply dogtag bonus");
            return;
        }

        if (fameAreaDb.Stages?.TryGetValue(fameAreaProfile.Level?.ToString() ?? string.Empty, out var stage) is null or false)
        {
            logger.Error($"Could not locate stage: {fameAreaProfile.Level?.ToString() ?? "`Level is null`"} in fame area");
            return;
        }

        // Get SkillGroupLevelingBoost object
        var combatBoostBonusDb = stage.Bonuses?.FirstOrDefault(bonus => bonus.Type.ToString() == "SkillGroupLevelingBoost");

        // Get SkillGroupLevelingBoost object in profile
        var combatBonusProfile = pmcData.Bonuses?.FirstOrDefault(bonus => bonus.Id == combatBoostBonusDb?.Id);
        if (combatBonusProfile is null)
        {
            logger.Error($"Could not locate SkillGroupLevelingBoost: {combatBoostBonusDb?.Id.ToString() ?? "`Id is null`"} in profile");
            return;
        }

        // Get all slotted dogtag items
        var activeDogtags = pmcData.Inventory?.Items?.Where(item => item.SlotId?.StartsWith("dogtag") ?? false);
        if (activeDogtags is null)
        {
            logger.Warning("Could not locate any dogtag in the hall of fame when trying to apply dogtag bonus");
            return;
        }

        // Calculate bonus percent (apply hideoutManagement bonus)
        var hideoutManagementSkill = pmcData.GetSkillFromProfile(SkillTypes.HideoutManagement);
        if (hideoutManagementSkill is null)
        {
            logger.Error("Could not locate hideout management skill in profile when trying to apply dogtag bonus");
            return;
        }

        var hideoutManagementSkillBonusPercent = 1 + hideoutManagementSkill.Progress / 10000; // 5100 becomes 0.51, add 1 to it, 1.51
        var bonus = GetDogtagCombatSkillBonusPercent(pmcData, activeDogtags) * hideoutManagementSkillBonusPercent;

        // Update bonus value to above calculated value
        combatBonusProfile.Value = Math.Round(bonus, 2);
    }

    /// <summary>
    ///     Calculate the raw dogtag combat skill bonus for place of fame based on number of dogtags
    ///     Reverse engineered from client code
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="activeDogtags">Active dogtags in place of fame dogtag slots</param>
    /// <returns>Combat bonus</returns>
    protected static double GetDogtagCombatSkillBonusPercent(PmcData pmcData, IEnumerable<Item> activeDogtags)
    {
        // Not own dogtag
        // Side = opposite of player
        var result = 0D;
        foreach (var dogtag in activeDogtags)
        {
            if (dogtag.Upd?.Dogtag?.AccountId is null)
            {
                continue;
            }

            if (int.Parse(dogtag.Upd.Dogtag.AccountId) == pmcData.Aid)
            {
                continue;
            }

            result += 0.01 * dogtag.Upd.Dogtag.Level ?? 0;
        }

        return result;
    }

    /// <summary>
    ///     The wall pollutes a profile with various temp buffs/debuffs,
    ///     Remove them all
    /// </summary>
    /// <param name="wallAreaDb">Hideout area data</param>
    /// <param name="pmcData">Player profile</param>
    public void RemoveHideoutWallBuffsAndDebuffs(HideoutArea wallAreaDb, PmcData pmcData)
    {
        // Smush all stage bonuses into one array for easy iteration
        var wallBonuses = wallAreaDb.Stages?.SelectMany(stage => stage.Value.Bonuses!);
        if (wallBonuses is null)
        {
            logger.Warning("Could not locate wall bonuses in wall area, what are we trying to remove?");
            return;
        }

        // Get all bonus Ids that the wall adds
        HashSet<string> bonusIdsToRemove = [];
        foreach (var bonus in wallBonuses)
        {
            bonusIdsToRemove.Add(bonus.Id);
        }

        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug($"Removing: {bonusIdsToRemove.Count} bonuses from profile");
        }

        // Remove the wall bonuses from profile by id
        pmcData.Bonuses = pmcData.Bonuses?.Where(bonus => !bonusIdsToRemove.Contains(bonus.Id)).ToList();
    }
}
