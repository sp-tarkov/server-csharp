using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
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
public class LauncherV2Controller(
    IReadOnlyList<SptMod> loadedMods,
    HashUtil hashUtil,
    SaveServer saveServer,
    DatabaseService databaseService,
    ServerLocalisationService serverLocalisationService,
    ConfigServer configServer,
    Watermark watermark,
    ProfileController profileController
)
{
    protected readonly CoreConfig CoreConfig = configServer.GetConfig<CoreConfig>();

    /// <summary>
    ///     Returns a simple string of Pong!
    /// </summary>
    /// <returns></returns>
    public string Ping()
    {
        return "Pong!";
    }

    /// <summary>
    ///     Returns all available profile types and descriptions for creation.
    ///     - This is also localised.
    /// </summary>
    /// <returns>dict of profile names + description</returns>
    public Dictionary<string, string> Types()
    {
        var result = new Dictionary<string, string>();
        var dbProfiles = databaseService.GetProfileTemplates();

        foreach (var (templateName, template) in dbProfiles)
        {
            result.TryAdd(templateName, serverLocalisationService.GetText(template.DescriptionLocaleKey));
        }

        return result;
    }

    /// <summary>
    ///     Checks if login details were correct.
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public bool Login(LoginRequestData info)
    {
        var sessionId = GetSessionId(info);

        return !sessionId.IsEmpty;
    }

    /// <summary>
    ///     Register a new profile.
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public async Task<bool> Register(RegisterData info)
    {
        foreach (var (_, profile) in saveServer.GetProfiles())
        {
            if (info.Username == profile.ProfileInfo!.Username)
            {
                return false;
            }
        }

        await CreateAccount(info);
        return true;
    }

    /// <summary>
    ///     Remove profile from server.
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public bool Remove(LoginRequestData info)
    {
        var sessionId = GetSessionId(info);

        return !sessionId.IsEmpty && saveServer.RemoveProfile(sessionId);
    }

    /// <summary>
    ///     Gets the Servers SPT Version.
    ///     - "4.0.0"
    /// </summary>
    /// <returns></returns>
    public string SptVersion()
    {
        return watermark.GetVersionTag();
    }

    /// <summary>
    ///     Gets the compatible EFT Version.
    ///     - "0.14.9.31124"
    /// </summary>
    /// <returns></returns>
    public string EftVersion()
    {
        return CoreConfig.CompatibleTarkovVersion;
    }

    /// <summary>
    ///     Gets the Servers loaded mods.
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, AbstractModMetadata> LoadedMods()
    {
        return loadedMods.ToDictionary(sptMod => sptMod.ModMetadata.Name, sptMod => sptMod.ModMetadata);
    }

    /// <summary>
    ///     Creates the account from provided details.
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

    protected MongoId GetSessionId(LoginRequestData info)
    {
        foreach (var (sessionId, profile) in saveServer.GetProfiles())
        {
            if (info.Username == profile.ProfileInfo!.Username)
            {
                return sessionId;
            }
        }

        return MongoId.Empty();
    }

    public SptProfile GetProfile(MongoId sessionId)
    {
        return saveServer.GetProfile(sessionId);
    }

    public MiniProfile? GetMiniProfileFromUsername(LoginRequestData info)
    {
        return profileController.GetMiniProfile(GetSessionId(info));
    }
}
