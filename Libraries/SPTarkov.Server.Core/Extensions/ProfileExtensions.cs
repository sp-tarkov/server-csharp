using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Enums.Hideout;

namespace SPTarkov.Server.Core.Extensions;

public static class ProfileExtensions
{
    /// <summary>
    ///     Return all quest items current in the supplied profile
    /// </summary>
    /// <param name="profile">Profile to get quest items from</param>
    /// <returns>List of item objects</returns>
    public static IEnumerable<Item> GetQuestItemsInProfile(this PmcData profile)
    {
        return profile?.Inventory?.Items.Where(i => i.ParentId == profile.Inventory.QuestRaidItems).ToList();
    }

    /// <summary>
    ///     Upgrade hideout wall from starting level to interactable level if necessary stations have been upgraded
    /// </summary>
    /// <param name="profile">Profile to upgrade wall in</param>
    public static void UnlockHideoutWallInProfile(this PmcData profile)
    {
        var profileHideoutAreas = profile.Hideout.Areas;
        var waterCollector = profileHideoutAreas.FirstOrDefault(x => x.Type == HideoutAreas.WaterCollector);
        var medStation = profileHideoutAreas.FirstOrDefault(x => x.Type == HideoutAreas.MedStation);
        var wall = profileHideoutAreas.FirstOrDefault(x => x.Type == HideoutAreas.EmergencyWall);

        // No collector or med station, skip
        if (waterCollector is null && medStation is null)
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
    ///     Does the provided profile contain any condition counters
    /// </summary>
    /// <param name="profile"> Profile to check for condition counters </param>
    /// <returns> Profile has condition counters </returns>
    public static bool ProfileHasConditionCounters(this PmcData profile)
    {
        if (profile.TaskConditionCounters is null)
        {
            return false;
        }

        return profile.TaskConditionCounters.Count > 0;
    }

    /// <summary>
    ///     Get a specific common skill from supplied profile
    /// </summary>
    /// <param name="profile">Player profile</param>
    /// <param name="skill">Skill to look up and return value from</param>
    /// <returns>Common skill object from desired profile</returns>
    public static CommonSkill? GetSkillFromProfile(this PmcData profile, SkillTypes skill)
    {
        return profile?.Skills?.Common?.FirstOrDefault(s => s.Id == skill);
    }

    /// <summary>
    ///     Get a multiplier based on player's skill level and value per level
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="skill">Player skill from profile</param>
    /// <param name="valuePerLevel">Value from globals.config.SkillsSettings - `PerLevel`</param>
    /// <returns>Multiplier from 0 to 1</returns>
    public static double GetSkillBonusMultipliedBySkillLevel(this PmcData pmcData, SkillTypes skill, double valuePerLevel)
    {
        var profileSkill = pmcData.GetSkillFromProfile(skill);
        if (profileSkill is null || profileSkill.Progress == 0)
        {
            return 0;
        }

        // If the level is 51 we need to round it at 50 so on elite you dont get 25.5%
        // at level 1 you already get 0.5%, so it goes up until level 50. For some reason the wiki
        // says that it caps at level 51 with 25% but as per dump data that is incorrect apparently
        var roundedLevel = Math.Floor(profileSkill.Progress / 100);
        roundedLevel = roundedLevel.Approx(51d) ? roundedLevel - 1 : roundedLevel;

        return roundedLevel * valuePerLevel / 100;
    }

    /// <summary>
    ///     Get the scav karma level for a profile
    ///     Is also the fence trader rep level
    /// </summary>
    /// <param name="pmcData">pmc profile</param>
    /// <returns>karma level</returns>
    public static double GetScavKarmaLevel(this PmcData pmcData)
    {
        // can be empty during profile creation
        if (!pmcData.TradersInfo.TryGetValue(Traders.FENCE, out var fenceInfo))
        {
            return 0;
        }

        if (fenceInfo.Standing > 6)
        {
            return 6;
        }

        return Math.Floor(fenceInfo.Standing ?? 0);
    }

    public static Skills GetSkillsOrDefault(this PmcData profile)
    {
        return profile?.Skills ?? GetDefaultSkills();
    }

    private static Skills GetDefaultSkills()
    {
        return new Skills
        {
            Common = [],
            Mastering = [],
            Points = 0,
        };
    }

    /// <summary>
    ///     Recursively checks if the given item is
    ///     inside the stash, that is it has the stash as
    ///     ancestor with slotId=hideout
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="itemToCheck">Item to look for</param>
    /// <returns>True if item exists inside stash</returns>
    public static bool IsItemInStash(this PmcData pmcData, Item itemToCheck)
    {
        // Start recursive check
        return pmcData.IsParentInStash(itemToCheck.Id);
    }

    public static bool IsParentInStash(this PmcData pmcData, MongoId itemId)
    {
        // Item not found / has no parent
        var item = pmcData.Inventory.Items.FirstOrDefault(item => item.Id == itemId);
        if (item?.ParentId is null)
        {
            return false;
        }

        // Root level. Items parent is the stash with slotId "hideout"
        if (item.ParentId == pmcData.Inventory.Stash && item.SlotId == "hideout")
        {
            return true;
        }

        // Recursive case: Check the items parent
        return IsParentInStash(pmcData, item.ParentId);
    }

    /// <summary>
    ///     Iterate over all bonuses and sum up all bonuses of desired type in provided profile
    /// </summary>
    /// <param name="pmcProfile">Player profile</param>
    /// <param name="desiredBonus">Bonus to sum up</param>
    /// <returns>Summed bonus value or 0 if no bonus found</returns>
    public static double GetBonusValueFromProfile(this PmcData pmcProfile, BonusType desiredBonus)
    {
        var bonuses = pmcProfile?.Bonuses?.Where(b => b.Type == desiredBonus);
        if (!bonuses.Any())
        {
            return 0;
        }

        // Sum all bonuses found above
        return bonuses?.Sum(bonus => bonus?.Value ?? 0) ?? 0;
    }

    public static bool PlayerIsFleaBanned(this PmcData pmcProfile, long currentTimestamp)
    {
        return pmcProfile?.Info?.Bans?.Any(b => b.BanType == BanType.RagFair && currentTimestamp < b.DateTime) ?? false;
    }

    /// <summary>
    ///     Calculates the current level of a player based on their accumulated experience points.
    ///     This method iterates through an experience table to determine the highest level achieved
    ///     by comparing the player's experience against cumulative thresholds.
    /// </summary>
    /// <param name="pmcData"> Player profile </param>
    /// <param name="expTable">Experience table from globals.json</param>
    /// <returns>
    ///     The calculated level of the player as an integer, or null if the level cannot be determined.
    ///     This value is also assigned to <see cref="Info.Level" /> within the provided profile.
    /// </returns>
    public static int? CalculateLevel(this PmcData pmcData, ExpTable[] expTable)
    {
        var accExp = 0;
        for (var i = 0; i < expTable.Length; i++)
        {
            accExp += expTable[i].Experience;

            if (pmcData.Info.Experience < accExp)
            {
                break;
            }

            pmcData.Info.Level = i + 1;
        }

        return pmcData.Info.Level;
    }

    /// <summary>
    ///     Does the provided item have a root item with the provided id
    /// </summary>
    /// <param name="pmcData">Profile with items</param>
    /// <param name="item">Item to check</param>
    /// <param name="rootId">Root item id to check for</param>
    /// <returns>True when item has rootId, false when not</returns>
    public static bool DoesItemHaveRootId(this PmcData pmcData, Item item, MongoId rootId)
    {
        var currentItem = item;
        while (currentItem is not null)
        {
            // If we've found the equipment root ID, return true
            if (currentItem.Id == rootId)
            {
                return true;
            }

            // Otherwise get the parent item
            currentItem = pmcData.Inventory.Items.FirstOrDefault(item => item.Id == currentItem.ParentId);
        }

        return false;
    }

    /// <summary>
    ///     Get status of a quest in player profile by its id
    /// </summary>
    /// <param name="pmcData">Profile to search</param>
    /// <param name="questId">Quest id to look up</param>
    /// <returns>QuestStatus enum</returns>
    public static QuestStatusEnum GetQuestStatus(this PmcData pmcData, MongoId questId)
    {
        var quest = pmcData.Quests?.FirstOrDefault(q => q.QId == questId);

        return quest?.Status ?? QuestStatusEnum.Locked;
    }

    /// <summary>
    /// Use values from the profiles template to reset all body part max values
    /// </summary>
    /// <param name="profile">Profile to update</param>
    /// <param name="profileTemplate">Template used to create profile</param>
    public static void ResetMaxLimbHp(this PmcData profile, TemplateSide profileTemplate)
    {
        foreach (var (partKey, bodyPart) in profile.Health.BodyParts)
        {
            bodyPart.Health.Maximum = profileTemplate.Character.Health.BodyParts[partKey].Health.Maximum;
        }
    }

    /// <summary>
    ///     Handle Remove event
    ///     Remove item from player inventory + insured items array
    ///     Also deletes child items
    /// </summary>
    /// <param name="profile">Profile to remove item from (pmc or scav)</param>
    /// <param name="itemId">Items id to remove</param>
    /// <param name="sessionId">Session id</param>
    /// <param name="output">OPTIONAL - ItemEventRouterResponse</param>
    public static void RemoveItem(this PmcData profile, MongoId itemId, MongoId sessionId, ItemEventRouterResponse? output = null)
    {
        if (itemId.IsEmpty)
        {
            return;
        }

        // Get children of item, they get deleted too
        var itemAndChildrenToRemove = profile.Inventory.Items.GetItemWithChildren(itemId);
        if (!itemAndChildrenToRemove.Any())
        {
            return;
        }

        var inventoryItems = profile.Inventory.Items;
        var insuredItems = profile.InsuredItems;

        // We have output object, inform client of root item deletion, not children
        output?.ProfileChanges[sessionId].Items.DeletedItems.Add(new DeletedItem { Id = itemId });

        foreach (var item in itemAndChildrenToRemove)
        {
            // We expect that each inventory item and each insured item has unique "_id", respective "itemId".
            // Therefore, we want to use a NON-Greedy function and escape the iteration as soon as we find requested item.
            var inventoryIndex = inventoryItems.FindIndex(inventoryItem => inventoryItem.Id == item.Id);
            if (inventoryIndex != -1)
            {
                inventoryItems.RemoveAt(inventoryIndex);
            }

            var insuredItemIndex = insuredItems.FindIndex(insuredItem => insuredItem.ItemId == item.Id);
            if (insuredItemIndex != -1)
            {
                insuredItems.RemoveAt(insuredItemIndex);
            }
        }
    }

    /// <summary>
    ///     Does Player have necessary trader loyalty to purchase flea offer
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="fleaOffer">Flea offer being bought</param>
    /// <returns>True if player can buy offer</returns>
    public static bool ProfileMeetsTraderLoyaltyLevelToBuyOffer(this PmcData pmcData, RagfairOffer fleaOffer)
    {
        if (fleaOffer.LoyaltyLevel == 0)
        {
            // No requirement, always passes
            return true;
        }

        if (pmcData.TradersInfo.TryGetValue(fleaOffer.User.Id, out var traderInfo))
        {
            // Trader exists in profile ,do loyalty level check
            return traderInfo.LoyaltyLevel >= fleaOffer.LoyaltyLevel;
        }

        // No trader data on player profile, fail check
        return false;
    }
}
