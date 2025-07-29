using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;

namespace SPTarkov.Server.Core.Extensions
{
    public static class ItemEventRouterResponseExtensions
    {
        /// <summary>
        /// Add item stack change object into output route event response
        /// </summary>
        /// <param name="output">Response to add item change event into</param>
        /// <param name="sessionId">Session id</param>
        /// <param name="item">Item that was adjusted</param>
        public static void AddItemStackSizeChangeIntoEventResponse(this ItemEventRouterResponse output, MongoId sessionId, Item item)
        {
            // TODO: replace with something safer like TryGet
            output
                .ProfileChanges[sessionId]
                .Items.ChangedItems.Add(
                    new Item
                    {
                        Id = item.Id,
                        Template = item.Template,
                        ParentId = item.ParentId,
                        SlotId = item.SlotId,
                        Location = item.Location,
                        Upd = new Upd { StackObjectsCount = item.Upd.StackObjectsCount },
                    }
                );
        }
    }
}
