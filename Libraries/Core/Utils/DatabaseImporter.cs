using System.Diagnostics;
using Core.DI;
using Core.Models.Eft.Common.Tables;
using Core.Models.Spt.Config;
using Core.Models.Spt.Server;
using Core.Models.Utils;
using Core.Routers;
using Core.Servers;
using Core.Services;
using SptCommon.Annotations;
using LogLevel = Core.Models.Spt.Logging.LogLevel;

namespace Core.Utils;

[Injectable(InjectionType.Singleton, InjectableTypeOverride = typeof(IOnLoad), TypePriority = OnLoadOrder.Database)]
public class DatabaseImporter : IOnLoad
{
    private const string _sptDataPath = "./Assets/";
    private readonly HttpConfig httpConfig;
    private readonly ValidationResult valid = ValidationResult.UNDEFINED;
    protected ConfigServer _configServer;

    protected DatabaseServer _databaseServer;
    protected EncodingUtil _encodingUtil;
    protected FileUtil _fileUtil;
    protected HashUtil _hashUtil;

    protected ImageRouter _imageRouter;
    protected ImporterUtil _importerUtil;
    protected LocalisationService _localisationService;

    protected ISptLogger<DatabaseImporter> _logger;
    private string filepath;
    private object hashedFile;

    public DatabaseImporter(
        ISptLogger<DatabaseImporter> logger,
        // TODO: are we gonna use this? @inject("JsonUtil") protected jsonUtil: JsonUtil,
        FileUtil fileUtil,
        LocalisationService localisationService,
        DatabaseServer databaseServer,
        ImageRouter imageRouter,
        EncodingUtil encodingUtil,
        HashUtil hashUtil,
        ImporterUtil importerUtil,
        ConfigServer configServer
    )
    {
        _logger = logger;
        _localisationService = localisationService;
        _databaseServer = databaseServer;
        _encodingUtil = encodingUtil;
        _hashUtil = hashUtil;
        _importerUtil = importerUtil;
        _configServer = configServer;
        _fileUtil = fileUtil;
        _imageRouter = imageRouter;
        httpConfig = _configServer.GetConfig<HttpConfig>();
    }

    public async Task OnLoad()
    {
        filepath = GetSptDataPath();

        /*
        if (ProgramStatics.COMPILED) {
            try {
                // Reading the dynamic SHA1 file
                const file = "checks.dat";
                const fileWithPath = `${this.filepath}${file}`;
                if (this.vfs.exists(fileWithPath)) {
                    this.hashedFile = this.jsonUtil.deserialize(
                        this.encodingUtil.fromBase64(this.vfs.readFile(fileWithPath)),
                        file,
                    );
                } else {
                    this.valid = ValidationResult.NOT_FOUND;
                    this.logger.debug(this.localisationService.getText("validation_not_found"));
                }
            } catch (e) {
                this.valid = ValidationResult.FAILED;
                this.logger.warning(this.localisationService.getText("validation_error_decode"));
            }
        }
        */

        await HydrateDatabase(filepath);

        var imageFilePath = $"{filepath}images/";
        CreateRouteMapping(imageFilePath, "files");
    }

    public string GetRoute()
    {
        return "spt-database";
    }

    /**
     * Get path to spt data
     * @returns path to data
     */
    public string GetSptDataPath()
    {
        return _sptDataPath;
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

    /**
     * Read all json files in database folder and map into a json object
     * @param filepath path to database folder
     */
    protected async Task HydrateDatabase(string filePath)
    {
        _logger.Info(_localisationService.GetText("importing_database"));
        Stopwatch timer = new();
        timer.Start();

        var dataToImport = await _importerUtil.LoadRecursiveAsync<DatabaseTables>(
            $"{filePath}database/",
            OnReadValidate
        );

        // TODO: Fix loading of traders, so their full path is not included as the key

        var tempTraders = new Dictionary<string, Trader>();

        // temp fix for trader keys
        foreach (var trader in dataToImport.Traders)
        {
            // fix string for key
            var tempKey = trader.Key.Split("/").Last();
            tempTraders.Add(tempKey, trader.Value);
        }

        timer.Stop();

        dataToImport.Traders = tempTraders;

        var validation = valid == ValidationResult.FAILED || valid == ValidationResult.NOT_FOUND ? "." : "";
        _logger.Info($"{_localisationService.GetText("importing_database_finish")}{validation}");
        this._logger.Debug($"Database import took {timer.ElapsedMilliseconds}ms");
        _databaseServer.SetTables(dataToImport);
    }

    protected void OnReadValidate(string fileWithPath)
    {


        // Validate files
        //if (ProgramStatics.COMPILED && hashedFile && !ValidateFile(fileWithPath, data)) {
        //    this.valid = ValidationResult.FAILED;
        //}
    }

    protected bool ValidateFile(string filePathAndName, object fileData)
    {
        /*
        try {
            const finalPath = filePathAndName.replace(this.filepath, "").replace(".json", "");
            let tempObject: any;
            for (const prop of finalPath.split("/")) {
                if (!tempObject) {
                    tempObject = this.hashedFile[prop];
                } else {
                    tempObject = tempObject[prop];
                }
            }

            if (tempObject !== this.hashUtil.generateSha1ForData(fileData)) {
                this.logger.debug(this.localisationService.getText("validation_error_file", filePathAndName));
                return false;
            }
        } catch (e) {
            this.logger.warning(this.localisationService.getText("validation_error_exception", filePathAndName));
            this.logger.warning(e);
            return false;
        }
        return true;
        */
        return true;
    }

    /**
     * absolute dogshit, do not use
     * Find and map files with image router inside a designated path
     * @param filepath Path to find files in
     */
    [Obsolete]
    public void LoadImages(string filepath, string[] directories, List<string> routes)
    {
        for (var i = 0; i < directories.Length; i++)
        {
            // Get all files in directory
            var filesInDirectory = _fileUtil.GetFiles(directories[i]);
            foreach (var file in filesInDirectory)
            {
                var imagePath = file;
                // Register each file in image router
                var filename = _fileUtil.StripExtension(file);
                var routeKey = $"{routes[i]}{filename}";
                //var imagePath = $"{filepath}{directories[i]}/{file}";

                var pathOverride = GetImagePathOverride(imagePath);
                if (!string.IsNullOrEmpty(pathOverride))
                {
                    if (_logger.IsLogEnabled(LogLevel.Debug))
                    {
                        _logger.Debug($"overrode route: {routeKey} endpoint: {imagePath} with {pathOverride}");
                    }

                    imagePath = pathOverride;
                }

                _imageRouter.AddRoute(routeKey, imagePath);
            }
        }

        // Map icon file separately
        _imageRouter.AddRoute("/favicon.ico", $"{filepath}icon.ico");
    }

    /**
     * Check for a path override in the http json config file
     * @param imagePath Key
     * @returns override for key
     */
    protected string? GetImagePathOverride(string imagePath)
    {
        if (httpConfig.ServerImagePathOverride.TryGetValue(imagePath, out var value))
        {
            return value;
        }

        return null;
    }
}

internal enum ValidationResult
{
    SUCCESS = 0,
    FAILED = 1,
    NOT_FOUND = 2,
    UNDEFINED = 3
}
