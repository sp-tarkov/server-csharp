using Core.Models.Eft.Common.Tables;
using Core.Models.External;
using Core.Models.Spt.Config;
using Core.Models.Spt.Mod;
using Core.Models.Utils;
using Core.Servers;
using Core.Services.Mod;

namespace _18._1CustomItemServiceLootBox;

public class CustomItemServiceLootBox : IPostDBLoadMod, IPostSptLoadMod
{
    private CustomItemService _customItemService;
    private DatabaseServer _databaseServer;
    private ConfigServer _configServer;
    private readonly ISptLogger<CustomItemServiceLootBox> _logger;
    private Dictionary<string, TemplateItem> _itemDb;

    private InventoryConfig _inventoryConfig;

    public CustomItemServiceLootBox(
        DatabaseServer databaseServer,
        ConfigServer configServer,
        ISptLogger<CustomItemServiceLootBox> logger,
        CustomItemService customItemService
    )
    {
        _databaseServer = databaseServer;
        _configServer = configServer;
        _logger = logger;
        _customItemService = customItemService;

        _inventoryConfig = _configServer.GetConfig<InventoryConfig>();
    }

    public void PostDBLoad()
    {
        _itemDb = _databaseServer.GetTables().Templates.Items;

        // Example of adding new item by cloning existing item using createclonedetails
        var crateId = "new_crate_with_randomized_content";
        var exampleCloneItem = new NewItemFromCloneDetails
        {
            // The item we want to clone, in this example i will cloning the sealed weapon crate
            ItemTplToClone = "6489b2b131a2135f0d7d0fcb",
            // ParentId refers to the Node item the container will be under, you can check it in https://db.sp-tarkov.com/search
            ParentId = "62f109593b54472778797866",
            // The new id of our cloned item
            NewId = crateId,
            FleaPriceRoubles = 50000,
            HandbookPriceRoubles = 42500,
            // Handbook Parent Id refers to the category the container will be under
            // Handbook parent can be found in SPT_Data\Server\database\templates.
            HandbookParentId = "62f109593b54472778797866",
            Locales = new Dictionary<string, LocaleDetails>
            {
                {"en", new LocaleDetails
                    {
                        Name = "Custom Lootbox",
                        ShortName = "Custom Lootbox",
                        Description = "A custom lootbox container"
                    }
                }
            },
            OverrideProperties = new Props
            {
                Name = "Custom Lootbox",
                ShortName = "Custom Lootbox",
                Description = "A custom lootbox container",
                Weight = 15
            },
        };

        // Basically calls the function and tell the server to add our Cloned new item into the server
        _customItemService.CreateItemFromClone(exampleCloneItem);

        // Change item _name to remove it from the *actual* sealed weapon crate logic, this removes it from airdrops and allows easier access to change the contents

        var customItemInDb = _itemDb.GetValueOrDefault(crateId);
        customItemInDb.Name = crateId;

        // Add to inventory config with custom item pool
        _inventoryConfig.RandomLootContainers[crateId] = new RewardDetails
        {
            RewardCount = 6,
            FoundInRaid = true,
            RewardTplPool = new Dictionary<string, double>
            {
                {"57514643245977207f2c2d09", 1},
                {"544fb62a4bdc2dfb738b4568", 1},
                {"57513f07245977207e26a311", 1},
                {"57513f9324597720a7128161", 1},
                {"57513fcc24597720a31c09a6", 1},
                {"5e8f3423fd7471236e6e3b64", 1},
                {"60b0f93284c20f0feb453da7", 1},
                {"5734773724597737fd047c14", 1},
                {"59e3577886f774176a362503", 1},
                {"57505f6224597709a92585a9", 1},
                {"544fb6cc4bdc2d34748b456e", 1}
            }
        };
    }

    // Check if our item is in the server or not
    public void PostSptLoad()
    {
        if (_itemDb.TryGetValue("new_crate_with_randomized_content", out var crate))
        {
            _logger.LogWithColor("Item Exists in DB");
            return;
        }

        _logger.LogWithColor("Item Doesn't Exist in DB");
    }
}
