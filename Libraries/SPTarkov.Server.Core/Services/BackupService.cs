using System.Globalization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class BackupService
{
    protected const string _profileDir = "./user/profiles";

    protected readonly List<string> _activeServerMods;
    protected readonly BackupConfig _backupConfig;

    // Runs Init() every x minutes
    protected Timer _backupIntervalTimer;
    protected readonly FileUtil _fileUtil;
    protected readonly JsonUtil _jsonUtil;
    protected readonly ISptLogger<BackupService> _logger;
    protected readonly TimeUtil _timeUtil;
    protected readonly IReadOnlyList<SptMod> _loadedMods;

    public BackupService(
        ISptLogger<BackupService> logger,
        IReadOnlyList<SptMod> loadedMods,
        JsonUtil jsonUtil,
        TimeUtil timeUtil,
        ConfigServer configServer,
        FileUtil fileUtil
    )
    {
        _logger = logger;
        _jsonUtil = jsonUtil;
        _timeUtil = timeUtil;
        _fileUtil = fileUtil;
        _loadedMods = loadedMods;

        _activeServerMods = GetActiveServerMods();
        _backupConfig = configServer.GetConfig<BackupConfig>();
    }

    /// <summary>
    ///     Start the backup interval if enabled in config.
    /// </summary>
    public async Task StartBackupSystem()
    {
        if (!_backupConfig.BackupInterval.Enabled)
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
                    _logger.Error($"Profile backup failed: {ex.Message}, {ex.StackTrace}");
                }
            },
            null,
            TimeSpan.Zero,
            TimeSpan.FromMinutes(_backupConfig.BackupInterval.IntervalMinutes)
        );
    }

    /// <summary>
    ///     Initializes the backup process. <br />
    ///     This method orchestrates the profile backup service. Handles copying profiles to a backup directory and cleaning
    ///     up old backups if the number exceeds the configured maximum.
    /// </summary>
    public async Task Init()
    {
        if (!IsEnabled())
        {
            return;
        }

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
            if (_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug("No profiles to backup");
            }

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
                if (!_fileUtil.CopyFile(relativeSourceFilePath, absoluteDestinationFilePath))
                {
                    _logger.Error($"Source file not found: {relativeSourceFilePath}. Cannot copy to: {absoluteDestinationFilePath}");
                }
            }

            // Write a copy of active mods.
            await _fileUtil.WriteFileAsync(Path.Combine(targetDir, "activeMods.json"), _jsonUtil.Serialize(_activeServerMods));

            if (_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug($"Profile backup created in: {targetDir}");
            }
        }
        catch (Exception ex)
        {
            _logger.Error($"Unable to write to backup profile directory: {ex.Message}");
            return;
        }

        CleanBackups();
    }

    /// <summary>
    ///     Check to see if the backup service is enabled via the config.
    /// </summary>
    /// <returns> True if enabled, false otherwise. </returns>
    protected bool IsEnabled()
    {
        if (_backupConfig.Enabled)
        {
            return true;
        }

        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug("Profile backups disabled");
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
        return Path.GetFullPath($"{_backupConfig.Directory}/{backupDate}");
    }

    /// <summary>
    ///     Generates a formatted backup date string in the format `YYYY-MM-DD_hh-mm-ss`.
    /// </summary>
    /// <returns> The formatted backup date string. </returns>
    protected string GenerateBackupDate()
    {
        return _timeUtil.GetDateTimeNow().ToString("yyyy-MM-dd_HH-mm-ss");
    }

    /// <summary>
    ///     Cleans up old backups in the backup directory. <br />
    ///     This method reads the backup directory, and sorts backups by modification time. If the number of backups exceeds
    ///     the configured maximum, it deletes the oldest backups.
    /// </summary>
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

    /// <summary>
    ///     Retrieves and sorts the backup file paths from the specified directory.
    /// </summary>
    /// <param name="dir"> The directory to search for backup files. </param>
    /// <returns> List of sorted backup file paths. </returns>
    protected List<string> GetBackupPaths(string dir)
    {
        var backups = _fileUtil.GetDirectories(dir).ToList();
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

        _logger.Warning($"Invalid backup folder name format: {folderPath}, [{folderName}]");
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
            _fileUtil.DeleteDirectory(Path.Combine(pathToDelete), true);

            if (_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug($"Deleted old backup: {pathToDelete}");
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

        foreach (var mod in _loadedMods)
        {
            result.Add($"{mod.ModMetadata.Author} - {mod.ModMetadata.Version}");
        }

        return result;
    }
}
