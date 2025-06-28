using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Extensions
{
    public static class ProfileExtensions
    {
        /// <summary>
        ///     Return all quest items current in the supplied profile
        /// </summary>
        /// <param name="profile">Profile to get quest items from</param>
        /// <returns>List of item objects</returns>
        public static List<Item> GetQuestItemsInProfile(this PmcData profile)
        {
            return profile
                ?.Inventory?.Items.Where(i => i.ParentId == profile.Inventory.QuestRaidItems)
                .ToList();
        }

        /// <summary>
        ///     Upgrade hideout wall from starting level to interactable level if necessary stations have been upgraded
        /// </summary>
        /// <param name="profile">Profile to upgrade wall in</param>
        public static void UnlockHideoutWallInProfile(this PmcData profile)
        {
            var profileHideoutAreas = profile.Hideout.Areas;
            var waterCollector = profileHideoutAreas.FirstOrDefault(x =>
                x.Type == HideoutAreas.WaterCollector
            );
            var medStation = profileHideoutAreas.FirstOrDefault(x =>
                x.Type == HideoutAreas.MedStation
            );
            var wall = profileHideoutAreas.FirstOrDefault(x =>
                x.Type == HideoutAreas.EmergencyWall
            );

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

        public static bool IsParentInStash(this PmcData pmcData, string itemId)
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
        public static double GetBonusValueFromProfile(
            this PmcData pmcProfile,
            BonusType desiredBonus
        )
        {
            var bonuses = pmcProfile?.Bonuses?.Where(b => b.Type == desiredBonus);
            if (!bonuses.Any())
            {
                return 0;
            }

            // Sum all bonuses found above
            return bonuses?.Sum(bonus => bonus?.Value ?? 0) ?? 0;
        }
    }
}
