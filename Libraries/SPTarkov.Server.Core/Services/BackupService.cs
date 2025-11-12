using System.Globalization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using LogLevel = SPTarkov.Common.Models.Logging.LogLevel;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class BackupService
{
    protected const string ProfileDir = "./user/profiles";
    protected const string activeModsFilename = "activeMods.json";

    protected readonly List<string> ActiveServerMods;
    protected readonly BackupConfig BackupConfig;

    // Runs Init() every x minutes
    protected Timer _backupIntervalTimer;

    protected readonly SemaphoreSlim BackupLock = new SemaphoreSlim(1, 1);
    protected long LastBackupTimestamp;

    protected readonly FileUtil FileUtil;
    protected readonly JsonUtil JsonUtil;
    protected readonly ISptLogger<BackupService> Logger;
    protected readonly TimeUtil TimeUtil;
    protected readonly IReadOnlyList<SptMod> LoadedMods;

    public BackupService(
        ISptLogger<BackupService> logger,
        IReadOnlyList<SptMod> loadedMods,
        JsonUtil jsonUtil,
        TimeUtil timeUtil,
        ConfigServer configServer,
        FileUtil fileUtil
    )
    {
        Logger = logger;
        JsonUtil = jsonUtil;
        TimeUtil = timeUtil;
        FileUtil = fileUtil;
        LoadedMods = loadedMods;

        ActiveServerMods = GetActiveServerMods();
        BackupConfig = configServer.GetConfig<BackupConfig>();
    }

    /// <summary>
    ///     Start the backup interval if enabled in config.
    /// </summary>
    public async Task StartBackupSystem()
    {
        if (!BackupConfig.BackupInterval.Enabled)
        {
            // Not backing up at regular intervals, run once and exit
            await Init();

            return;
        }

        _backupIntervalTimer = new Timer(
            async void (_) =>
            {
                try
                {
                    await Init();
                }
                catch (Exception ex)
                {
                    Logger.Error($"Profile backup failed: {ex.Message}, {ex.StackTrace}");
                }
            },
            null,
            TimeSpan.Zero,
            TimeSpan.FromMinutes(BackupConfig.BackupInterval.IntervalMinutes)
        );
    }

    /// <summary>
    ///     Run the backup process. <br />
    ///     This method orchestrates the profile backup service. Handles copying profiles to a backup directory and cleaning
    ///     up old backups if the number exceeds the configured maximum.
    /// </summary>
    public async Task Init()
    {
        if (!IsEnabled())
        {
            return;
        }

        // If the backup lock is already locked, skip backup. This stops multiple backups running at once
        // Passing 0 is a non-blocking Wait, will return false if the lock can't be acquired
        var lockAcquired = await BackupLock.WaitAsync(0);
        if (!lockAcquired)
        {
            return;
        }

        try
        {
            // Make sure we don't back up too often by using a configurable Cooldown
            var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (currentTimestamp < LastBackupTimestamp + BackupConfig.BackupCooldown)
            {
                return;
            }
            LastBackupTimestamp = currentTimestamp;

            var targetDir = GenerateBackupTargetDir();

            // Fetch all profiles in the profile directory.
            List<string> currentProfilePaths;
            try
            {
                currentProfilePaths = FileUtil.GetFiles(ProfileDir);
            }
            catch (Exception ex)
            {
                Logger.Debug($"Skipping profile backup: Unable to read profiles directory, {ex.Message}");
                return;
            }

            if (currentProfilePaths.Count == 0)
            {
                if (Logger.IsLogEnabled(LogLevel.Debug))
                {
                    Logger.Debug("No profiles to backup");
                }

                return;
            }

            try
            {
                FileUtil.CreateDirectory(targetDir);

                foreach (var profilePath in currentProfilePaths)
                {
                    // Get filename + extension, removing the path
                    var profileFileName = FileUtil.GetFileNameAndExtension(profilePath);

                    // Create absolute path to file
                    var relativeSourceFilePath = Path.Combine(ProfileDir, profileFileName);
                    var absoluteDestinationFilePath = Path.Combine(targetDir, profileFileName);
                    if (!FileUtil.CopyFile(relativeSourceFilePath, absoluteDestinationFilePath))
                    {
                        Logger.Error($"Source file not found: {relativeSourceFilePath}. Cannot copy to: {absoluteDestinationFilePath}");
                    }
                }

                // Write a copy of active mods.
                await FileUtil.WriteFileAsync(Path.Combine(targetDir, activeModsFilename), JsonUtil.Serialize(ActiveServerMods)!);

                if (Logger.IsLogEnabled(LogLevel.Debug))
                {
                    Logger.Debug($"Profile backup created in: {targetDir}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Unable to write to backup profile directory: {ex.Message}");
                return;
            }

            CleanBackups();
        }
        finally
        {
            BackupLock.Release();
        }
    }

    /// <summary>
    ///     Check to see if the backup service is enabled via the config.
    /// </summary>
    /// <returns> True if enabled, false otherwise. </returns>
    protected bool IsEnabled()
    {
        if (BackupConfig.Enabled)
        {
            return true;
        }

        if (Logger.IsLogEnabled(LogLevel.Debug))
        {
            Logger.Debug("Profile backups disabled");
        }

        return false;
    }

    /// <summary>
    ///     Generates the target directory path for the backup. The directory path is constructed using the `directory` from
    ///     the configuration and the current backup date.
    /// </summary>
    /// <returns> The target directory path for the backup. </returns>
    protected string GenerateBackupTargetDir()
    {
        var backupDate = GenerateBackupDate();
        return Path.GetFullPath($"{BackupConfig.Directory}/{backupDate}");
    }

    /// <summary>
    ///     Generates a formatted backup date string in the format `YYYY-MM-DD_hh-mm-ss`.
    /// </summary>
    /// <returns> The formatted backup date string. </returns>
    protected string GenerateBackupDate()
    {
        return TimeUtil.GetDateTimeNow().ToString("yyyy-MM-dd_HH-mm-ss");
    }

    /// <summary>
    ///     Cleans up old backups in the backup directory. <br />
    ///     This method reads the backup directory, and sorts backups by modification time. If the number of backups exceeds
    ///     the configured maximum, it deletes the oldest backups.
    /// </summary>
    protected void CleanBackups()
    {
        var backupDir = BackupConfig.Directory;
        var backupPaths = GetBackupPaths(backupDir);

        // Filter out invalid backup paths by ensuring they contain a valid date.
        var backupPathsWithCreationDateTime = GetBackupPathsWithCreationTimestamp(backupPaths);
        var excessCount = backupPathsWithCreationDateTime.Count - BackupConfig.MaxBackups;
        if (excessCount > 0)
        {
            var excessBackupPaths = backupPaths.GetRange(0, excessCount);
            RemoveExcessBackups(excessBackupPaths);
        }
    }

    protected SortedDictionary<long, string> GetBackupPathsWithCreationTimestamp(IEnumerable<string> backupPaths)
    {
        var result = new SortedDictionary<long, string>();
        foreach (var backupPath in backupPaths)
        {
            var date = ExtractDateFromFolderName(backupPath);
            if (!date.HasValue)
            {
                continue;
            }

            result.Add(date.Value.ToFileTimeUtc(), backupPath);
        }

        return result;
    }

    protected string? GetMostRecentProfileBackup(IEnumerable<string> backupPaths, string profileId)
    {
        var profileFilename = $"{profileId}.json";
        var backupPathsWithCreationDateTime = GetBackupPathsWithCreationTimestamp(backupPaths);

        foreach (var (_, backupPath) in backupPathsWithCreationDateTime.Reverse())
        {
            var profileBackups = FileUtil.GetFiles(backupPath);
            var profileBackup = profileBackups.FirstOrDefault(path => path.EndsWith(profileFilename));
            if (profileBackup != null)
            {
                return profileBackup;
            }
        }

        return null;
    }

    /// <summary>
    ///     Retrieves and sorts the backup file paths from the specified directory.
    /// </summary>
    /// <param name="dir"> The directory to search for backup files. </param>
    /// <returns> List of sorted backup file paths. </returns>
    protected List<string> GetBackupPaths(string dir)
    {
        var backups = FileUtil.GetDirectories(dir).ToList();
        backups.Sort(CompareBackupDates);

        return backups;
    }

    /// <summary>
    ///     Compares two backup folder names based on their extracted dates.
    /// </summary>
    /// <param name="a"> The name of the first backup folder. </param>
    /// <param name="b"> The name of the second backup folder. </param>
    /// <returns> The difference in time between the two dates in milliseconds, or `null` if either date is invalid. </returns>
    protected int CompareBackupDates(string a, string b)
    {
        var dateA = ExtractDateFromFolderName(a);
        var dateB = ExtractDateFromFolderName(b);

        if (!dateA.HasValue || !dateB.HasValue)
        {
            return 0; // Skip comparison if either date is invalid.
        }

        return (int)(dateA.Value.ToFileTimeUtc() - dateB.Value.ToFileTimeUtc());
    }

    /// <summary>
    ///     Extracts a date from a folder name string formatted as `YYYY-MM-DD_hh-mm-ss`.
    /// </summary>
    /// <param name="folderPath"> The name of the folder from which to extract the date. </param>
    /// <returns> A DateTime object if the folder name is in the correct format, otherwise null. </returns>
    protected DateTime? ExtractDateFromFolderName(string folderPath)
    {
        var folderName = Path.GetFileName(folderPath);

        const string format = "yyyy-MM-dd_HH-mm-ss";
        if (DateTime.TryParseExact(folderName, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
        {
            return dateTime;
        }

        Logger.Warning($"Invalid backup folder name format: {folderPath}, [{folderName}]");
        return null;
    }

    /// <summary>
    ///     Removes excess backups from the backup directory.
    /// </summary>
    /// <param name="backupFilenames"> List of backup file names to be removed. </param>
    /// <returns> A promise that resolves when all specified backups have been removed. </returns>
    protected void RemoveExcessBackups(IEnumerable<string> backupFilenames)
    {
        var filePathsToDelete = backupFilenames.Select(x => x);
        foreach (var pathToDelete in filePathsToDelete)
        {
            FileUtil.DeleteDirectory(Path.Combine(pathToDelete), true);

            if (Logger.IsLogEnabled(LogLevel.Debug))
            {
                Logger.Debug($"Deleted old backup: {pathToDelete}");
            }
        }
    }

    /// <summary>
    ///     Get a List of active server mod details.
    /// </summary>
    /// <returns> A List of mod names. </returns>
    protected List<string> GetActiveServerMods()
    {
        List<string> result = [];

        foreach (var mod in LoadedMods)
        {
            result.Add($"{mod.ModMetadata.Author} - {mod.ModMetadata.Version}");
        }

        return result;
    }

    /// <summary>
    ///     Restores the most recent profile backup for the given profile Id
    /// </summary>
    /// <param name="profileId">The profile ID of the backup to restore</param>
    /// <returns>True on success. False on failure</returns>
    public bool RestoreProfile(string profileId)
    {
        var backupDir = BackupConfig.Directory;
        var backupPaths = GetBackupPaths(backupDir);
        var mostRecentBackupForProfile = GetMostRecentProfileBackup(backupPaths, profileId);

        // Verify we have a backup for this profile
        if (mostRecentBackupForProfile == null)
        {
            return false;
        }

        // Restore the most recent profile backup
        var profileFileName = FileUtil.GetFileNameAndExtension(mostRecentBackupForProfile);
        var targetProfilePath = Path.Combine(ProfileDir, profileFileName);

        File.Copy(mostRecentBackupForProfile, targetProfilePath, true);
        return true;
    }
}
