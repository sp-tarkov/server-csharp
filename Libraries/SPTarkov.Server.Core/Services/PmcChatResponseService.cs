using System.Text.RegularExpressions;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Services;

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

    /// <summary>
    ///     For each PMC victim of the player, have a chance to send a message to the player, can be positive or negative
    /// </summary>
    /// <param name="sessionId"> Session ID </param>
    /// <param name="pmcVictims"> List of bots killed by player </param>
    /// <param name="pmcData"> Player profile </param>
    public void SendVictimResponse(string sessionId, List<Victim> pmcVictims, PmcData pmcData)
    {
        foreach (var victim in pmcVictims)
        {
            if (!_randomUtil.GetChance100(_pmcResponsesConfig.Victim.ResponseChancePercent))
            {
                continue;
            }

            if (string.IsNullOrEmpty(victim.Name))
            {
                _logger.Warning($"Victim: {victim.ProfileId} does not have a nickname, skipping pmc response message send");

                continue;
            }

            var victimDetails = GetVictimDetails(victim);
            var message = ChooseMessage(true, pmcData, victim);
            if (message is not null)
            {
                _notificationSendHelper.SendMessageToPlayer(
                    sessionId,
                    victimDetails,
                    message,
                    MessageType.USER_MESSAGE
                );
            }
        }
    }

    /// <summary>
    ///     Not fully implemented yet, needs method of acquiring killers details after raid
    /// </summary>
    /// <param name="sessionId"> Session id </param>
    /// <param name="pmcData"> Players profile </param>
    /// <param name="killer"> The bot who killed the player </param>
    public void SendKillerResponse(string sessionId, PmcData pmcData, Aggressor killer)
    {
        if (killer is null)
        {
            return;
        }

        if (!_randomUtil.GetChance100(_pmcResponsesConfig.Killer.ResponseChancePercent))
        {
            return;
        }

        // find bot by id in cache
        var killerDetailsInCache = _matchBotDetailsCacheService.GetBotById(killer.ProfileId);
        if (killerDetailsInCache is null)
        {
            return;
        }

        // Because we've cached PMC sides as "Savage" for the client,
        // we need to figure out what side it really is
        var side = killerDetailsInCache.Side == DogtagSide.Usec ? "Usec" : "Bear";

        var killerDetails = new UserDialogInfo
        {
            Id = killer.ProfileId,
            Aid = killerDetailsInCache.Aid,
            Info = new UserDialogDetails
            {
                Nickname = killerDetailsInCache.Nickname,
                Side = side,
                Level = killerDetailsInCache.Level,
                MemberCategory = killerDetailsInCache.Type
            }
        };

        var message = ChooseMessage(false, pmcData);
        if (message is null)
        {
            return;
        }

        _notificationSendHelper.SendMessageToPlayer(sessionId, killerDetails, message, MessageType.USER_MESSAGE);
    }

    /// <summary>
    ///     Choose a localised message to send the player (different if sender was killed or killed player)
    /// </summary>
    /// <param name="isVictim"> Is the message coming from a bot killed by the player </param>
    /// <param name="pmcData"> Player profile </param>
    /// <param name="victimData"> OPTIONAL - details of the pmc killed </param>
    /// <returns> Message from PMC to player </returns>
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
            responseText += $" {suffixText}";
        }

        if (StripCapitalisation(isVictim))
        {
            responseText = responseText.ToLower();
        }

        if (AllCaps(isVictim))
        {
            responseText = responseText.ToUpper();
        }

        return responseText;
    }

    /// <summary>
    ///     use map key to get a localised location name
    ///     e.g. factory4_day becomes "Factory"
    /// </summary>
    /// <param name="locationKey"> Location key to localise </param>
    /// <returns> Localised location name </returns>
    protected string GetLocationName(string locationKey)
    {
        return _localeService.GetLocaleDb()[locationKey] ?? locationKey;
    }

    /// <summary>
    ///     Should capitalisation be stripped from the message response before sending
    /// </summary>
    /// <param name="isVictim"> Was responder a victim of player </param>
    /// <returns> True = should be stripped </returns>
    protected bool StripCapitalisation(bool isVictim)
    {
        var chance = isVictim
            ? _pmcResponsesConfig.Victim.StripCapitalisationChancePercent
            : _pmcResponsesConfig.Killer.StripCapitalisationChancePercent;

        return _randomUtil.GetChance100(chance);
    }

    /// <summary>
    ///     Should capitalisation be stripped from the message response before sending
    /// </summary>
    /// <param name="isVictim"> Was responder a victim of player </param>
    /// <returns> True = should be stripped </returns>
    protected bool AllCaps(bool isVictim)
    {
        var chance = isVictim
            ? _pmcResponsesConfig.Victim.AllCapsChancePercent
            : _pmcResponsesConfig.Killer.AllCapsChancePercent;

        return _randomUtil.GetChance100(chance);
    }

    /// <summary>
    ///     Should a suffix be appended to the end of the message being sent to player
    /// </summary>
    /// <param name="isVictim"> Was responder a victim of player </param>
    /// <returns> True = should be appended </returns>
    protected bool AppendSuffixToMessageEnd(bool isVictim)
    {
        var chance = isVictim
            ? _pmcResponsesConfig.Victim.AppendBroToMessageEndChancePercent
            : _pmcResponsesConfig.Killer.AppendBroToMessageEndChancePercent;

        return _randomUtil.GetChance100(chance);
    }

    /// <summary>
    ///     Choose a type of response based on the weightings in pmc response config
    /// </summary>
    /// <param name="isVictim"> Was responder killed by player </param>
    /// <returns> Response type (positive/negative) </returns>
    protected string ChooseResponseType(bool isVictim = true)
    {
        var responseWeights = isVictim
            ? _pmcResponsesConfig.Victim.ResponseTypeWeights
            : _pmcResponsesConfig.Killer.ResponseTypeWeights;

        return _weightedRandomHelper.GetWeightedValue<string>(responseWeights);
    }

    /// <summary>
    ///     Get locale keys related to the type of response to send (victim/killer)
    /// </summary>
    /// <param name="keyType"> Positive/negative </param>
    /// <param name="isVictim"> Was responder killed by player </param>
    /// <returns>List of response locale keys </returns>
    protected List<string> GetResponseLocaleKeys(string keyType, bool isVictim = true)
    {
        var keyBase = isVictim ? "pmcresponse-victim_" : "pmcresponse-killer_";
        var keys = _localisationService.GetKeys();

        return keys.Where(x => x.StartsWith($"{keyBase}{keyType}")).ToList();
    }

    /// <summary>
    ///     Get all locale keys that start with `pmcresponse-suffix`
    /// </summary>
    /// <returns> List of keys </returns>
    protected List<string> GetResponseSuffixLocaleKeys()
    {
        var keys = _localisationService.GetKeys();

        return keys.Where(x => x.StartsWith("pmcresponse-suffix")).ToList();
    }

    /// <summary>
    ///     Randomly draw a victim of the list and return their details
    /// </summary>
    /// <param name="pmcVictims"> Possible victims to choose from </param>
    /// <returns> UserDialogInfo object </returns>
    // TODO: is this used?
    protected UserDialogInfo ChooseRandomVictim(List<Victim> pmcVictims)
    {
        var randomVictim = _randomUtil.GetArrayValue(pmcVictims);

        return GetVictimDetails(randomVictim);
    }

    /// <summary>
    ///     Convert a victim object into a IUserDialogInfo object
    /// </summary>
    /// <param name="pmcVictim"> Victim to convert </param>
    /// <returns> UserDialogInfo object </returns>
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
