using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Services;

[Injectable]
public class BackupService(
    ISptLogger<BackupService> _logger,
    JsonUtil _jsonUtil,
    TimeUtil _timeUtil,
    ConfigServer _configServer,
    FileUtil _fileUtil)
{
    protected BackupConfig _backupConfig = _configServer.GetConfig<BackupConfig>();

    protected readonly List<string> _activeServerMods = [];
    protected readonly string _profileDir = "./user/profiles";

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
        if (!IsEnabled())
        {
            return;
        }

        var targetDir = GenerateBackupTargetDir();

        // Fetch all profiles in the profile directory.
        List<string> currentProfilePaths = [];
        try
        {
            currentProfilePaths = _fileUtil.GetFiles(_profileDir);
        }
        catch (Exception ex)
        {
            _logger.Debug("Skipping profile backup: Unable to read profiles directory");
            return;
        }

        if (currentProfilePaths.Count == 0)
        {
            _logger.Debug("No profiles to backup");
            return;
        }

        try
        {
            _fileUtil.CreateDirectory(targetDir);

            // Track write promises.
            var profileIds = currentProfilePaths.Select(x => x.Last());
            foreach (var profileId in profileIds)
            {
                var fullPath = $"{_profileDir}";
                var destinationPath = "";
                _fileUtil.CopyFile(fullPath, destinationPath);

            }

            // Write a copy of active mods.
            _fileUtil.WriteFile($"{targetDir}/activeMods.json", _jsonUtil.Serialize(_activeServerMods));

            _logger.Debug($"Profile backup created in: {targetDir}");
        }
        catch (Exception ex)
        {
            _logger.Error($"Unable to write to backup profile directory: { ex.Message}");
            return;
        }

        CleanBackups();
    }

    /**
     * Fetches the names of all JSON files in the profile directory.
     *
     * This method normalizes the profile directory path and reads all files within it. It then filters the files to
     * include only those with a `.json` extension and returns their names.
     *
     * @returns A promise that resolves to a List of JSON file names.
     */
    protected List<string> FetchProfileFiles()
    {
        throw new NotImplementedException();
    }

    /**
     * Check to see if the backup service is enabled via the config.
     *
     * @returns True if enabled, false otherwise.
     */
    protected bool IsEnabled()
    {
        if (_backupConfig.Enabled)
        {
            return true;
        }

        _logger.Debug("Profile backups disabled");

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
        return Path.GetFullPath($"{ _backupConfig.Directory}/${backupDate}");
    }

    /**
     * Generates a formatted backup date string in the format `YYYY-MM-DD_hh-mm-ss`.
     *
     * @returns The formatted backup date string.
     */
    protected string GenerateBackupDate()
    {
        var date = _timeUtil.GetDateTimeNow();

        return $"{date.Year}-{date.Month}-{date.Day}_{date.Hour}-{date.Minute}-{date.Second}"; }

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
        var validBackupPaths = backupPaths.Where((path) => ExtractDateFromFolderName(path) != null);

        var excessCount = validBackupPaths.Count() - _backupConfig.MaxBackups;
        if (excessCount > 0)
        {
            var excessBackups = backupPaths.Slice(0, excessCount);
            RemoveExcessBackups(excessBackups);
        }
    }

    /**
     * Retrieves and sorts the backup file paths from the specified directory.
     *
     * @param dir - The directory to search for backup files.
     * @returns A promise that resolves to a List of sorted backup file paths.
     */
    private List<string> GetBackupPaths(string dir)
    {
        // TODO: Fully implement
        var backups = _fileUtil.GetFiles(dir).Where(x => x.EndsWith(".json")).ToList();
        //return backups.Sort(CompareBackupDates.Bind(this));
        throw new NotImplementedException();
    }

    /**
     * Compares two backup folder names based on their extracted dates.
     *
     * @param a - The name of the first backup folder.
     * @param b - The name of the second backup folder.
     * @returns The difference in time between the two dates in milliseconds, or `null` if either date is invalid.
     */
    private long? CompareBackupDates(string a, string b)
    {
        var dateA = ExtractDateFromFolderName(a);
        var dateB = ExtractDateFromFolderName(b);

        if (!dateA.HasValue || !dateB.HasValue)
        {
            return null; // Skip comparison if either date is invalid.
        }

        return dateA.Value.ToFileTimeUtc() - dateB.Value.ToFileTimeUtc();
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
        var parts = folderName.Split('-', '_');
        if (parts.Length != 6)
        {
            _logger.Warning($"Invalid backup folder name format: {folderName}");
            return null;
        }

        var year = int.Parse(parts[0]);
        var month = int.Parse(parts[1]);
        var day = int.Parse(parts[2]);
        var hour = int.Parse(parts[3]);
        var minute = int.Parse(parts[4]);
        var second = int.Parse(parts[5]);

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
            _fileUtil.DeleteFile($"{_backupConfig.Directory}/{pathToDelete}");
        }
    }

    /**
     * Start the backup interval if enabled in the configuration.
     */
    protected void StartBackupInterval()
    {
        if (!_backupConfig.BackupInterval.Enabled)
        {
            return;
        }

        var minutes = _backupConfig.BackupInterval.IntervalMinutes * 60 * 1000; // Minutes to milliseconds
        //SetInterval(() => {
        //    Init().catch((error) => this.logger.error(`Profile backup failed: ${ error.message}`));
        //}, minutes);

        throw new NotImplementedException();
    }

    /**
     * Get a List of active server mod details.
     *
     * @returns A List of mod names.
     */
    protected List<string> GetActiveServerMods()
    {
        List<string> result = [];

        //var activeMods = _preSptModLoader.getImportedModDetails();
        //foreach (var activeModKey in activeMods) {
        //    result.Add($"{ activeModKey} -{ activeMods[activeModKey].author ?? "unknown"} -{ activeMods[activeModKey].version ?? ""}");
        //}
        //return result;

        throw new NotImplementedException();
    }
}
