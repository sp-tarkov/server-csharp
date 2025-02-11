using Core.Helpers;
using System.Reflection;
using Core.Models.Eft.Common.Tables;
using Core.Models.External;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Routers;
using Core.Servers;
using Core.Services;
using Core.Utils.Cloners;
using SptCommon.Annotations;
using Path = System.IO.Path;

namespace _13AddTraderWithAssortJson;

[Injectable]
public class AddTraderWithAssortJson : IPostDBLoadMod
{
    private readonly ISptLogger<AddTraderWithAssortJson> _logger;
    private readonly ModHelper _modHelper;
    private readonly DatabaseService _databaseService;
    private readonly ImageRouter _imageRouter;
    private readonly ConfigServer _configServer;
    private readonly ICloner _cloner;
    private readonly TraderConfig _traderConfig;
    private readonly RagfairConfig _ragfairConfig;

    public AddTraderWithAssortJson(
        ISptLogger<AddTraderWithAssortJson> logger,
        ModHelper modHelper,
        DatabaseService databaseService,
        ImageRouter imageRouter,
        ConfigServer configServer,
        ICloner cloner)
    {
        _logger = logger;
        _modHelper = modHelper;
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

        var assort = _modHelper.GetFileFromModFolder<TraderAssort>(pathToMod, "db/assort.json");

        // Create helper class and use it to register our traders image/icon + set its stock refresh time
        var addTraderHelper = new AddTraderHelper();
        _imageRouter.AddRoute(traderBase.Avatar.Replace(".jpg", ""), traderImagePath);
        addTraderHelper.SetTraderUpdateTime(_traderConfig, traderBase, 3600, 4000);

        // Add trader to flea market
        _ragfairConfig.Traders[traderBase.Id] = true;

        // Add new trader to the trader dictionary in DatabaseServer - this is where the assort json is loaded
        /*
         * The assortJSON includes the following:
         * Milk available for roubles at LL1
         * Milk available for item barter at LL1
         * Helmet w/ soft armour available for roubles at LL1
         * Helmet w/ soft armour available for item barter at LL1
         *
         * It is *REQUIRED* to use MongoIDs for IDs in the assort
         */
        addTraderHelper.AddTraderToDb(
            traderBase,
            _databaseService.GetTables(),
            _cloner,
            assort);

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
