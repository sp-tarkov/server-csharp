using System.Diagnostics;
using SptCommon.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Spt.Bots;
using Core.Models.Spt.Server;
using Core.Models.Spt.Templates;
using Core.Models.Utils;
using Core.Servers;
using Core.Utils;
using SptCommon.Extensions;
using Hideout = Core.Models.Spt.Hideout.Hideout;
using Locations = Core.Models.Spt.Server.Locations;
using LogLevel = Core.Models.Spt.Logging.LogLevel;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class DatabaseService(
    ISptLogger<DatabaseService> _logger,
    DatabaseServer _databaseServer,
    LocalisationService _localisationService,
    HashUtil _hashUtil
)
{
    protected bool isDataValid = true;

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
        if (_databaseServer.GetTables().Bots == null)
            throw new Exception(_localisationService.GetText("database-data_at_path_missing", "assets/database/bots"));

        return _databaseServer.GetTables().Bots!;
    }

    /**
     * @returns assets/database/globals.json
     */
    public Globals GetGlobals()
    {
        if (_databaseServer.GetTables().Globals == null)
            throw new Exception(
                _localisationService.GetText(
                    "database-data_at_path_missing",
                    "assets/database/globals.json"
                )
            );

        return _databaseServer.GetTables().Globals!;
    }

    /**
     * @returns assets/database/hideout/
     */
    public Hideout GetHideout()
    {
        if (_databaseServer.GetTables().Hideout == null)
            throw new Exception(
                _localisationService.GetText("database-data_at_path_missing", "assets/database/hideout")
            );

        return _databaseServer.GetTables().Hideout!;
    }

    /**
     * @returns assets/database/locales/
     */
    public LocaleBase GetLocales()
    {
        if (_databaseServer.GetTables().Locales == null)
            throw new Exception(
                _localisationService.GetText("database-data_at_path_missing", "assets/database/locales")
            );

        return _databaseServer.GetTables().Locales!;
    }

    /**
     * @returns assets/database/locations
     */
    public Locations GetLocations()
    {
        if (_databaseServer.GetTables().Locations == null)
            throw new Exception(
                _localisationService.GetText("database-data_at_path_missing", "assets/database/locales")
            );

        return _databaseServer.GetTables().Locations!;
    }

    /**
     * Get specific location by its Id
     * @param locationId Desired location id
     * @returns assets/database/locations/
     */
    public Location GetLocation(string locationId)
    {
        var locations = GetLocations();
        var desiredLocation = locations.GetByJsonProp<Location>(locationId.ToLower());
        if (desiredLocation == null) throw new Exception(_localisationService.GetText("database-no_location_found_with_id", locationId));

        return desiredLocation;
    }

    /**
     * @returns assets/database/match/
     */
    public Match GetMatch()
    {
        if (_databaseServer.GetTables().Match == null)
            throw new Exception(
                _localisationService.GetText("database-data_at_path_missing", "assets/database/locales")
            );

        return _databaseServer.GetTables().Match!;
    }

    /**
     * @returns assets/database/server.json
     */
    public ServerBase GetServer()
    {
        if (_databaseServer.GetTables().Server == null)
            throw new Exception(
                _localisationService.GetText(
                    "database-data_at_path_missing",
                    "assets/database/server.json"
                )
            );

        return _databaseServer.GetTables().Server!;
    }

    /**
     * @returns assets/database/settings.json
     */
    public SettingsBase GetSettings()
    {
        if (_databaseServer.GetTables().Settings == null)
            throw new Exception(
                _localisationService.GetText(
                    "database-data_at_path_missing",
                    "assets/database/settings.json"
                )
            );

        return _databaseServer.GetTables().Settings!;
    }

    /**
     * @returns assets/database/templates/
     */
    public Templates GetTemplates()
    {
        if (_databaseServer.GetTables().Templates == null)
            throw new Exception(
                _localisationService.GetText(
                    "database-data_at_path_missing",
                    "assets/database/templates"
                )
            );

        return _databaseServer.GetTables().Templates!;
    }

    /**
     * @returns assets/database/templates/achievements.json
     */
    public List<Achievement> GetAchievements()
    {
        if (_databaseServer.GetTables().Templates?.Achievements == null)
            throw new Exception(
                _localisationService.GetText(
                    "database-data_at_path_missing",
                    "assets/database/templates/achievements.json"
                )
            );

        return _databaseServer.GetTables().Templates?.Achievements!;
    }

    /**
     * @returns assets/database/templates/customisation.json
     */
    public Dictionary<string, CustomizationItem?> GetCustomization()
    {
        if (_databaseServer.GetTables().Templates?.Customization == null)
            throw new Exception(
                _localisationService.GetText(
                    "database-data_at_path_missing",
                    "assets/database/templates/customization.json"
                )
            );

        return _databaseServer.GetTables().Templates?.Customization!;
    }

    /**
     * @returns assets/database/templates/handbook.json
     */
    public HandbookBase GetHandbook()
    {
        if (_databaseServer.GetTables().Templates?.Handbook == null)
            throw new Exception(_localisationService.GetText("database-data_at_path_missing", "assets/database/templates/handbook.json"));

        return _databaseServer.GetTables().Templates?.Handbook!;
    }

    /**
     * @returns assets/database/templates/items.json
     */
    public Dictionary<string, TemplateItem> GetItems()
    {
        if (_databaseServer.GetTables().Templates?.Items == null)
            throw new Exception(_localisationService.GetText("database-data_at_path_missing", "assets/database/templates/items.json"));

        return _databaseServer.GetTables().Templates?.Items!;
    }

    /**
     * @returns assets/database/templates/prices.json
     */
    public Dictionary<string, double> GetPrices()
    {
        if (_databaseServer.GetTables().Templates?.Prices == null)
            throw new Exception(_localisationService.GetText("database-data_at_path_missing", "assets/database/templates/prices.json"));

        return _databaseServer.GetTables().Templates?.Prices!;
    }

    /**
     * @returns assets/database/templates/profiles.json
     */
    public ProfileTemplates GetProfiles()
    {
        if (_databaseServer.GetTables().Templates?.Profiles == null)
            throw new Exception(_localisationService.GetText("database-data_at_path_missing", "assets/database/templates/profiles.json"));

        return _databaseServer.GetTables().Templates?.Profiles!;
    }

    /**
     * @returns assets/database/templates/quests.json
     */
    public Dictionary<string, Quest> GetQuests()
    {
        if (_databaseServer.GetTables().Templates?.Quests == null)
            throw new Exception(_localisationService.GetText("database-data_at_path_missing", "assets/database/templates/quests.json"));

        return _databaseServer.GetTables().Templates?.Quests!;
    }

    /**
     * @returns assets/database/traders/
     */
    public Dictionary<string, Trader> GetTraders()
    {
        if (_databaseServer.GetTables().Traders == null)
            throw new Exception(_localisationService.GetText("database-data_at_path_missing", "assets/database/traders"));

        return _databaseServer.GetTables().Traders!;
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
            throw new Exception(_localisationService.GetText("database-no_trader_found_with_id", traderId));

        return desiredTrader;
    }

    /**
     * @returns assets/database/locationServices/
     */
    public LocationServices GetLocationServices()
    {
        if (_databaseServer.GetTables().Templates?.LocationServices == null)
            throw new Exception(_localisationService.GetText("database-data_at_path_missing", "assets/database/locationServices.json"));

        return _databaseServer.GetTables().Templates?.LocationServices!;
    }

    /**
     * Validates that the database doesn't contain invalid ID data
     */
    public void ValidateDatabase()
    {
        var start = Stopwatch.StartNew();

        isDataValid =
            ValidateTable(GetQuests(), "quest") &&
            ValidateTable(GetTraders(), "trader") &&
            ValidateTable(GetItems(), "item") &&
            ValidateTable(GetCustomization(), "customization");

        if (!isDataValid) _logger.Error(_localisationService.GetText("database-invalid_data"));

        start.Stop();
        if (_logger.IsLogEnabled(LogLevel.Debug)) _logger.Debug($"ID validation took: {start.ElapsedMilliseconds}ms");
    }

    /**
     * Validate that the given table only contains valid MongoIDs
     * @param table Table to validate for MongoIDs
     * @param tableType The type of table, used in output message
     * @returns True if the table only contains valid data
     */
    private bool ValidateTable<T>(Dictionary<string, T> table, string tableType)
    {
        foreach (var keyValuePair in table)
            if (!_hashUtil.IsValidMongoId(keyValuePair.Key))
            {
                _logger.Error($"Invalid {tableType} ID: '{keyValuePair.Key}'");
                return false;
            }

        return true;
    }

    /**
     * Check if the database is valid
     * @returns True if the database contains valid data, false otherwise
     */
    public bool IsDatabaseValid()
    {
        return isDataValid;
    }
}
