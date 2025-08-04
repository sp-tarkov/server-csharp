using System.Diagnostics;
using SPTarkov.Common.Extensions;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Bots;
using SPTarkov.Server.Core.Models.Spt.Server;
using SPTarkov.Server.Core.Models.Spt.Templates;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using Hideout = SPTarkov.Server.Core.Models.Spt.Hideout.Hideout;
using Locations = SPTarkov.Server.Core.Models.Spt.Server.Locations;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Services;

/// <summary>
/// Provides access to the servers database, these are in-memory representations of the .JSON files stored inside `Libraries\SPTarkov.Server.Assets\Assets\database`
/// </summary>
[Injectable(InjectionType.Singleton)]
public class DatabaseService(
    ISptLogger<DatabaseService> logger,
    DatabaseServer databaseServer,
    ServerLocalisationService serverLocalisationService
)
{
    private bool _isDataValid = true;

    /// <returns> assets/database/ </returns>
    public DatabaseTables GetTables()
    {
        return databaseServer.GetTables();
    }

    /// <returns> assets/database/bots/ </returns>
    public Bots GetBots()
    {
        if (databaseServer.GetTables().Bots == null)
        {
            throw new Exception(serverLocalisationService.GetText("database-data_at_path_missing", "assets/database/bots"));
        }

        return databaseServer.GetTables().Bots!;
    }

    /// <returns> assets/database/globals.json </returns>
    public Globals GetGlobals()
    {
        if (databaseServer.GetTables().Globals == null)
        {
            throw new Exception(serverLocalisationService.GetText("database-data_at_path_missing", "assets/database/globals.json"));
        }

        return databaseServer.GetTables().Globals!;
    }

    /// <returns> assets/database/hideout/ </returns>
    public Hideout GetHideout()
    {
        if (databaseServer.GetTables().Hideout == null)
        {
            throw new Exception(serverLocalisationService.GetText("database-data_at_path_missing", "assets/database/hideout"));
        }

        return databaseServer.GetTables().Hideout!;
    }

    /// <returns> assets/database/locales/ </returns>
    public LocaleBase GetLocales()
    {
        if (databaseServer.GetTables().Locales == null)
        {
            throw new Exception(serverLocalisationService.GetText("database-data_at_path_missing", "assets/database/locales"));
        }

        return databaseServer.GetTables().Locales!;
    }

    /// <returns> assets/database/locations </returns>
    public Locations GetLocations()
    {
        if (databaseServer.GetTables().Locations == null)
        {
            throw new Exception(serverLocalisationService.GetText("database-data_at_path_missing", "assets/database/locations"));
        }

        return databaseServer.GetTables().Locations!;
    }

    /// <summary>
    ///     Get specific location by its ID, automatically ToLowers id
    /// </summary>
    /// <param name="locationId"> Desired location ID </param>
    /// <returns> assets/database/locations/ </returns>
    public Location? GetLocation(string locationId)
    {
        var desiredLocation = GetLocations()?.GetByJsonProp<Location>(locationId.ToLowerInvariant());
        if (desiredLocation == null)
        {
            logger.Error(serverLocalisationService.GetText("database-no_location_found_with_id", locationId));

            return null;
        }

        return desiredLocation;
    }

    /// <returns> assets/database/match/ </returns>
    public Match GetMatch()
    {
        if (databaseServer.GetTables().Match == null)
        {
            throw new Exception(serverLocalisationService.GetText("database-data_at_path_missing", "assets/database/locales"));
        }

        return databaseServer.GetTables().Match!;
    }

    /// <returns> assets/database/server.json </returns>
    public ServerBase GetServer()
    {
        if (databaseServer.GetTables().Server == null)
        {
            throw new Exception(serverLocalisationService.GetText("database-data_at_path_missing", "assets/database/server.json"));
        }

        return databaseServer.GetTables().Server!;
    }

    /// <returns> assets/database/settings.json </returns>
    public SettingsBase GetSettings()
    {
        if (databaseServer.GetTables().Settings == null)
        {
            throw new Exception(serverLocalisationService.GetText("database-data_at_path_missing", "assets/database/settings.json"));
        }

        return databaseServer.GetTables().Settings!;
    }

    /// <returns> assets/database/templates/ </returns>
    public Templates GetTemplates()
    {
        if (databaseServer.GetTables().Templates == null)
        {
            throw new Exception(serverLocalisationService.GetText("database-data_at_path_missing", "assets/database/templates"));
        }

        return databaseServer.GetTables().Templates!;
    }

    /// <returns> assets/database/templates/achievements.json </returns>
    public List<Achievement> GetAchievements()
    {
        if (databaseServer.GetTables().Templates?.Achievements == null)
        {
            throw new Exception(
                serverLocalisationService.GetText("database-data_at_path_missing", "assets/database/templates/achievements.json")
            );
        }

        return databaseServer.GetTables().Templates?.Achievements!;
    }

    /// <returns> assets/database/templates/customAchievements.json </returns>
    public List<Achievement> GetCustomAchievements()
    {
        if (databaseServer.GetTables().Templates?.Achievements == null)
        {
            throw new Exception(
                serverLocalisationService.GetText("database-data_at_path_missing", "assets/database/templates/customAchievements.json")
            );
        }

        return databaseServer.GetTables().Templates?.CustomAchievements!;
    }

    /// <returns> assets/database/templates/customisation.json </returns>
    public Dictionary<MongoId, CustomizationItem?> GetCustomization()
    {
        if (databaseServer.GetTables().Templates?.Customization == null)
        {
            throw new Exception(
                serverLocalisationService.GetText("database-data_at_path_missing", "assets/database/templates/customization.json")
            );
        }

        return databaseServer.GetTables().Templates?.Customization!;
    }

    /// <returns> assets/database/templates/handbook.json </returns>
    public HandbookBase GetHandbook()
    {
        if (databaseServer.GetTables().Templates?.Handbook == null)
        {
            throw new Exception(
                serverLocalisationService.GetText("database-data_at_path_missing", "assets/database/templates/handbook.json")
            );
        }

        return databaseServer.GetTables().Templates?.Handbook!;
    }

    /// <returns> assets/database/templates/items.json </returns>
    public Dictionary<MongoId, TemplateItem> GetItems()
    {
        if (databaseServer.GetTables().Templates?.Items == null)
        {
            throw new Exception(serverLocalisationService.GetText("database-data_at_path_missing", "assets/database/templates/items.json"));
        }

        return databaseServer.GetTables().Templates?.Items!;
    }

    /// <returns> assets/database/templates/prices.json </returns>
    public Dictionary<MongoId, double> GetPrices()
    {
        if (databaseServer.GetTables().Templates?.Prices == null)
        {
            throw new Exception(
                serverLocalisationService.GetText("database-data_at_path_missing", "assets/database/templates/prices.json")
            );
        }

        return databaseServer.GetTables().Templates?.Prices!;
    }

    /// <returns> assets/database/templates/profiles.json </returns>
    public Dictionary<string, ProfileSides> GetProfileTemplates()
    {
        if (databaseServer.GetTables().Templates?.Profiles == null)
        {
            throw new Exception(
                serverLocalisationService.GetText("database-data_at_path_missing", "assets/database/templates/profiles.json")
            );
        }

        return databaseServer.GetTables().Templates?.Profiles!;
    }

    /// <returns> assets/database/templates/quests.json </returns>
    public Dictionary<MongoId, Quest> GetQuests()
    {
        if (databaseServer.GetTables().Templates?.Quests == null)
        {
            throw new Exception(
                serverLocalisationService.GetText("database-data_at_path_missing", "assets/database/templates/quests.json")
            );
        }

        return databaseServer.GetTables().Templates?.Quests!;
    }

    /// <returns> assets/database/traders/ </returns>
    public Dictionary<MongoId, Trader> GetTraders()
    {
        if (databaseServer.GetTables().Traders == null)
        {
            throw new Exception(serverLocalisationService.GetText("database-data_at_path_missing", "assets/database/traders"));
        }

        return databaseServer.GetTables().Traders!;
    }

    /// <summary>
    ///     Get specific trader by their ID
    /// </summary>
    /// <param name="traderId"> Desired trader ID </param>
    /// <returns> assets/database/traders/ </returns>
    public Trader? GetTrader(MongoId traderId)
    {
        if (!databaseServer.GetTables().Traders.TryGetValue(traderId, out var desiredTrader))
        {
            logger.Error(serverLocalisationService.GetText("database-no_trader_found_with_id", traderId));

            return null;
        }

        return desiredTrader;
    }

    /// <returns> assets/database/locationServices/ </returns>
    public LocationServices GetLocationServices()
    {
        if (databaseServer.GetTables().Templates?.LocationServices == null)
        {
            throw new Exception(
                serverLocalisationService.GetText("database-data_at_path_missing", "assets/database/locationServices.json")
            );
        }

        return databaseServer.GetTables().Templates?.LocationServices!;
    }

    /// <summary>
    ///     Validates that the database doesn't contain invalid ID data
    /// </summary>
    public void ValidateDatabase()
    {
        var start = Stopwatch.StartNew();

        _isDataValid =
            ValidateTable(GetQuests(), "quest")
            && ValidateTable(GetTraders(), "trader")
            && ValidateTable(GetItems(), "item")
            && ValidateTable(GetCustomization(), "customization");

        if (!_isDataValid)
        {
            logger.Error(serverLocalisationService.GetText("database-invalid_data"));
        }

        start.Stop();
        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug($"ID validation took: {start.ElapsedMilliseconds}ms");
        }
    }

    /// <summary>
    ///     Validate that the given table only contains valid MongoIDs
    /// </summary>
    /// <param name="table"> Table to validate for MongoIDs</param>
    /// <param name="tableType"> The type of table, used in output message </param>
    /// <returns> True if the table only contains valid data </returns>
    private bool ValidateTable<T>(Dictionary<string, T> table, string tableType)
    {
        foreach (var keyValuePair in table)
        {
            if (!keyValuePair.Key.IsValidMongoId())
            {
                logger.Error($"Invalid {tableType} ID: '{keyValuePair.Key}'");
                return false;
            }
        }

        return true;
    }

    private bool ValidateTable<T>(Dictionary<MongoId, T> table, string tableType)
    {
        foreach (var keyValuePair in table)
        {
            if (!keyValuePair.Key.IsValidMongoId())
            {
                logger.Error($"Invalid {tableType} ID: '{keyValuePair.Key}'");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Check if the database is valid
    /// </summary>
    /// <returns> True if the database contains valid data, false otherwise </returns>
    public bool IsDatabaseValid()
    {
        return _isDataValid;
    }
}
