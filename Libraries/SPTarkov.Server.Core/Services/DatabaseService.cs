using SPTarkov.Common.Extensions;
using SPTarkov.DI.Annotations;
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
    /// <returns> assets/database/ </returns>
    public DatabaseTables GetTables()
    {
        return databaseServer.GetTables();
    }

    /// <returns> assets/database/bots/ </returns>
    public Bots GetBots()
    {
        return databaseServer.GetTables().Bots;
    }

    /// <returns> assets/database/globals.json </returns>
    public Globals GetGlobals()
    {
        return databaseServer.GetTables().Globals;
    }

    /// <returns> assets/database/hideout/ </returns>
    public Hideout GetHideout()
    {
        return databaseServer.GetTables().Hideout;
    }

    /// <returns> assets/database/locales/ </returns>
    public LocaleBase GetLocales()
    {
        return databaseServer.GetTables().Locales;
    }

    /// <returns> assets/database/locations </returns>
    public Locations GetLocations()
    {
        return databaseServer.GetTables().Locations;
    }

    /// <summary>
    ///     Get specific location by its ID, automatically ToLowers id
    /// </summary>
    /// <param name="locationId"> Desired location ID </param>
    /// <returns> assets/database/locations/ </returns>
    public Location? GetLocation(string locationId)
    {
        var desiredLocation = GetLocations().GetByJsonProperty<Location>(locationId.ToLowerInvariant());
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
        return databaseServer.GetTables().Match;
    }

    /// <returns> assets/database/server.json </returns>
    public ServerBase GetServer()
    {
        return databaseServer.GetTables().Server;
    }

    /// <returns> assets/database/settings.json </returns>
    public SettingsBase GetSettings()
    {
        return databaseServer.GetTables().Settings;
    }

    /// <returns> assets/database/templates/ </returns>
    public Templates GetTemplates()
    {
        return databaseServer.GetTables().Templates;
    }

    /// <returns> assets/database/templates/achievements.json </returns>
    public List<Achievement> GetAchievements()
    {
        return databaseServer.GetTables().Templates.Achievements;
    }

    /// <returns> assets/database/templates/customAchievements.json </returns>
    public List<Achievement> GetCustomAchievements()
    {
        return databaseServer.GetTables().Templates.CustomAchievements;
    }

    /// <returns> assets/database/templates/customisation.json </returns>
    public Dictionary<MongoId, CustomizationItem> GetCustomization()
    {
        return databaseServer.GetTables().Templates.Customization;
    }

    /// <returns> assets/database/templates/handbook.json </returns>
    public HandbookBase GetHandbook()
    {
        return databaseServer.GetTables().Templates.Handbook;
    }

    /// <returns> assets/database/templates/items.json </returns>
    public Dictionary<MongoId, TemplateItem> GetItems()
    {
        return databaseServer.GetTables().Templates.Items;
    }

    /// <returns> assets/database/templates/prices.json </returns>
    public Dictionary<MongoId, double> GetPrices()
    {
        return databaseServer.GetTables().Templates.Prices;
    }

    /// <returns> assets/database/templates/profiles.json </returns>
    public Dictionary<string, ProfileSides> GetProfileTemplates()
    {
        return databaseServer.GetTables().Templates.Profiles;
    }

    /// <returns> assets/database/templates/quests.json </returns>
    public Dictionary<MongoId, Quest> GetQuests()
    {
        return databaseServer.GetTables().Templates.Quests;
    }

    /// <returns> assets/database/traders/ </returns>
    public Dictionary<MongoId, Trader> GetTraders()
    {
        return databaseServer.GetTables().Traders;
    }

    /// <summary>
    ///     Get specific trader by their ID
    /// </summary>
    /// <param name="traderId"> Desired trader ID </param>
    /// <returns> assets/database/traders/ </returns>
    public Trader? GetTrader(MongoId traderId)
    {
        return databaseServer.GetTables().Traders.GetValueOrDefault(traderId);
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
}
