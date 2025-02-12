using Core.Models.Eft.Common.Tables;
using Core.Models.External;
using Core.Models.Logging;
using Core.Models.Utils;
using Core.Servers;
using SptCommon.Annotations;

namespace _14AfterDBLoadHook;

[Injectable(InjectableTypeOverride = typeof(IPostDBLoadMod))]
[Injectable(InjectableTypeOverride = typeof(IPostSptLoadMod))]
public class AfterDBLoadHook : IPostDBLoadMod, IPostSptLoadMod
{
    private readonly ConfigServer _configServer;
    private readonly DatabaseServer _databaseServer;
    private readonly ISptLogger<AfterDBLoadHook> _logger;
    private Dictionary<string, TemplateItem>? _itemsDb;

    public AfterDBLoadHook(
        ConfigServer configServer,
        DatabaseServer databaseServer,
        ISptLogger<AfterDBLoadHook> logger
    )
    {
        _configServer = configServer;
        _databaseServer = databaseServer;
        _logger = logger;
    }

    public void PostDBLoad()
    {
        _itemsDb = _databaseServer.GetTables().Templates.Items;

        // Database will be loaded, this is the fresh state of the DB so NOTHING from the SPT
        // logic has modified anything yet. This is the DB loaded straight from the JSON files
        _logger.LogWithColor($"Database item size: {_itemsDb.Count}", LogTextColor.Red, LogBackgroundColor.Yellow);

        // lets do a quick modification and see how this reflect later on, on the postSptLoad()
        // find the nvgs item by its Id
        // this also checks if the item exists before giving you the item
        // if it doesn't, this if check will fail
        if (_itemsDb.TryGetValue("5c0558060db834001b735271", out var nvgs))
        {
            // Lets log the state before the modification
            _logger.LogWithColor($"NVGs default CanSellOnRagfair: {nvgs.Properties.CanSellOnRagfair}", LogTextColor.Red, LogBackgroundColor.Yellow);
            // Update one of its properties to be true
            nvgs.Properties.CanSellOnRagfair = true;
        }
    }

    public void PostSptLoad()
    {
        // The modification we made above would have been processed by now by SPT, so any values we changed had
        // already been passed through the initial lifecycles (OnLoad) of SPT.

        if (_itemsDb.TryGetValue("5c0558060db834001b735271", out var nvgs))
        {
            // Lets log the state after the modification
            _logger.LogWithColor($"NVGs default CanSellOnRagfair: {nvgs.Properties.CanSellOnRagfair}", LogTextColor.Red, LogBackgroundColor.Yellow);
        }
    }
}
