using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Context;
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
    protected ApplicationContext _applicationContext;
    protected BackupConfig _backupConfig;

    // Runs Init() every x minutes
    protected Timer _backupIntervalTimer;
    protected FileUtil _fileUtil;
    protected JsonUtil _jsonUtil;
    protected ISptLogger<BackupService> _logger;
    protected TimeUtil _timeUtil;

    public BackupService(
        ISptLogger<BackupService> logger,
        JsonUtil jsonUtil,
        TimeUtil timeUtil,
        ConfigServer configServer,
        FileUtil fileUtil,
        ApplicationContext applicationContext
    )
    {
        _logger = logger;
        _jsonUtil = jsonUtil;
        _timeUtil = timeUtil;
        _fileUtil = fileUtil;
        _applicationContext = applicationContext;

        _activeServerMods = GetActiveServerMods();
        _backupConfig = configServer.GetConfig<BackupConfig>();
    }

    /// <summary>
    ///     Start the backup interval if enabled in config.
    /// </summary>
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

    /// <summary>
    ///     Initializes the backup process. <br />
    ///     This method orchestrates the profile backup service. Handles copying profiles to a backup directory and cleaning
    ///     up old backups if the number exceeds the configured maximum.
    /// </summary>
    public void Init()
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
            _fileUtil.WriteFile(Path.Combine(targetDir, "activeMods.json"), _jsonUtil.Serialize(_activeServerMods));

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
        var date = _timeUtil.GetDateTimeNow();

        return $"{date.Year}-{date.Month}-{date.Day}_{date.Hour}-{date.Minute}-{date.Second}";
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

    private SortedDictionary<long, string> GetBackupPathsWithCreationTimestamp(List<string> backupPaths)
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
    private List<string> GetBackupPaths(string dir)
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
    private int CompareBackupDates(string a, string b)
    {
        var dateA = ExtractDateFromFolderName(a);
        var dateB = ExtractDateFromFolderName(b);

        if (!dateA.HasValue || !dateB.HasValue)
        {
            return 0; // Skip comparison if either date is invalid.
        }

        return (int) (dateA.Value.ToFileTimeUtc() - dateB.Value.ToFileTimeUtc());
    }

    /// <summary>
    ///     Extracts a date from a folder name string formatted as `YYYY-MM-DD_hh-mm-ss`.
    /// </summary>
    /// <param name="folderName"> The name of the folder from which to extract the date. </param>
    /// <returns> A DateTime object if the folder name is in the correct format, otherwise null. </returns>
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

    /// <summary>
    ///     Removes excess backups from the backup directory.
    /// </summary>
    /// <param name="backupFilenames"> List of backup file names to be removed. </param>
    /// <returns> A promise that resolves when all specified backups have been removed. </returns>
    private void RemoveExcessBackups(List<string> backupFilenames)
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
        var mods = _applicationContext?.GetLatestValue(ContextVariableType.LOADED_MOD_ASSEMBLIES)?.GetValue<List<SptMod>>();
        if (mods == null)
        {
            return [];
        }

        List<string> result = [];

        foreach (var mod in mods)
        {
            result.Add($"{mod.PackageJson.Author} - {mod.PackageJson.Version ?? ""}");
        }

        return result;
    }
}
