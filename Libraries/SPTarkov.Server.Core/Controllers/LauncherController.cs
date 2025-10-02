using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Launcher;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using Info = SPTarkov.Server.Core.Models.Eft.Profile.Info;

namespace SPTarkov.Server.Core.Controllers;

[Injectable]
public class LauncherController(
    IReadOnlyList<SptMod> loadedMods,
    HashUtil hashUtil,
    SaveServer saveServer,
    HttpServerHelper httpServerHelper,
    ProfileHelper profileHelper,
    DatabaseService databaseService,
    ServerLocalisationService serverLocalisationService,
    ConfigServer configServer
)
{
    protected readonly CoreConfig CoreConfig = configServer.GetConfig<CoreConfig>();

    /// <summary>
    ///     Handle launcher connecting to server
    /// </summary>
    /// <returns>ConnectResponse</returns>
    public ConnectResponse Connect()
    {
        // Get all possible profile types + filter out any that are blacklisted
        var profileTemplates = databaseService
            .GetProfileTemplates()
            .Where(profile => !CoreConfig.Features.CreateNewProfileTypesBlacklist.Contains(profile.Key))
            .ToDictionary();

        return new ConnectResponse
        {
            BackendUrl = httpServerHelper.GetBackendUrl(),
            Name = CoreConfig.ServerName,
            Editions = profileTemplates.Select(x => x.Key).ToList(),
            ProfileDescriptions = GetProfileDescriptions(profileTemplates),
        };
    }

    /// <summary>
    ///     Get descriptive text for each of the profile editions a player can choose, keyed by profile.json profile type e.g. "Edge Of Darkness"
    /// </summary>
    /// <param name="profileTemplates">Profiles to get descriptions of</param>
    /// <returns>Dictionary of profile types with related descriptive text</returns>
    protected Dictionary<string, string> GetProfileDescriptions(Dictionary<string, ProfileSides> profileTemplates)
    {
        var result = new Dictionary<string, string>();
        foreach (var (profileKey, profile) in profileTemplates)
        {
            result.TryAdd(profileKey, serverLocalisationService.GetText(profile.DescriptionLocaleKey));
        }

        return result;
    }

    /// <summary>
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns></returns>
    public Info? Find(MongoId sessionId)
    {
        return saveServer.GetProfiles().TryGetValue(sessionId, out var profile) ? profile.ProfileInfo : null;
    }

    /// <summary>
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public MongoId Login(LoginRequestData? info)
    {
        foreach (var (sessionId, profile) in saveServer.GetProfiles())
        {
            var account = profile.ProfileInfo;
            if (info?.Username == account?.Username)
            {
                return sessionId;
            }
        }

        return MongoId.Empty();
    }

    /// <summary>
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public async Task<MongoId> Register(RegisterData info)
    {
        foreach (var (_, profile) in saveServer.GetProfiles())
        {
            if (info.Username == profile.ProfileInfo?.Username)
            {
                return MongoId.Empty();
            }
        }

        return await CreateAccount(info);
    }

    /// <summary>
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    protected async Task<MongoId> CreateAccount(RegisterData info)
    {
        var profileId = new MongoId();
        var scavId = new MongoId();
        var newProfileDetails = new Info
        {
            ProfileId = profileId,
            ScavengerId = scavId,
            Aid = hashUtil.GenerateAccountId(),
            Username = info.Username,
            IsWiped = true,
            Edition = info.Edition,
        };
        saveServer.CreateProfile(newProfileDetails);

        await saveServer.LoadProfileAsync(profileId);
        await saveServer.SaveProfileAsync(profileId);

        return profileId;
    }

    /// <summary>
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public MongoId ChangeUsername(ChangeRequestData info)
    {
        var sessionID = Login(info);

        if (!sessionID.IsEmpty)
        {
            saveServer.GetProfile(sessionID).ProfileInfo!.Username = info.Change;
        }

        return sessionID;
    }

    /// <summary>
    ///     Handle launcher requesting profile be wiped
    /// </summary>
    /// <param name="info">Registration data</param>
    /// <returns>Session id</returns>
    public MongoId Wipe(RegisterData info)
    {
        if (!CoreConfig.AllowProfileWipe)
        {
            return MongoId.Empty();
        }

        var sessionId = Login(info);

        if (!sessionId.IsEmpty)
        {
            var profileInfo = saveServer.GetProfile(sessionId).ProfileInfo;
            profileInfo!.Edition = info.Edition;
            profileInfo.IsWiped = true;
        }

        return sessionId;
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public string GetCompatibleTarkovVersion()
    {
        return CoreConfig.CompatibleTarkovVersion;
    }

    /// <summary>
    ///     Get the mods the server has currently loaded
    /// </summary>
    /// <returns>Dictionary of mod name and mod details</returns>
    public Dictionary<string, AbstractModMetadata> GetLoadedServerMods()
    {
        return loadedMods.ToDictionary(sptMod => sptMod.ModMetadata?.Name ?? "UNKNOWN MOD", sptMod => sptMod.ModMetadata);
    }

    /// <summary>
    ///     Get the mods a profile has ever loaded into game with
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns>Array of mod details</returns>
    public List<ModDetails> GetServerModsProfileUsed(MongoId sessionId)
    {
        var profile = profileHelper.GetFullProfile(sessionId);

        if (profile?.SptData?.Mods is not null)
        {
            return GetProfileModsGroupedByModName(profile?.SptData?.Mods);
        }

        return [];
    }

    /// <summary>
    /// </summary>
    /// <param name="profileMods"></param>
    /// <returns></returns>
    public List<ModDetails> GetProfileModsGroupedByModName(List<ModDetails> profileMods)
    {
        // Group all mods used by profile by name
        var modsGroupedByName = new Dictionary<string, List<ModDetails>>();
        foreach (var mod in profileMods)
        {
            if (!modsGroupedByName.ContainsKey(mod.Name))
            {
                modsGroupedByName[mod.Name] = [];
            }

            modsGroupedByName[mod.Name].Add(mod);
        }

        // Find the highest versioned mod and add to results array
        var result = new List<ModDetails>();
        foreach (var (modName, modDatas) in modsGroupedByName)
        {
            var modVersions = modDatas.Select(x => x.Version);
            // var highestVersion = MaxSatisfying(modVersions, "*"); ?? TODO: Node used SemVer here

            var chosenVersion = modDatas.FirstOrDefault(x => x.Name == modName); // && x.Version == highestVersion
            if (chosenVersion is null)
            {
                continue;
            }

            result.Add(chosenVersion);
        }

        return result;
    }
}
