using Core.Models.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Spt.Config;
using Core.Models.Spt.Server;
using Core.Utils;
using Core.Utils.Cloners;

namespace _13AddTraderWithAssortJson
{
    public class AddTraderHelper
    {
        /**
     * Add record to trader config to set the refresh time of trader in seconds (default is 60 minutes)
     * @param traderConfig trader config to add our trader to
     * @param baseJson json file for trader (db/base.json)
     * @param refreshTimeSecondsMin How many seconds between trader stock refresh min time
     * @param refreshTimeSecondsMax How many seconds between trader stock refresh max time
     */
    public void SetTraderUpdateTime(TraderConfig traderConfig, TraderBase baseJson, int refreshTimeSecondsMin, int refreshTimeSecondsMax)
    {
        // Add refresh time in seconds to config
        var traderRefreshRecord = new UpdateTime
        {
            TraderId = baseJson.Id,
            Seconds = new MinMax<int>(refreshTimeSecondsMin, refreshTimeSecondsMax)
        };
    

        traderConfig.UpdateTime.Add(traderRefreshRecord);
    }

    /**
     * Add our new trader to the database
     * @param traderDetailsToAdd trader details
     * @param tables database
     * @param jsonUtil json utility class
     */
    public void AddTraderToDb(TraderBase traderDetailsToAdd, DatabaseTables tables, ICloner cloner, TraderAssort assortJson)
    {
        // Create trader data ready to add to database
        var traderDataToAdd = new Trader
        {
            Assort = cloner.Clone(assortJson), // Clone the data before saving it so we dont mess up any references
            Base = cloner.Clone(traderDetailsToAdd),
            QuestAssort = new Dictionary<string, Dictionary<string, string>> // questassort is empty as trader has no assorts unlocked by quests
            {
                { "Started", new Dictionary<string, string>() },
                { "Success", new Dictionary<string, string>() },
                { "Fail", new Dictionary<string, string>() }
            }
        };

        // Add trader to trader table, key is the traders id
        tables.Traders.Add(traderDetailsToAdd.Id, traderDataToAdd);
    }

    /**
     * Add traders name/location/description to the locale table
     * @param baseJson json file for trader (db/base.json)
     * @param tables database tables
     * @param fullName Complete name of trader
     * @param firstName First name of trader
     * @param nickName Nickname of trader
     * @param location Location of trader (e.g. "Here in the cat shop")
     * @param description Description of trader
     */
    public void AddTraderToLocales(TraderBase baseJson, DatabaseTables tables, string fullName, string firstName, string nickName, string location, string description)
    {
        // For each language, add locale for the new trader
        var locales = tables.Locales.Global;

        foreach (var (key, value) in locales) {
            value.Value[$"{baseJson.Id} FullName"] = fullName;
            value.Value[$"{baseJson.Id} FirstName"] = firstName;
            value.Value[$"{baseJson.Id} Nickname"] = nickName;
            value.Value[$"{baseJson.Id} Location"] = location;
            value.Value[$"{baseJson.Id} Description"] = description;
        }
    }
    }
}
