using Core.Helpers;
using System.Reflection;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;
using Core.Models.External;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Routers;
using Core.Servers;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;
using SptCommon.Annotations;
using Path = System.IO.Path;

namespace _13._1AddTraderWithDynamicAssorts;

[Injectable]
public class AddTraderWithDynamicAssorts : IPostDBLoadMod
{
    private readonly ISptLogger<AddTraderWithDynamicAssorts> _logger;
    private readonly ModHelper _modHelper;
    private readonly HashUtil _hashUtil;
    private readonly JsonUtil _jsonUtil;
    private readonly FileUtil _fileUtil;
    private readonly DatabaseService _databaseService;
    private readonly ImageRouter _imageRouter;
    private readonly ConfigServer _configServer;
    private readonly ICloner _cloner;
    private readonly TraderConfig _traderConfig;
    private readonly RagfairConfig _ragfairConfig;

    // This MUST match the folder name of the mod in the user/mods folder
    private const string _modName = "13.1AddTraderWithDynamicAssorts";

    public AddTraderWithDynamicAssorts(
        ISptLogger<AddTraderWithDynamicAssorts> logger,
        ModHelper modHelper,
        HashUtil hashUtil,
        JsonUtil jsonUtil,
        FileUtil fileUtil,
        DatabaseService databaseService,
        ImageRouter imageRouter,
        ConfigServer configServer,
        ICloner cloner        )
    {
        _logger = logger;
        _modHelper = modHelper;
        _hashUtil = hashUtil;
        _jsonUtil = jsonUtil;
        _fileUtil = fileUtil;
        _databaseService = databaseService;
        _imageRouter = imageRouter;
        _configServer = configServer;
        _cloner = cloner;

        _traderConfig = _configServer.GetConfig<TraderConfig>();
        _ragfairConfig = _configServer.GetConfig<RagfairConfig>();
    }

    public void PostDBLoad()
    {
        var pathToMod = _modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());

        var traderImagePath = Path.Combine(pathToMod, "db/cat.jpg");
        var traderBase = _modHelper.GetFileFromModFolder<TraderBase>(pathToMod, "db/base.json");

        // Create helper class and use it to register our traders image/icon + set its stock refresh time
        var addTraderHelper = new AddTraderHelper();
        _imageRouter.AddRoute(traderBase.Avatar.Replace(".jpg", ""), traderImagePath);
        addTraderHelper.SetTraderUpdateTime(_traderConfig, traderBase, 3600, 4000);

        // Add trader to flea market
        _ragfairConfig.Traders[traderBase.Id] = true;

        // Get a reference to the database tables
        var tables = _databaseService.GetTables();

        addTraderHelper.AddTraderToDb(
            traderBase,
            _databaseService.GetTables(),
            _cloner,
            new TraderAssort {Items = [], BarterScheme = new Dictionary<string, List<List<BarterScheme>>>(), LoyalLevelItems = new Dictionary<string, int>()});
        _logger.Success("added trader base");
        var fluentAssortCreator = new FluentTraderAssortCreator(_logger, _hashUtil);

        // Add milk
        fluentAssortCreator
            .CreateSingleAssortItem(ItemTpl.DRINK_PACK_OF_MILK)
            .AddStackCount(200)
            .AddBuyRestriction(10)
            .AddMoneyCost(Money.ROUBLES, 2000)
            .AddLoyaltyLevel(1)
            .Export(tables.Traders[traderBase.Id]);

        // Add 3x bitcoin + salewa for milk barter
        fluentAssortCreator
            .CreateSingleAssortItem(ItemTpl.DRINK_PACK_OF_MILK)
            .AddStackCount(100)
            .AddBarterCost(ItemTpl.BARTER_PHYSICAL_BITCOIN, 3)
            .AddBarterCost(ItemTpl.MEDKIT_SALEWA_FIRST_AID_KIT, 1)
            .AddLoyaltyLevel(1)
            .Export(tables.Traders[traderBase.Id]);


        // Add glock as a money purchase
        fluentAssortCreator
            .CreateComplexAssortItem(addTraderHelper.CreateGlock())
            .AddUnlimitedStackCount()
            .AddMoneyCost(Money.ROUBLES, 20000)
            .AddBuyRestriction(3)
            .AddLoyaltyLevel(1)
            .Export(tables.Traders[traderBase.Id]);

        // Add mp133 preset as a barter for mayonase 
        fluentAssortCreator
            .CreateComplexAssortItem(tables.Globals.ItemPresets["584148f2245977598f1ad387"].Items) // Weapon preset id comes from globals.json
            .AddStackCount(200)
            .AddBarterCost(ItemTpl.FOOD_JAR_OF_DEVILDOG_MAYO, 1)
            .AddBuyRestriction(3)
            .AddLoyaltyLevel(1)
            .Export(tables.Traders[traderBase.Id]);

        addTraderHelper.AddTraderToLocales(
            traderBase,
            _databaseService.GetTables(),
            traderBase.Name,
            "Cat",
            traderBase.Nickname,
            traderBase.Location,
            "This is the cat shop. Meow.");
    }
}

public static class NewItemIds
{
    public static string GLOCK_BASE = "66eeef3b2a166b73d2066a74";
    public static string GLOCK_BARREL = "66eeef3b2a166b73d2066a75";
    public static string GLOCK_RECIEVER = "66eeef3b2a166b73d2066a76";
    public static string GLOCK_COMPENSATOR = "66eeef3b2a166b73d2066a77";
    public static string GLOCK_PISTOL_GRIP = "66eeef3b2a166b73d2066a78";
    public static string GLOCK_REAR_SIGHT = "66eeef3b2a166b73d2066a79";
    public static string GLOCK_FRONT_SIGHT = "66eeef3b2a166b73d2066a7a";
    public static string GLOCK_MAGAZINE = "66eeef3b2a166b73d2066a7b";
}
