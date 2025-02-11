using Core.Models.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Spt.Config;
using Core.Models.Spt.Server;
using Core.Utils;
using Core.Utils.Cloners;

namespace _13._1AddTraderWithDynamicAssorts
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
    public void AddTraderToDb(TraderBase traderDetailsToAdd, DatabaseTables tables, ICloner cloner, TraderAssort assort)
    {
        // Create trader data ready to add to database
        var traderDataToAdd = new Trader
        {
            Assort = cloner.Clone(assort),
            Base = cloner.Clone(traderDetailsToAdd),
            QuestAssort = new Dictionary<string, Dictionary<string, string>> // quest assort is empty as trader has no assorts unlocked by quests
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

    public List<Item> CreateGlock()
    {
            // Create an array ready to hold weapon + all mods
            var glock = new List<Item>();

            // Add the base first
            glock.Add(new Item { // Add the base weapon first
            Id =
                NewItemIds.GLOCK_BASE, // Ids matter, MUST BE UNIQUE
            Template =
                "5a7ae0c351dfba0017554310", // This is the weapons tpl, found on: https://db.sp-tarkov.com/search
        });

            // Add barrel
            glock.Add(new Item {
            Id =
                NewItemIds.GLOCK_BARREL,
            Template =
                "5a6b60158dc32e000a31138b",
            ParentId =
                NewItemIds.GLOCK_BASE, // This is a sub item, you need to define its parent its attached to / inserted into
            SlotId =
                "mod_barrel", // Required for mods, you need to define what 'slot' the mod will fill on the weapon
        });

            // Add receiver
            glock.Add( new Item {
            Id =
                NewItemIds.GLOCK_RECIEVER,
            Template =
                "5a9685b1a2750c0032157104",
            ParentId =
                NewItemIds.GLOCK_BASE,
            SlotId =
                "mod_reciever",
        });

            // Add compensator
            glock.Add(new Item {
            Id =
                NewItemIds.GLOCK_COMPENSATOR,
            Template =
                "5a7b32a2e899ef00135e345a",
            ParentId =
                NewItemIds.GLOCK_RECIEVER, // The parent of this mod is the receiver NOT weapon, be careful to get the correct parent
            SlotId =
                "mod_muzzle",
        });

            // Add Pistol grip
            glock.Add(new Item {
            Id =
                NewItemIds.GLOCK_PISTOL_GRIP,
            Template =
                "5a7b4960e899ef197b331a2d",
            ParentId =
                NewItemIds.GLOCK_BASE,
            SlotId =
                "mod_pistol_grip",
        });

            // Add front sight
            glock.Add(new Item {
            Id =
                NewItemIds.GLOCK_FRONT_SIGHT,
            Template =
                "5a6f5d528dc32e00094b97d9",
            ParentId =
                NewItemIds.GLOCK_RECIEVER,
            SlotId =
                "mod_sight_rear",
        });

            // Add rear sight
            glock.Add(new Item {
            Id =
                NewItemIds.GLOCK_REAR_SIGHT,
            Template =
                "5a6f58f68dc32e000a311390",
            ParentId =
                NewItemIds.GLOCK_RECIEVER,
            SlotId =
                "mod_sight_front",
        });

            // Add magazine
            glock.Add(new Item {
            Id =
                NewItemIds.GLOCK_MAGAZINE,
            Template =
                "630769c4962d0247b029dc60",
            ParentId =
                NewItemIds.GLOCK_BASE,
            SlotId =
                "mod_magazine",
        });

            return glock;
        }
    }
}
