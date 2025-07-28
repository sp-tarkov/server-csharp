using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Server;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Core.Utils;

[Injectable(InjectionType.Singleton, TypePriority = OnLoadOrder.Database)]
public class DatabaseImporter(
    ISptLogger<DatabaseImporter> logger,
    FileUtil _fileUtil,
    ServerLocalisationService _serverLocalisationService,
    DatabaseServer _databaseServer,
    ImageRouter _imageRouter,
    ImporterUtil _importerUtil,
    JsonUtil _jsonUtil
) : IOnLoad
{
    private const string _sptDataPath = "./SPT_Data/";
    protected readonly Dictionary<string, string> databaseHashes = [];

    public async Task OnLoad()
    {
        await LoadHashes();
        await HydrateDatabase(_sptDataPath);

        var imageFilePath = $"{_sptDataPath}images/";
        CreateRouteMapping(imageFilePath, "files");
    }

    private void CreateRouteMapping(string directory, string newBasePath)
    {
        var directoryContent = GetAllFilesInDirectory(directory);

        foreach (var fileNameWithPath in directoryContent)
        {
            var fileNameWithNoSPTPath = fileNameWithPath.Replace(directory, "");
            var filePathNoExtension = _fileUtil.StripExtension(fileNameWithNoSPTPath, true);
            if (filePathNoExtension.StartsWith("/") || fileNameWithPath.StartsWith("\\"))
            {
                filePathNoExtension = $"{filePathNoExtension.Substring(1)}";
            }

            var bsgPath = $"/{newBasePath}/{filePathNoExtension}".Replace("\\", "/");
            _imageRouter.AddRoute(bsgPath, fileNameWithPath);
        }
    }

    private List<string> GetAllFilesInDirectory(string directoryPath)
    {
        List<string> result = [];
        result.AddRange(Directory.GetFiles(directoryPath));

        foreach (var subdirectory in Directory.GetDirectories(directoryPath))
        {
            result.AddRange(GetAllFilesInDirectory(subdirectory));
        }

        return result;
    }

    protected async Task LoadHashes()
    {
        // The checks hash file is only made in Release mode
        if (ProgramStatics.DEBUG())
        {
            return;
        }

        var checksFilePath = System.IO.Path.Combine(_sptDataPath, "checks.dat");

        try
        {
            if (File.Exists(checksFilePath))
            {
                await using FileStream fs = File.OpenRead(checksFilePath);

                using var reader = new StreamReader(fs, Encoding.ASCII);
                string base64Content = await reader.ReadToEndAsync();

                byte[] jsonBytes = Convert.FromBase64String(base64Content);

                await using var ms = new MemoryStream(jsonBytes);

                var FileHashes = await _jsonUtil.DeserializeFromMemoryStreamAsync<List<FileHash>>(ms) ?? [];

                foreach (var hash in FileHashes)
                {
                    databaseHashes.Add(hash.Path, hash.Hash);
                }
            }
            else
            {
                logger.Error(_serverLocalisationService.GetText("validation_error_exception", checksFilePath));
            }
        }
        catch (Exception)
        {
            logger.Error(_serverLocalisationService.GetText("validation_error_exception", checksFilePath));
        }
    }

    /// <summary>
    /// Read all json files in database folder and map into a json object
    /// </summary>
    /// <param name="filePath">path to database folder</param>
    /// <returns></returns>
    protected async Task HydrateDatabase(string filePath)
    {
        logger.Info(_serverLocalisationService.GetText("importing_database"));
        Stopwatch timer = new();
        timer.Start();

        var dataToImport = await _importerUtil.LoadRecursiveAsync<DatabaseTables>($"{filePath}database/", VerifyDatabase);

        timer.Stop();

        logger.Info(_serverLocalisationService.GetText("importing_database_finish"));
        logger.Debug($"Database import took {timer.ElapsedMilliseconds}ms");
        _databaseServer.SetTables(dataToImport);
    }

    protected async Task VerifyDatabase(string fileName)
    {
        // The checks hash file is only made in Release mode
        if (ProgramStatics.DEBUG())
        {
            return;
        }

        var relativePath = fileName.StartsWith(_sptDataPath, StringComparison.OrdinalIgnoreCase)
            ? fileName.Substring(_sptDataPath.Length)
            : fileName;

        using (var md5 = MD5.Create())
        {
            await using (var stream = File.OpenRead(fileName))
            {
                var hashBytes = await md5.ComputeHashAsync(stream);
                var hashString = Convert.ToHexString(hashBytes);

                bool hashKeyExists = databaseHashes.ContainsKey(relativePath);

                if (hashKeyExists)
                {
                    if (databaseHashes[relativePath] != hashString)
                    {
                        logger.Warning(_serverLocalisationService.GetText("validation_error_file", fileName));
                    }
                }
                else
                {
                    logger.Warning(_serverLocalisationService.GetText("validation_error_file", fileName));
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
