using System.Text.Json;
using SPTarkov.Common.Extensions;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Core.Extensions
{
    public static class ItemExtensions
    {
        /// <summary>
        /// This method will compare two items and see if they are equivalent
        /// This method will NOT compare IDs on the items
        /// </summary>
        /// <param name="item1">first item to compare</param>
        /// <param name="item2">second item to compare</param>
        /// <param name="compareUpdProperties">Upd properties to compare between the items</param>
        /// <returns>true if they are the same</returns>
        public static bool IsSameItem(
            this Item item1,
            Item item2,
            ISet<string>? compareUpdProperties = null
        )
        {
            // Different tpl == different item
            if (item1.Template != item2.Template)
            {
                return false;
            }

            // Both lack upd object + same tpl = same
            if (item1.Upd is null && item2.Upd is null)
            {
                return true;
            }

            // item1 lacks upd, item2 has one
            if (item1.Upd is null && item2.Upd is not null)
            {
                return false;
            }

            // item1 has upd, item2 lacks one
            if (item1.Upd is not null && item2.Upd is null)
            {
                return false;
            }

            // key = Upd property Type as string, value = comparison function that returns bool
            var comparers = new Dictionary<string, Func<Upd, Upd, bool>>
            {
                { "Key", (upd1, upd2) => upd1.Key?.NumberOfUsages == upd2.Key?.NumberOfUsages },
                {
                    "Buff",
                    (upd1, upd2) =>
                        upd1.Buff?.Value == upd2.Buff?.Value
                        && upd1.Buff?.BuffType == upd2.Buff?.BuffType
                },
                {
                    "CultistAmulet",
                    (upd1, upd2) =>
                        upd1.CultistAmulet?.NumberOfUsages == upd2.CultistAmulet?.NumberOfUsages
                },
                { "Dogtag", (upd1, upd2) => upd1.Dogtag?.ProfileId == upd2.Dogtag?.ProfileId },
                { "FaceShield", (upd1, upd2) => upd1.FaceShield?.Hits == upd2.FaceShield?.Hits },
                {
                    "Foldable",
                    (upd1, upd2) =>
                        upd1.Foldable?.Folded.GetValueOrDefault(false)
                        == upd2.Foldable?.Folded.GetValueOrDefault(false)
                },
                {
                    "FoodDrink",
                    (upd1, upd2) => upd1.FoodDrink?.HpPercent == upd2.FoodDrink?.HpPercent
                },
                { "MedKit", (upd1, upd2) => upd1.MedKit?.HpResource == upd2.MedKit?.HpResource },
                {
                    "RecodableComponent",
                    (upd1, upd2) =>
                        upd1.RecodableComponent?.IsEncoded == upd2.RecodableComponent?.IsEncoded
                },
                {
                    "RepairKit",
                    (upd1, upd2) => upd1.RepairKit?.Resource == upd2.RepairKit?.Resource
                },
                {
                    "Resource",
                    (upd1, upd2) => upd1.Resource?.UnitsConsumed == upd2.Resource?.UnitsConsumed
                },
            };

            // Choose above keys or passed in keys to compare items with
            var valuesToCompare =
                compareUpdProperties?.Count > 0 ? compareUpdProperties : comparers.Keys.ToHashSet();
            foreach (var propertyName in valuesToCompare)
            {
                if (!comparers.TryGetValue(propertyName, out var comparer))
                // Key not found, skip
                {
                    continue;
                }

                if (!comparer(item1.Upd, item2.Upd))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Check if item is stored inside a container
        /// </summary>
        /// <param name="itemToCheck">Item to check is inside of container</param>
        /// <param name="desiredContainerSlotId">Name of slot to check item is in e.g. SecuredContainer/Backpack</param>
        /// <param name="items">Inventory with child parent items to check</param>
        /// <returns>True when item is in container</returns>
        public static bool ItemIsInsideContainer(
            this Item itemToCheck,
            string desiredContainerSlotId,
            IEnumerable<Item> items
        )
        {
            // Get items parent
            var parent = items.FirstOrDefault(item => item.Id.Equals(itemToCheck.ParentId));
            if (parent is null)
            // No parent, end of line, not inside container
            {
                return false;
            }

            if (parent.SlotId == desiredContainerSlotId)
            {
                return true;
            }

            return parent.ItemIsInsideContainer(desiredContainerSlotId, items);
        }

        /// <summary>
        ///     Get the size of a stack, return 1 if no stack object count property found
        /// </summary>
        /// <param name="item">Item to get stack size of</param>
        /// <returns>size of stack</returns>
        public static int GetItemStackSize(this Item item)
        {
            if (item.Upd?.StackObjectsCount is not null)
            {
                return (int)item.Upd.StackObjectsCount;
            }

            return 1;
        }

        /// <summary>
        /// Create a dictionary from a collection of items, keyed by item id
        /// </summary>
        /// <param name="items">Collection of items</param>
        /// <returns>Dictionary of items</returns>
        public static Dictionary<MongoId, Item> GenerateItemsMap(this IEnumerable<Item> items)
        {
            // Convert list to dictionary, keyed by items Id
            return items.ToDictionary(item => item.Id);
        }

        /// <summary>
        /// Adopts orphaned items by resetting them as root "hideout" items. Helpful in situations where a parent has been
        /// deleted from a group of items and there are children still referencing the missing parent. This method will
        /// remove the reference from the children to the parent and set item properties to root values.
        /// </summary>
        /// <param name="rootId">The ID of the "root" of the container</param>
        /// <param name="items">Array of Items that should be adjusted</param>
        /// <returns>Returns Array of Items that have been adopted</returns>
        public static List<Item> AdoptOrphanedItems(this List<Item> items, string rootId)
        {
            foreach (var item in items)
            {
                // Check if the item's parent exists.
                var parentExists = items.Any(parentItem => parentItem.Id.Equals(item.ParentId));

                // If the parent does not exist and the item is not already a 'hideout' item, adopt the orphaned item by
                // setting the parent ID to the PMCs inventory equipment ID, the slot ID to 'hideout', and remove the location.
                if (!parentExists && item.ParentId != rootId && item.SlotId != "hideout")
                {
                    item.ParentId = rootId;
                    item.SlotId = "hideout";
                    item.Location = null;
                }
            }

            return items;
        }

        /// <summary>
        /// Recursive function that looks at every item from parameter and gets their children's Ids + includes parent item in results
        /// </summary>
        /// <param name="items">List of items (item + possible children)</param>
        /// <param name="baseItemId">Parent item's id</param>
        /// <returns>list of child item ids</returns>
        public static List<MongoId> FindAndReturnChildrenByItems(
            this IEnumerable<Item> items,
            MongoId baseItemId
        )
        {
            List<MongoId> list = [];

            foreach (var childItem in items)
            {
                if (childItem.ParentId == baseItemId.ToString())
                {
                    list.AddRange(FindAndReturnChildrenByItems(items, childItem.Id));
                }
            }

            list.Add(baseItemId); // Required, push original item id onto array

            return list;
        }

        /// <summary>
        /// Check if the passed in item has buy count restrictions
        /// </summary>
        /// <param name="itemToCheck">Item to check</param>
        /// <returns>true if it has buy restrictions</returns>
        public static bool HasBuyRestrictions(this Item itemToCheck)
        {
            return itemToCheck.Upd?.BuyRestrictionCurrent is not null
                && itemToCheck.Upd?.BuyRestrictionMax is not null;
        }

        /// <summary>
        ///     Gets the identifier for a child using slotId, locationX and locationY.
        /// </summary>
        /// <param name="item">Item.</param>
        /// <returns>SlotId OR slotid, locationX, locationY.</returns>
        public static string GetChildId(this Item item)
        {
            if (item.Location is null)
            {
                return item.SlotId;
            }

            var LocationTyped = (ItemLocation)item.Location;

            return $"{item.SlotId},{LocationTyped.X},{LocationTyped.Y}";
        }

        public static bool IsVertical(this ItemLocation itemLocation)
        {
            var castValue = itemLocation.R.ToString();
            return castValue == "1"
                || string.Equals(castValue, "vertical", StringComparison.OrdinalIgnoreCase)
                || string.Equals(
                    itemLocation.Rotation?.ToString(),
                    "vertical",
                    StringComparison.OrdinalIgnoreCase
                );
        }

        /// <summary>
        ///     Update items upd.StackObjectsCount to be 1 if its upd is missing or StackObjectsCount is undefined
        /// </summary>
        /// <param name="item">Item to update</param>
        /// <returns>Fixed item</returns>
        public static void FixItemStackCount(this Item item)
        {
            // Ensure item has 'Upd' object
            item.Upd ??= new Upd { StackObjectsCount = 1 };

            // Ensure item has 'StackObjectsCount' property
            item.Upd.StackObjectsCount ??= 1;
        }

        /// <summary>
        /// A variant of FindAndReturnChildren where the output is list of item objects instead of their ids.
        /// </summary>
        /// <param name="items">List of items (item + possible children)</param>
        /// <param name="baseItemId">Parent item's id</param>
        /// <param name="modsOnly">OPTIONAL - Include only mod items, exclude items stored inside root item</param>
        /// <returns>list of Item objects</returns>
        public static List<Item> FindAndReturnChildrenAsItems(
            this IEnumerable<Item> items,
            MongoId baseItemId,
            bool modsOnly = false
        )
        {
            // Use dictionary to make key lookup faster, convert to list before being returned
            var itemList = items.ToList();
            OrderedDictionary<MongoId, Item> result = [];
            foreach (var childItem in itemList)
            {
                // Include itself
                if (childItem.Id == baseItemId)
                {
                    // Root item MUST be at 0 index for things like flea market offers
                    result.Insert(0, childItem.Id, childItem);
                    continue;
                }

                // Is stored in parent and disallowed
                if (modsOnly && childItem.Location is not null)
                {
                    continue;
                }

                // Items parentId matches root item AND returned items doesn't contain current child
                if (
                    !result.ContainsKey(childItem.Id)
                    && childItem.ParentId != "hideout"
                    && childItem.ParentId == baseItemId.ToString()
                )
                {
                    foreach (var item in FindAndReturnChildrenAsItems(items, childItem.Id))
                    {
                        result.Add(item.Id, item);
                    }
                }
            }

            return result.Values.ToList();
        }

        /// <summary>
        /// Convert an Item to SptLootItem
        /// </summary>
        /// <param name="item">Item to convert</param>
        /// <returns>Converted SptLootItem</returns>
        public static SptLootItem ToLootItem(this Item item)
        {
            return new SptLootItem
            {
                ComposedKey = null,
                Id = item.Id,
                Template = item.Template,
                Upd = item.Upd,
                ParentId = item.ParentId,
                SlotId = item.SlotId,
                Location = item.Location,
                Desc = item.Desc,
                ExtensionData = item.ExtensionData,
            };
        }

        public static ItemLocation? GetParsedLocation(this Item item)
        {
            if (item.Location is null)
            {
                return null;
            }

            if (item.Location is JsonElement element)
            {
                // TODO: when is this true
                return element.ToObject<ItemLocation>();
            }

            return (ItemLocation)item.Location;
        }

        /// <summary>
        ///     Get a list of the item IDs (NOT tpls) inside a secure container
        /// </summary>
        /// <param name="items">Inventory items to look for secure container in</param>
        /// <returns>List of ids</returns>
        public static List<MongoId> GetSecureContainerItems(this List<Item> items)
        {
            var secureContainer = items.First(x => x.SlotId == "SecuredContainer");

            // No container found, drop out
            if (secureContainer is null)
            {
                return [];
            }

            var itemsInSecureContainer = items.FindAndReturnChildrenByItems(secureContainer.Id);

            // Return all items returned and exclude the secure container item itself
            return itemsInSecureContainer.Where(x => x != secureContainer.Id).ToList();
        }

        /// <summary>
        ///     Regenerate all GUIDs with new IDs, except special item types (e.g. quest, sorting table, etc.)
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static IEnumerable<Item> ReplaceIDs(this IEnumerable<Item> items)
        {
            foreach (var item in items)
            {
                // Generate new id
                var newId = new MongoId();

                // Keep copy of original id
                var originalId = item.Id;

                // Update items id to new one we generated
                item.Id = newId;

                // Find all children of item and update their parent ids to match
                var childItems = items.Where(item => item.ParentId == originalId.ToString());
                foreach (var childItem in childItems)
                {
                    childItem.ParentId = newId;
                }
            }

            return items;
        }

        /// <summary>
        /// Update a root items _id property value to be unique
        /// </summary>
        /// <param name="itemWithChildren">Item to update root items _id property</param>
        /// <param name="newId">Optional: new id to use</param>
        /// <returns>New root id</returns>
        public static MongoId RemapRootItemId(
            this List<Item> itemWithChildren,
            MongoId? newId = null
        )
        {
            newId ??= new MongoId();

            var rootItemExistingId = itemWithChildren.FirstOrDefault().Id;

            foreach (var item in itemWithChildren)
            {
                // Root, update id
                if (item.Id.Equals(rootItemExistingId))
                {
                    item.Id = newId.Value;

                    continue;
                }

                // Child with parent of root, update
                if (item.ParentId == rootItemExistingId)
                {
                    item.ParentId = newId.Value;
                }
            }

            return newId.Value;
        }
    }
}
