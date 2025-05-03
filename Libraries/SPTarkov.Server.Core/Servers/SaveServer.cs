using System.Collections.Concurrent;
using System.Diagnostics;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.DI;
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
    FileUtil _fileUtil,
    IEnumerable<SaveLoadRouter> _saveLoadRouters,
    JsonUtil _jsonUtil,
    HashUtil _hashUtil,
    LocalisationService _localisationService,
    ISptLogger<SaveServer> _logger,
    ConfigServer _configServer
)
{
    protected const string profileFilepath = "user/profiles/";

    // onLoad = require("../bindings/SaveLoad");
    protected readonly Dictionary<string, Func<SptProfile, SptProfile>> onBeforeSaveCallbacks = new();

    protected ConcurrentDictionary<string, SptProfile> profiles = new();
    protected ConcurrentDictionary<string, string> saveMd5 = new();

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
        if (onBeforeSaveCallbacks.ContainsKey(id))
        {
            onBeforeSaveCallbacks.Remove(id);
        }
    }

    /// <summary>
    ///     Load all profiles in /user/profiles folder into memory (this.profiles)
    /// </summary>
    public void Load()
    {
        // get files to load
        if (!_fileUtil.DirectoryExists(profileFilepath))
        {
            _fileUtil.CreateDirectory(profileFilepath);
        }

        var files = _fileUtil.GetFiles(profileFilepath).Where(item =>
        {
            return _fileUtil.GetFileExtension(item) == "json";
        });

        // load profiles
        var stopwatch = Stopwatch.StartNew();
        foreach (var file in files)
        {
            LoadProfile(_fileUtil.StripExtension(file));
        }

        stopwatch.Stop();
        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug($"{files.Count()} Profiles took: {stopwatch.ElapsedMilliseconds}ms to load.");
        }
    }

    /// <summary>
    ///     Save changes for each profile from memory into user/profiles json
    /// </summary>
    public void Save()
    {
        // Save every profile
        var totalTime = 0L;
        foreach (var sessionID in profiles)
        {
            totalTime += SaveProfile(sessionID.Key);
        }

        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug($"Saved {profiles.Count} profiles, took: {totalTime}ms");
        }
    }

    /// <summary>
    ///     Get a player profile from memory
    /// </summary>
    /// <param name="sessionId"> Session ID </param>
    /// <returns> SptProfile of the player </returns>
    /// <exception cref="Exception"> Thrown when sessionId is null / empty or no profiles with that ID are found </exception>
    public SptProfile GetProfile(string sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            throw new Exception("session id provided was empty, did you restart the server while the game was running?");
        }

        if (profiles == null || profiles.Count == 0)
        {
            throw new Exception($"no profiles found in saveServer with id: {sessionId}");
        }

        if (!profiles.TryGetValue(sessionId, out var sptProfile))
        {
            throw new Exception($"no profile found for sessionId: {sessionId}");
        }

        return sptProfile;
    }

    public bool ProfileExists(string id)
    {
        return profiles.ContainsKey(id);
    }

    /// <summary>
    ///     Gets all profiles from memory
    /// </summary>
    /// <returns> Dictionary of Profiles with their ID as Keys. </returns>
    public Dictionary<string, SptProfile> GetProfiles()
    {
        return profiles.ToDictionary();
    }

    /// <summary>
    ///     Delete a profile by id (Does not remove the profile file!)
    /// </summary>
    /// <param name="sessionID"> ID of profile to remove </param>
    /// <returns> True when deleted, false when profile not found </returns>
    public bool DeleteProfileById(string sessionID)
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
        if (profiles.ContainsKey(profileInfo.ProfileId))
        {
            throw new Exception($"profile already exists for sessionId: {profileInfo.ProfileId}");
        }

        profiles.TryAdd(
            profileInfo.ProfileId,
            new SptProfile
            {
                ProfileInfo = profileInfo,
                CharacterData = new Characters
                {
                    PmcData = new PmcData(),
                    ScavData = new PmcData()
                }
            }
        );
    }

    /// <summary>
    ///     Add full profile in memory by key (info.id)
    /// </summary>
    /// <param name="profileDetails"> Profile to save </param>
    public void AddProfile(SptProfile profileDetails)
    {
        profiles.TryAdd(profileDetails.ProfileInfo.ProfileId, profileDetails);
    }

    /// <summary>
    ///     Look up profile json in user/profiles by id and store in memory. <br />
    ///     Execute saveLoadRouters callbacks after being loaded into memory.
    /// </summary>
    /// <param name="sessionID"> ID of profile to store in memory </param>
    public void LoadProfile(string sessionID)
    {
        var filename = $"{sessionID}.json";
        var filePath = $"{profileFilepath}{filename}";
        if (_fileUtil.FileExists(filePath))
        // File found, store in profiles[]
        {
            profiles[sessionID] = _jsonUtil.DeserializeFromFile<SptProfile>(filePath);
        }

        // Run callbacks
        foreach (var callback in
                 _saveLoadRouters) // HealthSaveLoadRouter, InraidSaveLoadRouter, InsuranceSaveLoadRouter, ProfileSaveLoadRouter. THESE SHOULD EXIST IN HERE
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
    public long SaveProfile(string sessionID)
    {
        var filePath = $"{profileFilepath}{sessionID}.json";

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
                _logger.Error(
                    _localisationService.GetText(
                        "profile_save_callback_error",
                        new
                        {
                            callback,
                            error = e
                        }
                    )
                );
                profiles[sessionID] = previous;
            }
        }

        var start = Stopwatch.StartNew();
        var jsonProfile = _jsonUtil.Serialize(profiles[sessionID], !_configServer.GetConfig<CoreConfig>().Features.CompressProfile);
        var fmd5 = _hashUtil.GenerateMd5ForData(jsonProfile);
        if (!saveMd5.TryGetValue(sessionID, out var currentMd5) || currentMd5 != fmd5)
        {
            saveMd5[sessionID] = fmd5;
            // save profile to disk
            _fileUtil.WriteFile(filePath, jsonProfile);
        }

        start.Stop();
        return start.ElapsedMilliseconds;
    }

    /// <summary>
    ///     Remove a physical profile json from user/profiles
    /// </summary>
    /// <param name="sessionID"> Profile ID to remove </param>
    /// <returns> True if successful </returns>
    public bool RemoveProfile(string sessionID)
    {
        var file = $"{profileFilepath}{sessionID}.json";
        if (profiles.ContainsKey(sessionID))
        {
            profiles.TryRemove(sessionID, out _);
            if (!_fileUtil.DeleteFile(file))
            {
                _logger.Error($"Unable to delete file, not found: {file}");
            }

        }

        return !_fileUtil.FileExists(file);
    }
}
