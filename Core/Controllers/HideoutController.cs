using Core.Annotations;
using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Hideout;
using Core.Models.Eft.ItemEvent;
using Core.Servers;
using Core.Services;
using Core.Utils;
using ILogger = Core.Models.Utils.ILogger;

namespace Core.Controllers;

[Injectable]
public class HideoutController
{
    private readonly ILogger _logger;
    private readonly HashUtil _hashUtil;
    private readonly TimeUtil _timeUtil;
    private readonly DatabaseService _databaseService;
    private readonly RandomUtil _randomUtil;
    private readonly InventoryHelper _inventoryHelper;
    private readonly ItemHelper _itemHelper;
    private readonly SaveServer _saveServer;

    public HideoutController(
        ILogger logger,
        HashUtil hashUtil,
        TimeUtil timeUtil,
        DatabaseService databaseService,
        RandomUtil randomUtil,
        InventoryHelper inventoryHelper,
        ItemHelper itemHelper,
        SaveServer saveServer)
    {
        _logger = logger;
        _hashUtil = hashUtil;
        _timeUtil = timeUtil;
        _databaseService = databaseService;
        _randomUtil = randomUtil;
        _inventoryHelper = inventoryHelper;
        _itemHelper = itemHelper;
        _saveServer = saveServer;
    }

    /**
     * Handle HideoutCustomizationSetMannequinPose event
     * @param sessionId Session id
     * @param pmcData Player profile
     * @param request Client request data
     * @returns Client response
     */
    public ItemEventRouterResponse HideoutCustomizationSetMannequinPose(string sessionId, PmcData pmcData, HideoutCustomizationSetMannequinPoseRequest request)
    {
        throw new NotImplementedException();
    }
}
