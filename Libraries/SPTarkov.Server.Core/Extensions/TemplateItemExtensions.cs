using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Core.Extensions
{
    public static class TemplateItemExtensions
    {
        public static IEnumerable<TemplateItem> OfClass(this Dictionary<MongoId, TemplateItem> templates, params MongoId[] baseClasses)
        {
            return templates.Where(x => baseClasses.Contains(x.Value.Parent)).Select(x => x.Value);
        }

        public static IEnumerable<TemplateItem> OfClass(
            this Dictionary<MongoId, TemplateItem> templates,
            Func<TemplateItem, bool> pred,
            params MongoId[] baseClasses
        )
        {
            return templates.Where(x => baseClasses.Contains(x.Value.Parent) && pred(x.Value)).Select(x => x.Value);
        }

        /// <summary>
        ///     Check if item is quest item
        /// </summary>
        /// <param name="templateItem">Item to check quest status of</param>
        /// <returns>true if item is flagged as quest item</returns>
        public static bool IsQuestItem(this TemplateItem templateItem)
        {
            if (templateItem.Properties.QuestItem.GetValueOrDefault(false))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Get a weapons default magazine template id
        /// </summary>
        /// <param name="weaponTemplate">Weapon to get default magazine for</param>
        /// <returns>Tpl of magazine</returns>
        public static MongoId? GetWeaponsDefaultMagazineTpl(this TemplateItem weaponTemplate)
        {
            return weaponTemplate.Properties.DefMagType;
        }

        /// <summary>
        ///     Get the default plate an armor has in its db item
        /// </summary>
        /// <param name="armorItem">Item to look up default plate</param>
        /// <param name="modSlot">front/back</param>
        /// <returns>Tpl of plate</returns>
        public static MongoId? GetDefaultPlateTpl(this TemplateItem armorItem, string modSlot)
        {
            var relatedItemDbModSlot = armorItem.Properties.Slots?.FirstOrDefault(slot =>
                string.Equals(slot.Name, modSlot, StringComparison.OrdinalIgnoreCase)
            );

            return relatedItemDbModSlot?.Props?.Filters?.FirstOrDefault()?.Plate;
        }

        /// <summary>
        ///     Does the passed in <see cref="TemplateItem"/> lack slots, cartridges or chambers
        /// </summary>
        /// <param name="item">Item to check</param>
        /// <returns>True if it lacks cartridges/chamber slots, False if not</returns>
        public static bool HasNoSlotsCartridgesOrChambers(this TemplateItem item)
        {
            if (item.Properties is null)
            {
                return true;
            }

            return item.Properties.Slots is null
                || !item.Properties.Slots.Any()
                    && (item.Properties.Cartridges is null || !item.Properties.Cartridges.Any())
                    && (item.Properties.Chambers is null || !item.Properties.Chambers.Any());
        }
    }
}
