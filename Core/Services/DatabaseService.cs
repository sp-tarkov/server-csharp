using System.Diagnostics;
using Core.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Spt.Bots;
using Core.Models.Spt.Config;
using Core.Models.Spt.Server;
using Core.Models.Spt.Templates;
using Core.Servers;
using Core.Utils;
using Hideout = Core.Models.Spt.Hideout.Hideout;
using ILogger = Core.Models.Utils.ILogger;
using Locations = Core.Models.Spt.Server.Locations;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class DatabaseService
{
    protected LocationConfig locationConfig;
    protected bool isDataValid;

    private readonly ILogger _logger;
    private readonly DatabaseServer _databaseServer;
    private readonly LocalisationService _localisationService;
    private readonly HashUtil _hashUtil;

    public DatabaseService(
        ILogger logger,
        DatabaseServer databaseServer,
        LocalisationService localisationService,
        HashUtil hashUtil
    )
    {
        _logger = logger;
        _databaseServer = databaseServer;
        _localisationService = localisationService;
        _hashUtil = hashUtil;
    }

    /**
     * @returns assets/database/
     */
    public DatabaseTables GetTables()
    {
        return _databaseServer.GetTables();
    }

    /**
     * @returns assets/database/bots/
     */
    public Bots GetBots()
    {
        if (_databaseServer.GetTables().bots == null)
        {
            throw new Exception(_localisationService.GetText("database-data_at_path_missing", "assets/database/bots"));
        }

        return _databaseServer.GetTables().bots!;
    }

    /**
     * @returns assets/database/globals.json
     */
    public Globals GetGlobals()
    {
        if (_databaseServer.GetTables().globals == null)
        {
            throw new Exception(_localisationService.GetText("database-data_at_path_missing",
                "assets/database/globals.json"));
        }

        return _databaseServer.GetTables().globals!;
    }

    /**
     * @returns assets/database/hideout/
     */
    public Hideout GetHideout()
    {
        if (_databaseServer.GetTables().hideout == null)
        {
            throw new Exception(
                _localisationService.GetText("database-data_at_path_missing", "assets/database/hideout"));
        }

        return _databaseServer.GetTables().hideout!;
    }

    /**
     * @returns assets/database/locales/
     */
    public LocaleBase GetLocales()
    {
        if (_databaseServer.GetTables().locales == null)
        {
            throw new Exception(
                _localisationService.GetText("database-data_at_path_missing", "assets/database/locales"));
        }

        return _databaseServer.GetTables().locales!;
    }

    /**
     * @returns assets/database/locations
     */
    public Locations GetLocations()
    {
        if (_databaseServer.GetTables().locations == null)
        {
            throw new Exception(
                _localisationService.GetText("database-data_at_path_missing", "assets/database/locales"));
        }

        return _databaseServer.GetTables().locations!;
    }

    /**
     * Get specific location by its Id
     * @param locationId Desired location id
     * @returns assets/database/locations/
     */
    public Location GetLocation(string locationId)
    {
        var locations = GetLocations();
        var desiredLocation = locations[locationId.ToLower()];
        if (desiredLocation == null)
        {
            throw new Exception(_localisationService.GetText("database-no_location_found_with_id", locationId));
        }

        return desiredLocation;
    }

    /**
     * @returns assets/database/match/
     */
    public Match GetMatch()
    {
        if (_databaseServer.GetTables().match == null)
        {
            throw new Exception(
                _localisationService.GetText("database-data_at_path_missing", "assets/database/locales"));
        }

        return _databaseServer.GetTables().match!;
    }

    /**
     * @returns assets/database/server.json
     */
    public ServerBase GetServer()
    {
        if (_databaseServer.GetTables().server == null)
        {
            throw new Exception(_localisationService.GetText("database-data_at_path_missing",
                "assets/database/server.json"));
        }

        return _databaseServer.GetTables().server!;
    }

    /**
     * @returns assets/database/settings.json
     */
    public SettingsBase GetSettings()
    {
        if (_databaseServer.GetTables().settings == null)
        {
            throw new Exception(_localisationService.GetText("database-data_at_path_missing",
                "assets/database/settings.json"));
        }

        return _databaseServer.GetTables().settings!;
    }

    /**
     * @returns assets/database/templates/
     */
    public Templates GetTemplates()
    {
        if (_databaseServer.GetTables().templates == null)
        {
            throw new Exception(_localisationService.GetText("database-data_at_path_missing",
                "assets/database/templates"));
        }

        return _databaseServer.GetTables().templates!;
    }

    /**
     * @returns assets/database/templates/achievements.json
     */
    public List<Achievement> GetAchievements()
    {
        if (_databaseServer.GetTables().templates?.Achievements == null)
        {
            throw new Exception(_localisationService.GetText("database-data_at_path_missing",
                "assets/database/templates/achievements.json"));
        }

        return _databaseServer.GetTables().templates?.Achievements!;
    }

    /**
     * @returns assets/database/templates/customisation.json
     */
    public Dictionary<string, CustomizationItem> GetCustomization()
    {
        if (_databaseServer.GetTables().templates?.Customization == null)
        {
            throw new Exception(_localisationService.GetText("database-data_at_path_missing",
                "assets/database/templates/customization.json"));
        }

        return _databaseServer.GetTables().templates?.Customization!;
    }

    /**
     * @returns assets/database/templates/handbook.json
     */
    public HandbookBase GetHandbook()
    {
        if (_databaseServer.GetTables().templates?.Handbook == null)
        {
            throw new Exception(_localisationService.GetText("database-data_at_path_missing", "assets/database/templates/handbook.json"));
        }

        return _databaseServer.GetTables().templates?.Handbook!;
    }

    /**
     * @returns assets/database/templates/items.json
     */
    public Dictionary<string, TemplateItem> GetItems() {
        if (_databaseServer.GetTables().templates?.Items == null)
        {
            throw new Exception(_localisationService.GetText("database-data_at_path_missing", "assets/database/templates/items.json"));
        }

        return _databaseServer.GetTables().templates?.Items!;
    }

    /**
     * @returns assets/database/templates/prices.json
     */
    public Dictionary<string, double> GetPrices() {
        if (_databaseServer.GetTables().templates?.Prices == null)
        {
            throw new Exception(_localisationService.GetText("database-data_at_path_missing", "assets/database/templates/prices.json"));
        }

        return _databaseServer.GetTables().templates?.Prices!;
    }

    /**
     * @returns assets/database/templates/profiles.json
     */
    public ProfileTemplates GetProfiles()
    {
        if (_databaseServer.GetTables().templates?.Profiles == null)
        {
            throw new Exception(_localisationService.GetText("database-data_at_path_missing", "assets/database/templates/profiles.json"));
        }

        return _databaseServer.GetTables().templates?.Profiles!;
    }

    /**
     * @returns assets/database/templates/quests.json
     */
    public Dictionary<string, Quest> GetQuests() {
        if (_databaseServer.GetTables().templates?.Quests == null)
        {
            throw new Exception(_localisationService.GetText("database-data_at_path_missing", "assets/database/templates/quests.json"));
        }

        return _databaseServer.GetTables().templates?.Quests!;
    }

    /**
     * @returns assets/database/traders/
     */
    public Dictionary<string, Trader> GetTraders() {
        if (_databaseServer.GetTables().traders == null)
        {
            throw new Exception(_localisationService.GetText("database-data_at_path_missing", "assets/database/traders"));
        }

        return _databaseServer.GetTables().traders!;
    }

    /**
     * Get specific trader by their Id
     * @param traderId Desired trader id
     * @returns assets/database/traders/
     */
    public Trader GetTrader(string traderId) 
    {
        var traders = GetTraders();
        if (!traders.TryGetValue(traderId, out var desiredTrader))
        {
            throw new Exception(_localisationService.GetText("database-no_trader_found_with_id", traderId));
        }

        return desiredTrader;
    }

    /**
     * @returns assets/database/locationServices/
     */
    public LocationServices GetLocationServices() {
        if (_databaseServer.GetTables().templates?.LocationServices == null)
        {
            throw new Exception(_localisationService.GetText("database-data_at_path_missing", "assets/database/locationServices.json"));
        }

        return _databaseServer.GetTables().templates?.LocationServices!;
    }

    /**
     * Validates that the database doesn't contain invalid ID data
     */
    public void ValidateDatabase() {
        var start  = Stopwatch.StartNew();

        isDataValid =
            ValidateTable(GetQuests(), "quest") &&
            ValidateTable(GetTraders(), "trader") &&
            ValidateTable(GetItems(), "item") &&
            ValidateTable(GetCustomization(), "customization");

        if (!isDataValid)
        {
            _logger.Error(_localisationService.GetText("database-invalid_data"));
        }

        start.Stop();
        _logger.Debug($"ID validation took: {start.ElapsedMilliseconds}ms");
    }

    /**
     * Validate that the given table only contains valid MongoIDs
     * @param table Table to validate for MongoIDs
     * @param tableType The type of table, used in output message
     * @returns True if the table only contains valid data
     */
    private bool ValidateTable<T>(Dictionary<string, T> table, string tableType) {
        foreach (var keyValuePair in table)
        {
            if (!_hashUtil.IsValidMongoId(keyValuePair.Key))
            {
                _logger.Error($"Invalid {tableType} ID: '{keyValuePair.Key}'");
                return false;
            }
        }

        return true;
    }

    /**
     * Check if the database is valid
     * @returns True if the database contains valid data, false otherwise
     */
    public bool IsDatabaseValid() => isDataValid;
}