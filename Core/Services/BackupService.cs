using Core.Annotations;

namespace Core.Services;

[Injectable]
public class BackupService
{
    /**
 * Initializes the backup process.
 *
 * This method orchestrates the profile backup service. Handles copying profiles to a backup directory and cleaning
 * up old backups if the number exceeds the configured maximum.
 *
 * @returns A promise that resolves when the backup process is complete.
 */
    public async Task InitAsync()
    {
        // TODO implement
    }

    /**
     * Fetches the names of all JSON files in the profile directory.
     *
     * This method normalizes the profile directory path and reads all files within it. It then filters the files to
     * include only those with a `.json` extension and returns their names.
     *
     * @returns A promise that resolves to a List of JSON file names.
     */
    protected async Task<List<string>> FetchProfileFilesAsync()
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
        throw new NotImplementedException();
    }

    /**
     * Generates the target directory path for the backup. The directory path is constructed using the `directory` from
     * the configuration and the current backup date.
     *
     * @returns The target directory path for the backup.
     */
    protected string GenerateBackupTargetDir()
    {
        throw new NotImplementedException();
    }

    /**
     * Generates a formatted backup date string in the format `YYYY-MM-DD_hh-mm-ss`.
     *
     * @returns The formatted backup date string.
     */
    protected string GenerateBackupDate()
    {
        throw new NotImplementedException();
    }

    /**
     * Cleans up old backups in the backup directory.
     *
     * This method reads the backup directory, and sorts backups by modification time. If the number of backups exceeds
     * the configured maximum, it deletes the oldest backups.
     *
     * @returns A promise that resolves when the cleanup is complete.
     */
    protected async Task CleanBackupsAsync()
    {
        throw new NotImplementedException();
    }

    /**
     * Retrieves and sorts the backup file paths from the specified directory.
     *
     * @param dir - The directory to search for backup files.
     * @returns A promise that resolves to a List of sorted backup file paths.
     */
    private async Task<List<string>> GetBackupPathsAsync(string dir)
    {
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
        throw new NotImplementedException();
    }

    /**
     * Extracts a date from a folder name string formatted as `YYYY-MM-DD_hh-mm-ss`.
     *
     * @param folderName - The name of the folder from which to extract the date.
     * @returns A DateTime object if the folder name is in the correct format, otherwise null.
     */
    private DateTime? ExtractDateFromFolderName(string folderName)
    {
        throw new NotImplementedException();
    }

    /**
     * Removes excess backups from the backup directory.
     *
     * @param backups - A List of backup file names to be removed.
     * @returns A promise that resolves when all specified backups have been removed.
     */
    private async Task RemoveExcessBackupsAsync(List<string> backups)
    {
        throw new NotImplementedException();
    }

    /**
     * Start the backup interval if enabled in the configuration.
     */
    protected void StartBackupInterval()
    {
        throw new NotImplementedException();
    }

    /**
     * Get a List of active server mod details.
     *
     * @returns A List of mod names.
     */
    protected List<string> GetActiveServerMods()
    {
        throw new NotImplementedException();
    }
}
