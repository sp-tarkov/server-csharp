using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Core.Extensions
{
    public static class TemplateItemExtensions
    {
        public static IEnumerable<TemplateItem> OfClass(
            this Dictionary<string, TemplateItem> templates,
            params string[] baseClasses
        )
        {
            return templates.Where(x => baseClasses.Contains(x.Value.Parent)).Select(x => x.Value);
        }

        public static IEnumerable<TemplateItem> OfClass(
            this Dictionary<string, TemplateItem> templates,
            Func<TemplateItem, bool> pred,
            params string[] baseClasses
        )
        {
            return templates
                .Where(x => baseClasses.Contains(x.Value.Parent) && pred(x.Value))
                .Select(x => x.Value);
        }

        /// <summary>
        ///     Check if item is quest item
        /// </summary>
        /// <param name="tpl">Items tpl to check quest status of</param>
        /// <returns>true if item is flagged as quest item</returns>
        public static bool IsQuestItem(this TemplateItem templateItem)
        {
            if (templateItem.Properties.QuestItem.GetValueOrDefault(false))
            {
                return true;
            }

            return false;
        }
    }
}
