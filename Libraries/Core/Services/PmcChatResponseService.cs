using System.Text.RegularExpressions;
using SptCommon.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Profile;
using Core.Models.Utils;
using Core.Servers;
using Core.Utils;
using Core.Helpers;
using Core.Models.Enums;
using Core.Models.Spt.Config;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class PmcChatResponseService(
    ISptLogger<OpenZoneService> _logger,
    HashUtil _hashUtil,
    RandomUtil _randomUtil,
    NotificationSendHelper _notificationSendHelper,
    WeightedRandomHelper _weightedRandomHelper,
    DatabaseService _databaseService,
    LocalisationService _localisationService,
    GiftService _giftService,
    LocaleService _localeService,
    MatchBotDetailsCacheService _matchBotDetailsCacheService,
    ConfigServer _configServer)
{
    protected GiftsConfig _giftConfig = _configServer.GetConfig<GiftsConfig>();
    protected PmcChatResponse _pmcResponsesConfig = _configServer.GetConfig<PmcChatResponse>();

    /**
     * For each PMC victim of the player, have a chance to send a message to the player, can be positive or negative
     * @param sessionId Session id
     * @param pmcVictims List of bots killed by player
     * @param pmcData Player profile
     */
    public void SendVictimResponse(string sessionId, List<Victim> pmcVictims, PmcData pmcData)
    {
        foreach (var victim in pmcVictims)
        {
            if (!_randomUtil.GetChance100(_pmcResponsesConfig.Victim.ResponseChancePercent)) continue;

            if (string.IsNullOrEmpty(victim.Name))
            {
                _logger.Warning($"Victim: {victim.ProfileId} does not have a nickname, skipping pmc response message send");

                continue;
            }

            var victimDetails = GetVictimDetails(victim);
            var message = ChooseMessage(true, pmcData, victim);
            if (message is not null)
                _notificationSendHelper.SendMessageToPlayer(
                    sessionId,
                    victimDetails,
                    message,
                    MessageType.USER_MESSAGE
                );
        }
    }

    /**
     * Not fully implemented yet, needs method of acquiring killers details after raid
     * @param sessionId Session id
     * @param pmcData Players profile
     * @param killer The bot who killed the player
     */
    public void SendKillerResponse(string sessionId, PmcData pmcData, Aggressor killer)
    {
        if (killer is null) return;

        if (!_randomUtil.GetChance100(_pmcResponsesConfig.Killer.ResponseChancePercent)) return;

        // find bot by name in cache
        var killerDetailsInCache = _matchBotDetailsCacheService.GetBotByNameAndSide(killer.Name, killer.Side);
        if (killerDetailsInCache is null) return;

        // If killer wasn't a PMC, skip
        var pmcTypes = new List<string> { "pmcUSEC", "pmcBEAR" };
        if (!pmcTypes.Contains(killerDetailsInCache.Info.Settings.Role)) return;

        var killerDetails = new UserDialogInfo
        {
            Id = killerDetailsInCache.Id,
            Aid = _hashUtil.GenerateAccountId(), // TODO: pass correct value
            Info = new UserDialogDetails
            {
                Nickname = killerDetailsInCache.Info.Nickname,
                Side = killerDetailsInCache.Info.Side,
                Level = killerDetailsInCache.Info.Level,
                MemberCategory = killerDetailsInCache.Info.MemberCategory,
                SelectedMemberCategory = killerDetailsInCache.Info.SelectedMemberCategory
            }
        };

        var message = ChooseMessage(false, pmcData);
        if (message is null) return;

        _notificationSendHelper.SendMessageToPlayer(sessionId, killerDetails, message, MessageType.USER_MESSAGE);
    }

    /**
     * Choose a localised message to send the player (different if sender was killed or killed player)
     * @param isVictim Is the message coming from a bot killed by the player
     * @param pmcData Player profile
     * @param victimData OPTIMAL - details of the pmc killed
     * @returns Message from PMC to player
     */
    protected string? ChooseMessage(bool isVictim, PmcData pmcData, Victim? victimData = null)
    {
        // Positive/negative etc
        var responseType = ChooseResponseType(isVictim);

        // Get all locale keys
        var possibleResponseLocaleKeys = GetResponseLocaleKeys(responseType, isVictim);
        if (possibleResponseLocaleKeys.Count == 0)
        {
            _logger.Warning(_localisationService.GetText("pmcresponse-unable_to_find_key", responseType));

            return null;
        }

        // Choose random response from above list and request it from localisation service
        var responseText = _localisationService.GetText(
            _randomUtil.GetArrayValue(possibleResponseLocaleKeys),
            new
            {
                playerName = pmcData.Info.Nickname,
                playerLevel = pmcData.Info.Level,
                playerSide = pmcData.Info.Side,
                victimDeathLocation = victimData is not null ? GetLocationName(victimData.Location) : ""
            }
        );

        // Give the player a gift code if they were killed and response is 'pity'.
        if (responseType == "pity")
        {
            var giftKeys = _giftService.GetGiftIds();
            var randomGiftKey = _randomUtil.GetArrayValue(giftKeys);

            Regex.Replace(responseText, "/(%giftcode%)/gi", randomGiftKey); // TODO: does regex still work
        }

        if (AppendSuffixToMessageEnd(isVictim))
        {
            var suffixText = _localisationService.GetText(_randomUtil.GetArrayValue(GetResponseSuffixLocaleKeys()));
            responseText += $"{suffixText}";
        }

        if (StripCapitalisation(isVictim)) responseText = responseText.ToLower();

        if (AllCaps(isVictim)) responseText = responseText.ToUpper();

        return responseText;
    }

    /**
     * use map key to get a localised location name
     * e.g. factory4_day becomes "Factory"
     * @param locationKey location key to localise
     * @returns Localised location name
     */
    protected string GetLocationName(string locationKey)
    {
        return _localeService.GetLocaleDb()[locationKey] ?? locationKey;
    }

    /**
     * Should capitalisation be stripped from the message response before sending
     * @param isVictim Was responder a victim of player
     * @returns true = should be stripped
     */
    protected bool StripCapitalisation(bool isVictim)
    {
        var chance = isVictim
            ? _pmcResponsesConfig.Victim.StripCapitalisationChancePercent
            : _pmcResponsesConfig.Killer.StripCapitalisationChancePercent;

        return _randomUtil.GetChance100(chance);
    }

    /**
     * Should capitalisation be stripped from the message response before sending
     * @param isVictim Was responder a victim of player
     * @returns true = should be stripped
     */
    protected bool AllCaps(bool isVictim)
    {
        var chance = isVictim
            ? _pmcResponsesConfig.Victim.AllCapsChancePercent
            : _pmcResponsesConfig.Killer.AllCapsChancePercent;

        return _randomUtil.GetChance100(chance);
    }

    /**
     * Should a suffix be appended to the end of the message being sent to player
     * @param isVictim Was responder a victim of player
     * @returns true = should be stripped
     */
    protected bool AppendSuffixToMessageEnd(bool isVictim)
    {
        var chance = isVictim
            ? _pmcResponsesConfig.Victim.AppendBroToMessageEndChancePercent
            : _pmcResponsesConfig.Killer.AppendBroToMessageEndChancePercent;

        return _randomUtil.GetChance100(chance);
    }

    /**
     * Choose a type of response based on the weightings in pmc response config
     * @param isVictim Was responder killed by player
     * @returns Response type (positive/negative)
     */
    protected string ChooseResponseType(bool isVictim = true)
    {
        var responseWeights = isVictim
            ? _pmcResponsesConfig.Victim.ResponseTypeWeights
            : _pmcResponsesConfig.Killer.ResponseTypeWeights;

        return _weightedRandomHelper.GetWeightedValue<string>(responseWeights);
    }

    /**
     * Get locale keys related to the type of response to send (victim/killer)
     * @param keyType Positive/negative
     * @param isVictim Was responder killed by player
     * @returns
     */
    protected List<string> GetResponseLocaleKeys(string keyType, bool isVictim = true)
    {
        var keyBase = isVictim ? "pmcresponse-victim_" : "pmcresponse-killer_";
        var keys = _localisationService.GetKeys();

        return keys.Where((x) => x.StartsWith($"{keyBase}{keyType}")).ToList();
    }

    /**
     * Get all locale keys that start with `pmcresponse-suffix`
     * @returns list of keys
     */
    protected List<string> GetResponseSuffixLocaleKeys()
    {
        var keys = _localisationService.GetKeys();

        return keys.Where((x) => x.StartsWith("pmcresponse-suffix")).ToList();
    }

    /**
     * TODO: is this used?
     * Randomly draw a victim of the list and return their details
     * @param pmcVictims Possible victims to choose from
     * @returns IUserDialogInfo
     */
    protected UserDialogInfo ChooseRandomVictim(List<Victim> pmcVictims)
    {
        var randomVictim = _randomUtil.GetArrayValue(pmcVictims);

        return GetVictimDetails(randomVictim);
    }

    /**
     * Convert a victim object into a IUserDialogInfo object
     * @param pmcVictim victim to convert
     * @returns IUserDialogInfo
     */
    protected UserDialogInfo GetVictimDetails(Victim pmcVictim)
    {
        var categories = new List<MemberCategory>
        {
            MemberCategory.UniqueId,
            MemberCategory.Default,
            MemberCategory.Default,
            MemberCategory.Default,
            MemberCategory.Default,
            MemberCategory.Default,
            MemberCategory.Default,
            MemberCategory.Sherpa,
            MemberCategory.Developer
        };

        var chosenCategory = _randomUtil.GetArrayValue(categories);

        return new UserDialogInfo
        {
            Id = pmcVictim.ProfileId,
            Aid = int.Parse(pmcVictim.AccountId),
            Info = new UserDialogDetails
            {
                Nickname = pmcVictim.Name,
                Level = pmcVictim.Level,
                Side = pmcVictim.Side,
                MemberCategory = chosenCategory,
                SelectedMemberCategory = chosenCategory
            }
        };
    }
}
