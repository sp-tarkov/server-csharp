using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Core.Extensions
{
    public static class TraderAssortExtensions
    {
        /// <summary>
        /// Remove an item from an assort
        /// Must be removed from the assorts; items + barterScheme + LoyaltyLevel
        /// </summary>
        /// <param name="assort">Assort to remove item from</param>
        /// <param name="itemId">Id of item to remove from assort</param>
        /// <param name="isFlea">Is the assort being modified the flea market assort</param>
        /// <returns>Modified assort</returns>
        public static TraderAssort RemoveItemFromAssort(this TraderAssort assort, MongoId itemId, bool isFlea = false)
        {
            // Flea assort needs special handling, item must remain in assort but be flagged as locked
            if (isFlea && assort.BarterScheme.TryGetValue(itemId, out var listToUse))
            {
                foreach (var barterScheme in listToUse.SelectMany(barterSchemes => barterSchemes))
                {
                    barterScheme.SptQuestLocked = true;
                }

                return assort;
            }

            assort.BarterScheme.Remove(itemId);
            assort.LoyalLevelItems.Remove(itemId);

            // The item being removed may have children linked to it, find and remove them too
            var idsToRemove = assort.Items.GetItemWithChildrenTpls(itemId).ToHashSet();
            assort.Items.RemoveAll(item => idsToRemove.Contains(item.Id));

            return assort;
        }

        /// <summary>
        ///     Given the blacklist provided, remove root items from assort
        /// </summary>
        /// <param name="assortToFilter">Trader assort to modify</param>
        /// <param name="itemsTplsToRemove">Item TPLs the assort should not have</param>
        public static void RemoveItemsFromAssort(this TraderAssort assortToFilter, HashSet<MongoId> itemsTplsToRemove)
        {
            assortToFilter.Items = assortToFilter
                .Items.Where(item => item.ParentId == "hideout" && itemsTplsToRemove.Contains(item.Template))
                .ToList();
        }
    }
}
