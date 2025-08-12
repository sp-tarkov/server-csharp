using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Servers;

[Injectable(InjectionType.Singleton)]
public class SaveServer(
    FileUtil fileUtil,
    IEnumerable<SaveLoadRouter> saveLoadRouters,
    JsonUtil jsonUtil,
    HashUtil hashUtil,
    ServerLocalisationService serverLocalisationService,
    ProfileValidatorService profileValidatorService,
    ISptLogger<SaveServer> logger,
    ConfigServer configServer
)
{
    protected const string profileFilepath = "user/profiles/";

    // onLoad = require("../bindings/SaveLoad");
    protected readonly Dictionary<string, Func<SptProfile, SptProfile>> onBeforeSaveCallbacks = new();

    protected readonly ConcurrentDictionary<MongoId, SptProfile> profiles = new();
    protected readonly ConcurrentDictionary<MongoId, string> saveMd5 = new();

    /// <summary>
    ///     Add callback to occur prior to saving profile changes
    /// </summary>
    /// <param name="id"> ID for the save callback </param>
    /// <param name="callback"> Callback to execute prior to running SaveServer.saveProfile() </param>
    public void AddBeforeSaveCallback(string id, Func<SptProfile, SptProfile> callback)
    {
        onBeforeSaveCallbacks[id] = callback;
    }

    /// <summary>
    ///     Remove a callback from being executed prior to saving profile in SaveServer.saveProfile()
    /// </summary>
    /// <param name="id"> ID of Callback to remove </param>
    public void RemoveBeforeSaveCallback(string id)
    {
        onBeforeSaveCallbacks.Remove(id);
    }

    /// <summary>
    ///     Load all profiles in /user/profiles folder into memory (this.profiles)
    /// </summary>
    public async Task LoadAsync()
    {
        // get files to load
        if (!fileUtil.DirectoryExists(profileFilepath))
        {
            fileUtil.CreateDirectory(profileFilepath);
        }

        var files = fileUtil.GetFiles(profileFilepath).Where(item => fileUtil.GetFileExtension(item) == "json");

        // load profiles
        var stopwatch = Stopwatch.StartNew();
        foreach (var file in files)
        {
            await LoadProfileAsync(fileUtil.StripExtension(file));
        }

        stopwatch.Stop();
        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug($"{files.Count()} Profiles took: {stopwatch.ElapsedMilliseconds}ms to load.");
        }
    }

    /// <summary>
    ///     Save changes for each profile from memory into user/profiles json
    /// </summary>
    public async Task SaveAsync()
    {
        // Save every profile
        var totalTime = 0L;
        foreach (var sessionID in profiles)
        {
            totalTime += await SaveProfileAsync(sessionID.Key);
        }

        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug($"Saved {profiles.Count} profiles, took: {totalTime}ms");
        }
    }

    /// <summary>
    ///     Get a player profile from memory
    /// </summary>
    /// <param name="sessionId"> Session ID </param>
    /// <returns> SptProfile of the player </returns>
    /// <exception cref="Exception"> Thrown when sessionId is null / empty or no profiles with that ID are found </exception>
    public SptProfile GetProfile(MongoId sessionId)
    {
        if (sessionId.IsEmpty)
        {
            throw new Exception("session id provided was empty, did you restart the server while the game was running?");
        }

        if (profiles == null || profiles.IsEmpty)
        {
            throw new Exception($"no profiles found in saveServer with id: {sessionId}");
        }

        if (!profiles.TryGetValue(sessionId, out var sptProfile))
        {
            throw new Exception($"no profile found for sessionId: {sessionId}");
        }

        return sptProfile;
    }

    public bool ProfileExists(MongoId id)
    {
        return profiles.ContainsKey(id);
    }

    /// <summary>
    ///     Gets all profiles from memory
    /// </summary>
    /// <returns> Dictionary of Profiles with their ID as Keys. </returns>
    public Dictionary<MongoId, SptProfile> GetProfiles()
    {
        return profiles.ToDictionary();
    }

    /// <summary>
    ///     Delete a profile by id (Does not remove the profile file!)
    /// </summary>
    /// <param name="sessionID"> ID of profile to remove </param>
    /// <returns> True when deleted, false when profile not found </returns>
    public bool DeleteProfileById(MongoId sessionID)
    {
        if (profiles.ContainsKey(sessionID))
        {
            if (profiles.TryRemove(sessionID, out _))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Create a new profile in memory with empty pmc/scav objects
    /// </summary>
    /// <param name="profileInfo"> Basic profile data </param>
    /// <exception cref="Exception"> Thrown when profile already exists </exception>
    public void CreateProfile(Info profileInfo)
    {
        if (!profileInfo.ProfileId.HasValue)
        {
            // TODO: Localize me
            throw new Exception("Creating profile failed: profile has no sessionId");
        }

        if (profiles.ContainsKey(profileInfo.ProfileId.Value))
        {
            // TODO: Localize me
            throw new Exception($"Creating profile failed: profile already exists for sessionId: {profileInfo.ProfileId}");
        }

        profiles.TryAdd(
            profileInfo.ProfileId.Value,
            new SptProfile
            {
                ProfileInfo = profileInfo,
                CharacterData = new Characters { PmcData = new PmcData(), ScavData = new PmcData() },
            }
        );
    }

    /// <summary>
    ///     Add full profile in memory by key (info.id)
    /// </summary>
    /// <param name="profileDetails"> Profile to save </param>
    public void AddProfile(SptProfile profileDetails)
    {
        profiles.TryAdd(profileDetails.ProfileInfo!.ProfileId!.Value, profileDetails);
    }

    /// <summary>
    ///     Look up profile json in user/profiles by id and store in memory. <br />
    ///     Execute saveLoadRouters callbacks after being loaded into memory.
    /// </summary>
    /// <param name="sessionID"> ID of profile to store in memory </param>
    public async Task LoadProfileAsync(MongoId sessionID)
    {
        var filename = $"{sessionID.ToString()}.json";
        var filePath = $"{profileFilepath}{filename}";
        if (fileUtil.FileExists(filePath))
        // File found, store in profiles[]
        {
            var profile = await jsonUtil.DeserializeFromFileAsync<JsonObject>(filePath);

            if (profile is not null)
            {
                try
                {
                    profiles[sessionID] = profileValidatorService.MigrateAndValidateProfile(profile);
                }
                catch (InvalidOperationException ex)
                {
                    logger.Critical($"Failed to load profile with ID '{sessionID}'");
                    logger.Critical(ex.ToString());
                }
            }
        }

        // We don't proceed further here as only one object in the profile has data in it.
        if (IsProfileInvalidOrUnloadable(sessionID))
        {
            return;
        }

        // Run callbacks
        foreach (var callback in saveLoadRouters) // HealthSaveLoadRouter, InraidSaveLoadRouter, InsuranceSaveLoadRouter, ProfileSaveLoadRouter. THESE SHOULD EXIST IN HERE
        {
            profiles[sessionID] = callback.HandleLoad(GetProfile(sessionID));
        }
    }

    /// <summary>
    ///     Save changes from in-memory profile to user/profiles json
    ///     Execute onBeforeSaveCallbacks callbacks prior to being saved to json
    /// </summary>
    /// <param name="sessionID"> Profile id (user/profiles/id.json) </param>
    /// <returns> Time taken to save the profile in seconds </returns>
    public async Task<long> SaveProfileAsync(MongoId sessionID)
    {
        // No need to save profiles that have been marked as invalid
        if (IsProfileInvalidOrUnloadable(sessionID))
        {
            return 0;
        }

        var filePath = $"{profileFilepath}{sessionID.ToString()}.json";

        // Run pre-save callbacks before we save into json
        foreach (var callback in onBeforeSaveCallbacks)
        {
            var previous = profiles[sessionID];
            try
            {
                profiles[sessionID] = onBeforeSaveCallbacks[callback.Key](profiles[sessionID]);
            }
            catch (Exception e)
            {
                logger.Error(serverLocalisationService.GetText("profile_save_callback_error", new { callback, error = e }));
                profiles[sessionID] = previous;
            }
        }

        var start = Stopwatch.StartNew();
        var jsonProfile = jsonUtil.Serialize(profiles[sessionID], !configServer.GetConfig<CoreConfig>().Features.CompressProfile);
        var fmd5 = await hashUtil.GenerateHashForDataAsync(HashingAlgorithm.MD5, jsonProfile);
        if (!saveMd5.TryGetValue(sessionID, out var currentMd5) || currentMd5 != fmd5)
        {
            saveMd5[sessionID] = fmd5;
            // save profile to disk
            await fileUtil.WriteFileAsync(filePath, jsonProfile);
        }

        start.Stop();
        return start.ElapsedMilliseconds;
    }

    /// <summary>
    ///     Remove a physical profile json from user/profiles
    /// </summary>
    /// <param name="sessionID"> Profile ID to remove </param>
    /// <returns> True if successful </returns>
    public bool RemoveProfile(MongoId sessionID)
    {
        var file = $"{profileFilepath}{sessionID}.json";
        if (profiles.ContainsKey(sessionID))
        {
            profiles.TryRemove(sessionID, out _);
            if (!fileUtil.DeleteFile(file))
            {
                logger.Error($"Unable to delete file, not found: {file}");
            }
        }

        return !fileUtil.FileExists(file);
    }

    /// <summary>
    /// Determines whether the specified profile is marked as invalid or cannot be loaded.
    /// </summary>
    /// <param name="sessionID">The ID of the profile to check.</param>
    /// <returns>
    /// <c>true</c> if the profile is invalid or unloadable; otherwise, <c>false</c>.
    /// </returns>
    public bool IsProfileInvalidOrUnloadable(MongoId sessionID)
    {
        if (
            profiles.TryGetValue(sessionID, out var profile)
            && profile.ProfileInfo!.InvalidOrUnloadableProfile is not null
            && profile.ProfileInfo!.InvalidOrUnloadableProfile!.Value
        )
        {
            return true;
        }

        return false;
    }
}
