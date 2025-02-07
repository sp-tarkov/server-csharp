using System.Diagnostics;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.Profile;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Services;
using Core.Utils;
using SptCommon.Annotations;
using LogLevel = Core.Models.Spt.Logging.LogLevel;

namespace Core.Servers;

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
    private readonly Lock _lock = new();

    // onLoad = require("../bindings/SaveLoad");
    protected readonly Dictionary<string, Func<SptProfile, SptProfile>> onBeforeSaveCallbacks = new();

    protected Dictionary<string, SptProfile> profiles = new();
    protected Dictionary<string, string> saveMd5 = new();

    /**
     * Add callback to occur prior to saving profile changes
     * @param id Id for save callback
     * @param callback Callback to execute prior to running SaveServer.saveProfile()
     */
    public void AddBeforeSaveCallback(string id, Func<SptProfile, SptProfile> callback)
    {
        onBeforeSaveCallbacks[id] = callback;
    }

    /**
     * Remove a callback from being executed prior to saving profile in SaveServer.saveProfile()
     * @param id Id of callback to remove
     */
    public void RemoveBeforeSaveCallback(string id)
    {
        if (onBeforeSaveCallbacks.ContainsKey(id))
        {
            onBeforeSaveCallbacks.Remove(id);
        }
    }

    /**
     * Load all profiles in /user/profiles folder into memory (this.profiles)
     */
    public void Load()
    {
        // get files to load
        if (!_fileUtil.DirectoryExists(profileFilepath))
        {
            _fileUtil.CreateDirectory(profileFilepath);
        }

        var files = _fileUtil.GetFiles(profileFilepath).Where(item => _fileUtil.GetFileExtension(item) == "json");

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

    /**
     * Save changes for each profile from memory into user/profiles json
     */
    public void Save()
    {
        lock (_lock)
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
    }

    /**
     * Get a player profile from memory
     * @param sessionId Session id
     * @returns ISptProfile
     */
    public SptProfile GetProfile(string sessionId)
    {
        lock (_lock)
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
    }

    public bool ProfileExists(string id)
    {
        return profiles.ContainsKey(id);
    }

    /**
     * Get all profiles from memory
     * @returns Dictionary of ISptProfile
     */
    public Dictionary<string, SptProfile> GetProfiles()
    {
        return profiles;
    }

    /**
     * Delete a profile by id
     * @param sessionID Id of profile to remove
     * @returns true when deleted, false when profile not found
     */
    public bool DeleteProfileById(string sessionID)
    {
        lock (_lock)
        {
            if (profiles.ContainsKey(sessionID))
            {
                profiles.Remove(sessionID);
                return true;
            }

            return false;
        }
    }

    /**
     * Create a new profile in memory with empty pmc/scav objects
     * @param profileInfo Basic profile data
     */
    public void CreateProfile(Info profileInfo)
    {
        lock (_lock)
        {
            if (profiles.ContainsKey(profileInfo.ProfileId))
            {
                throw new Exception($"profile already exists for sessionId: {profileInfo.ProfileId}");
            }

            profiles.Add(
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
    }

    /**
     * Add full profile in memory by key (info.id)
     * @param profileDetails Profile to save
     */
    public void AddProfile(SptProfile profileDetails)
    {
        lock (_lock)
        {
            profiles.Add(profileDetails.ProfileInfo.ProfileId, profileDetails);
        }
    }

    /**
     * Look up profile json in user/profiles by id and store in memory
     * Execute saveLoadRouters callbacks after being loaded into memory
     * @param sessionID Id of profile to store in memory
     */
    public void LoadProfile(string sessionID)
    {
        lock (_lock)
        {
            var filename = $"{sessionID}.json";
            var filePath = $"{profileFilepath}{filename}";
            if (_fileUtil.FileExists(filePath))
                // File found, store in profiles[]
            {
                profiles[sessionID] = _jsonUtil.Deserialize<SptProfile>(_fileUtil.ReadFile(filePath));
            }

            // Run callbacks
            foreach (var callback in
                     _saveLoadRouters) // HealthSaveLoadRouter, InraidSaveLoadRouter, InsuranceSaveLoadRouter, ProfileSaveLoadRouter. THESE SHOULD EXIST IN HERE
            {
                profiles[sessionID] = callback.HandleLoad(GetProfile(sessionID));
            }
        }
    }

    /**
     * Save changes from in-memory profile to user/profiles json
     * Execute onBeforeSaveCallbacks callbacks prior to being saved to json
     * @param sessionID profile id (user/profiles/id.json)
     * @returns time taken to save in MS
     */
    public long SaveProfile(string sessionID)
    {
        lock (_lock)
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
    }

    /**
     * Remove a physical profile json from user/profiles
     * @param sessionID Profile id to remove
     * @returns true if file no longer exists
     */
    public bool RemoveProfile(string sessionID)
    {
        lock (_lock)
        {
            var file = $"{profileFilepath}{sessionID}.json";
            if (profiles.ContainsKey(sessionID))
            {
                profiles.Remove(sessionID);
                _fileUtil.DeleteFile(file);
            }

            return !_fileUtil.FileExists(file);
        }
    }
}
