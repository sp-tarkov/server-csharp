using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Utils;
using SptCommon.Annotations;
using LogLevel = Core.Models.Spt.Logging.LogLevel;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class BackupService
{
    protected ISptLogger<BackupService> _logger;
    protected JsonUtil _jsonUtil;
    protected TimeUtil _timeUtil;
    protected FileUtil _fileUtil;
    protected BackupConfig _backupConfig;

    protected readonly List<string> _activeServerMods;
    protected const string _profileDir = "./user/profiles";

    // Runs Init() every x minutes
    protected Timer _backupIntervalTimer;

    public BackupService(
        ISptLogger<BackupService> logger,
        JsonUtil jsonUtil,
        TimeUtil timeUtil,
        ConfigServer configServer,
        FileUtil fileUtil)
    {
        _logger = logger;
        _jsonUtil = jsonUtil;
        _timeUtil = timeUtil;
        _fileUtil = fileUtil;

        _activeServerMods = GetActiveServerMods();
        _backupConfig = configServer.GetConfig<BackupConfig>();
    }

    /**
     * Start the backup interval if enabled in config.
     */
    public void StartBackupSystem()
    {
        if (!_backupConfig.BackupInterval.Enabled)
        {
            // Not backing up at regular intervals, run once and exit
            Init();

            return;
        }

        _backupIntervalTimer = new Timer(
            _ =>
            {
                try
                {
                    Init();
                }
                catch (Exception ex)
                {
                    _logger.Error($"Profile backup failed: {ex.Message}, {ex.StackTrace}");
                }
            },
            null,
            TimeSpan.Zero,
            TimeSpan.FromMinutes(_backupConfig.BackupInterval.IntervalMinutes)
        );
    }

    /**
     * Initializes the backup process.
     *
     * This method orchestrates the profile backup service. Handles copying profiles to a backup directory and cleaning
     * up old backups if the number exceeds the configured maximum.
     *
     * @returns A promise that resolves when the backup process is complete.
     */
    public void Init()
    {
        if (!IsEnabled()) return;

        var targetDir = GenerateBackupTargetDir();

        // Fetch all profiles in the profile directory.
        List<string> currentProfilePaths;
        try
        {
            currentProfilePaths = _fileUtil.GetFiles(_profileDir);
        }
        catch (Exception ex)
        {
            _logger.Debug($"Skipping profile backup: Unable to read profiles directory, {ex.Message}");
            return;
        }

        if (currentProfilePaths.Count == 0)
        {
            if (_logger.IsLogEnabled(LogLevel.Debug)) _logger.Debug("No profiles to backup");

            return;
        }

        try
        {
            _fileUtil.CreateDirectory(targetDir);

            foreach (var profilePath in currentProfilePaths)
            {
                // Get filename + extension, removing the path
                var profileFileName = _fileUtil.GetFileNameAndExtension(profilePath);

                // Create absolute path to file
                var relativeSourceFilePath = Path.Combine(_profileDir, profileFileName);
                var absoluteDestinationFilePath = Path.Combine(targetDir, profileFileName);
                _fileUtil.CopyFile(relativeSourceFilePath, absoluteDestinationFilePath);
            }

            // Write a copy of active mods.
            _fileUtil.WriteFile(Path.Combine(targetDir, "activeMods.json"), _jsonUtil.Serialize(_activeServerMods));

            if (_logger.IsLogEnabled(LogLevel.Debug)) _logger.Debug($"Profile backup created in: {targetDir}");
        }
        catch (Exception ex)
        {
            _logger.Error($"Unable to write to backup profile directory: {ex.Message}");
            return;
        }

        CleanBackups();
    }

    /**
     * Check to see if the backup service is enabled via the config.
     *
     * @returns True if enabled, false otherwise.
     */
    protected bool IsEnabled()
    {
        if (_backupConfig.Enabled) return true;

        if (_logger.IsLogEnabled(LogLevel.Debug)) _logger.Debug("Profile backups disabled");

        return false;
    }

    /**
     * Generates the target directory path for the backup. The directory path is constructed using the `directory` from
     * the configuration and the current backup date.
     *
     * @returns The target directory path for the backup.
     */
    protected string GenerateBackupTargetDir()
    {
        var backupDate = GenerateBackupDate();
        return Path.GetFullPath($"{_backupConfig.Directory}/{backupDate}");
    }

    /**
     * Generates a formatted backup date string in the format `YYYY-MM-DD_hh-mm-ss`.
     *
     * @returns The formatted backup date string.
     */
    protected string GenerateBackupDate()
    {
        var date = _timeUtil.GetDateTimeNow();

        return $"{date.Year}-{date.Month}-{date.Day}_{date.Hour}-{date.Minute}-{date.Second}";
    }

    /**
     * Cleans up old backups in the backup directory.
     *
     * This method reads the backup directory, and sorts backups by modification time. If the number of backups exceeds
     * the configured maximum, it deletes the oldest backups.
     *
     * @returns A promise that resolves when the cleanup is complete.
     */
    protected void CleanBackups()
    {
        var backupDir = _backupConfig.Directory;
        var backupPaths = GetBackupPaths(backupDir);

        // Filter out invalid backup paths by ensuring they contain a valid date.
        var backupPathsWithCreationDateTime = GetBackupPathsWithCreationTimestamp(backupPaths);
        var excessCount = backupPathsWithCreationDateTime.Count - _backupConfig.MaxBackups;
        if (excessCount > 0)
        {
            var excessBackupPaths = backupPaths.GetRange(0, excessCount);
            RemoveExcessBackups(excessBackupPaths);
        }
    }

    private SortedDictionary<long, string> GetBackupPathsWithCreationTimestamp(List<string> backupPaths)
    {
        var result = new SortedDictionary<long, string>();
        foreach (var backupPath in backupPaths)
        {
            var date = ExtractDateFromFolderName(backupPath);
            if (!date.HasValue) continue;

            result.Add(date.Value.ToFileTimeUtc(), backupPath);
        }

        return result;
    }

    /**
     * Retrieves and sorts the backup file paths from the specified directory.
     *
     * @param dir - The directory to search for backup files.
     * @returns A promise that resolves to a List of sorted backup file paths.
     */
    private List<string> GetBackupPaths(string dir)
    {
        var backups = _fileUtil.GetDirectories(dir).ToList();
        backups.Sort(CompareBackupDates);

        return backups;
    }

    /**
     * Compares two backup folder names based on their extracted dates.
     *
     * @param a - The name of the first backup folder.
     * @param b - The name of the second backup folder.
     * @returns The difference in time between the two dates in milliseconds, or `null` if either date is invalid.
     */
    private int CompareBackupDates(string a, string b)
    {
        var dateA = ExtractDateFromFolderName(a);
        var dateB = ExtractDateFromFolderName(b);

        if (!dateA.HasValue || !dateB.HasValue) return 0; // Skip comparison if either date is invalid.

        return (int)(dateA.Value.ToFileTimeUtc() - dateB.Value.ToFileTimeUtc());
    }

    /**
     * Extracts a date from a folder name string formatted as `YYYY-MM-DD_hh-mm-ss`.
     *
     * @param folderName - The name of the folder from which to extract the date.
     * @returns A DateTime object if the folder name is in the correct format, otherwise null.
     */
    private DateTime? ExtractDateFromFolderName(string folderName)
    {
        // backup
        var parts = folderName.Split('\\', '-', '_');
        if (parts.Length != 7)
        {
            _logger.Warning($"Invalid backup folder name format: {folderName}");
            return null;
        }

        var year = int.Parse(parts[1]);
        var month = int.Parse(parts[2]);
        var day = int.Parse(parts[3]);
        var hour = int.Parse(parts[4]);
        var minute = int.Parse(parts[5]);
        var second = int.Parse(parts[6]);

        return new DateTime(year, month, day, hour, minute, second);
    }

    /**
     * Removes excess backups from the backup directory.
     *
     * @param backups - A List of backup file names to be removed.
     * @returns A promise that resolves when all specified backups have been removed.
     */
    private void RemoveExcessBackups(List<string> backupFilenames)
    {
        var filePathsToDelete = backupFilenames.Select(x => x);
        foreach (var pathToDelete in filePathsToDelete)
        {
            _fileUtil.DeleteDirectory(Path.Combine(pathToDelete), true);

            if (_logger.IsLogEnabled(LogLevel.Debug)) _logger.Debug($"Deleted old backup: {pathToDelete}");
        }
    }

    /**
     * Get a List of active server mod details.
     *
     * @returns A List of mod names.
     */
    protected List<string> GetActiveServerMods()
    {
        _logger.Error($"NOT IMPLEMENTED - GetActiveServerMods");
        List<string> result = [];

        return result;

        //var activeMods = _preSptModLoader.getImportedModDetails();
        //foreach (var activeModKey in activeMods) {
        //    result.Add($"{ activeModKey} -{ activeMods[activeModKey].author ?? "unknown"} -{ activeMods[activeModKey].version ?? ""}");
        //}
        //return result;
    }
}
