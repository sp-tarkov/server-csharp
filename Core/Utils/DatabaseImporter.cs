using Core.Annotations;
using Core.DI;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Spt.Server;
using Core.Servers;
using Core.Services;
using ILogger = Core.Models.Utils.ILogger;

namespace Core.Utils;

[Injectable(InjectionType.Singleton, typePriority: 0)]
public class DatabaseImporter : OnLoad
{
    private object hashedFile;
    private ValidationResult valid = ValidationResult.UNDEFINED;
    private string filepath;
    protected HttpConfig httpConfig;

    protected readonly ILogger _logger;
    protected readonly LocalisationService _localisationService;

    protected readonly DatabaseServer _databaseServer;

    //protected readonly ImageRouter _imageRouter;
    protected readonly EncodingUtil _encodingUtil;
    protected readonly HashUtil _hashUtil;
    protected readonly ImporterUtil _importerUtil;
    protected readonly ConfigServer _configServer;

    public DatabaseImporter(
        ILogger logger,
        // TODO: are we gonna use this? @inject("JsonUtil") protected jsonUtil: JsonUtil,
        LocalisationService localisationService,
        DatabaseServer databaseServer,
        //ImageRouter imageRouter,
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
        httpConfig = _configServer.GetConfig<HttpConfig>(ConfigTypes.HTTP);
    }

    /**
     * Get path to spt data
     * @returns path to data
     */
    public string GetSptDataPath()
    {
        return "./Assets/";
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

        var imageFilePath = $"${filepath}images/";
        /*
        var directories = this.vfs.getDirs(imageFilePath);
        this.loadImages(imageFilePath, directories, [
            "/files/achievement/",
            "/files/CONTENT/banners/",
            "/files/handbook/",
            "/files/Hideout/",
            "/files/launcher/",
            "/files/prestige/",
            "/files/quest/icon/",
            "/files/trader/avatar/",
        ]);
        */
    }

    /**
     * Read all json files in database folder and map into a json object
     * @param filepath path to database folder
     */
    protected async Task HydrateDatabase(string filepath)
    {
        _logger.Info(_localisationService.GetText("importing_database"));

        var dataToImport = await _importerUtil.LoadRecursiveAsync(
            $"{filepath}database/",
            typeof(DatabaseTables),
            OnReadValidate
        );

        var validation = valid == ValidationResult.FAILED || valid == ValidationResult.NOT_FOUND ? "." : "";
        _logger.Info($"{_localisationService.GetText("importing_database_finish")}{validation}");
        _databaseServer.SetTables((DatabaseTables)dataToImport);
    }

    protected void OnReadValidate(string fileWithPath, string data)
    {
        // Validate files
        //if (ProgramStatics.COMPILED && hashedFile && !ValidateFile(fileWithPath, data)) {
        //    this.valid = ValidationResult.FAILED;
        //}
    }

    public string GetRoute()
    {
        return "spt-database";
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
     * Find and map files with image router inside a designated path
     * @param filepath Path to find files in
     */
    public void LoadImages(string filepath, List<string> directories, List<string> routes)
    {
        /*
        for (const directoryIndex in directories) {
            // Get all files in directory
            const filesInDirectory = this.vfs.getFiles(`${filepath}${directories[directoryIndex]}`);
            for (const file of filesInDirectory) {
                // Register each file in image router
                const filename = this.vfs.stripExtension(file);
                const routeKey = `${routes[directoryIndex]}${filename}`;
                let imagePath = `${filepath}${directories[directoryIndex]}/${file}`;

                const pathOverride = this.getImagePathOverride(imagePath);
                if (pathOverride) {
                    this.logger.debug(`overrode route: ${routeKey} endpoint: ${imagePath} with ${pathOverride}`);
                    imagePath = pathOverride;
                }

                this.imageRouter.addRoute(routeKey, imagePath);
            }
        }

        // Map icon file separately
        this.imageRouter.addRoute("/favicon.ico", `${filepath}icon.ico`);
        */
    }

    /**
     * Check for a path override in the http json config file
     * @param imagePath Key
     * @returns override for key
     */
    protected string GetImagePathOverride(string imagePath)
    {
        return httpConfig.ServerImagePathOverride[imagePath];
    }
}

internal enum ValidationResult
{
    SUCCESS = 0,
    FAILED = 1,
    NOT_FOUND = 2,
    UNDEFINED = 3
}