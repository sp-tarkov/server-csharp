using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Server;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Core.Utils;

[Injectable(InjectionType.Singleton, TypePriority = OnLoadOrder.Database)]
public class DatabaseImporter(
    ISptLogger<DatabaseImporter> logger,
    FileUtil fileUtil,
    ServerLocalisationService serverLocalisationService,
    DatabaseServer databaseServer,
    ImageRouter imageRouter,
    ImporterUtil importerUtil,
    JsonUtil jsonUtil
) : IOnLoad
{
    private const string SptDataPath = "./SPT_Data/";
    protected readonly Dictionary<string, string> DatabaseHashes = [];

    public async Task OnLoad(CancellationToken cancellationToken)
    {
        var shouldVerify = !ProgramStatics.DEBUG();

        if (shouldVerify)
        {
            await LoadHashesAsync(cancellationToken);
        }

        await HydrateDatabaseAsync(SptDataPath, shouldVerify, cancellationToken);

        var imageFilePath = $"{SptDataPath}images/";
        CreateRouteMapping(imageFilePath, "files");
    }

    private void CreateRouteMapping(string directory, string newBasePath)
    {
        var directoryContent = Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories);

        foreach (var fileNameWithPath in directoryContent)
        {
            var fileNameWithNoSPTPath = Path.GetRelativePath(directory, fileNameWithPath);
            var filePathNoExtension = fileUtil.StripExtension(fileNameWithNoSPTPath, true);
            if (filePathNoExtension.StartsWith("/") || fileNameWithPath.StartsWith("\\"))
            {
                filePathNoExtension = $"{filePathNoExtension.Substring(1)}";
            }

            var bsgPath = $"/{newBasePath}/{filePathNoExtension}".Replace("\\", "/");
            imageRouter.AddRoute(bsgPath, fileNameWithPath);

            if (fileNameWithNoSPTPath.Contains("icon.ico"))
            {
                imageRouter.AddRoute("/favicon", fileNameWithPath);
            }
        }
    }

    protected async Task LoadHashesAsync(CancellationToken cancellationToken = default)
    {
        var checksFilePath = Path.Combine(SptDataPath, "checks.dat");

        try
        {
            if (File.Exists(checksFilePath))
            {
                await using var fs = File.OpenRead(checksFilePath);

                using var reader = new StreamReader(fs, Encoding.ASCII);
                var base64Content = await reader.ReadToEndAsync(cancellationToken);

                var jsonBytes = Convert.FromBase64String(base64Content);

                await using var ms = new MemoryStream(jsonBytes);

                var FileHashes = await jsonUtil.DeserializeFromMemoryStreamAsync<List<FileHash>>(ms, cancellationToken) ?? [];

                foreach (var hash in FileHashes)
                {
                    DatabaseHashes.Add(hash.Path, hash.Hash);
                }
            }
            else
            {
                logger.Error(serverLocalisationService.GetText("validation_error_exception", checksFilePath));
            }
        }
        catch (Exception)
        {
            logger.Error(serverLocalisationService.GetText("validation_error_exception", checksFilePath));
        }
    }

    /// <summary>
    /// Read all json files in database folder and map into a json object
    /// </summary>
    /// <param name="filePath">path to database folder</param>
    /// <param name="shouldVerifyDatabase">if the database should be verified after deserialization</param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that can be used to cancel the database hydration operation.
    /// </param>
    /// <returns></returns>
    protected async Task HydrateDatabaseAsync(string filePath, bool shouldVerifyDatabase, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.Info(serverLocalisationService.GetText("importing_database"));
            Stopwatch timer = new();
            timer.Start();

            var dataToImport = await importerUtil.LoadRecursiveAsync<DatabaseTables>(
                $"{filePath}database/",
                shouldVerifyDatabase ? VerifyDatabaseAsync : null,
                cancellationToken: cancellationToken
            );

            timer.Stop();

            logger.Info(serverLocalisationService.GetText("importing_database_finish"));
            logger.Debug($"Database import took {timer.ElapsedMilliseconds}ms");
            databaseServer.SetTables(dataToImport);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.Warning("Database import was cancelled.");

            throw;
        }
    }

    protected async Task VerifyDatabaseAsync(string fileName, CancellationToken cancellationToken)
    {
        var relativePath = fileName.StartsWith(SptDataPath, StringComparison.OrdinalIgnoreCase)
            ? fileName.Substring(SptDataPath.Length)
            : fileName;

        using (var md5 = MD5.Create())
        {
            await using (var stream = File.OpenRead(fileName))
            {
                var hashBytes = await md5.ComputeHashAsync(stream, cancellationToken);
                var hashString = Convert.ToHexString(hashBytes);

                if (DatabaseHashes.TryGetValue(relativePath, out var expectedHash))
                {
                    if (expectedHash != hashString)
                    {
                        logger.Warning(serverLocalisationService.GetText("validation_error_file", fileName));
                    }
                }
                else
                {
                    logger.Warning(serverLocalisationService.GetText("validation_error_file", fileName));
                }
            }
        }
    }
}

public class FileHash
{
    public string Path { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
}
