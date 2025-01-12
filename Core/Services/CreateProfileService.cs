using Core.Annotations;
using Core.Generators;
using Core.Helpers;
using Core.Models.Eft.Profile;
using Core.Servers;
using Core.Services;
using Core.Utils.Cloners;
using Core.Utils;
using ILogger = Core.Models.Utils.ILogger;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.ItemEvent;

[Injectable]
public class CreateProfileService
{
    private readonly ILogger _logger;
    private readonly TimeUtil _timeUtil;
    private readonly DatabaseService _databaseService;
    private readonly SaveServer _saveServer;
    private readonly ICloner _cloner;

    public CreateProfileService(
        ILogger logger,
        TimeUtil timeUtil,
        DatabaseService databaseService,
        TraderAssortHelper traderAssortHelper,
        TraderAssortService traderAssortService,
        ProfileHelper profileHelper,
        TraderHelper traderHelper,
        PaymentHelper paymentHelper,
        RagfairPriceService ragfairPriceService,
        TraderPurchasePersisterService traderPurchasePersisterService,
        FenceBaseAssortGenerator fenceBaseAssortGenerator,
        ConfigServer configServer,
        SaveServer saveServer,
        ICloner cloner)
    {
        _logger = logger;
        _timeUtil = timeUtil;
        _databaseService = databaseService;
        _saveServer = saveServer;
        _cloner = cloner;
    }

    public string CreateProfile(string sessionID, ProfileCreateRequestData request)
    {
        var account = _saveServer.GetProfile(sessionID).ProfileInfo;
        var profileTemplateClone =
            _cloner.Clone(_databaseService.GetProfiles()[account.Edition][request.Side.ToLower()]);
        var pmcData = profileTemplateClone.Character;

        // Delete existing profile
        DeleteProfileBySessionId(sessionID);
    }

    private void DeleteProfileBySessionId(string sessionId)
    {

    }

    private void UpdateInventoryEquipmentId(PmcData pmcData)
    {

    }

    private void ResetAllTradersInProfile(string sessionId)
    {

    }

    /**
     * Ensure a profile has the necessary internal containers e.g. questRaidItems / sortingTable
     * DOES NOT check that stash exists
     * @param pmcData Profile to check
     */
    private void AddMissingInternalContainersToProfile(PmcData pmcData)
    {

    }

    private void AddCustomisationUnlocksToProfile(SptProfile fullProfile)
    {

    }

    private string GetGameEdition(SptProfile profile)
    {
        return "TODO";
    }

    /**
     * Iterate over all quests in player profile, inspect rewards for the quests current state (accepted/completed)
     * and send rewards to them in mail
     * @param profileDetails Player profile
     * @param sessionID Session id
     * @param response Event router response
     */
    private void GivePlayerStartingQuestRewards( SptProfile fullProfile, string sessionId, ItemEventRouterResponse response)
    {

    }
}
