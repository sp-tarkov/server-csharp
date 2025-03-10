using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;

namespace CreateSqlLiteDb
{
    [Injectable]
    public class CreateSqlLiteDb(
        ISptLogger<CreateSqlLiteDb> _logger,
        DatabaseServer databaseServer,
        IEnumerable<IOnLoad> onLoadComponents
    )
    {

        public async Task Run()
        {
            // Only need db event
            var dbOnload = onLoadComponents.FirstOrDefault(x => x.GetRoute() == "spt-database");
            await dbOnload.OnLoad();

            var converter = new JsonToSqliteConverter("dbtest.db");

            // poc data we want to store
            var areas = databaseServer.GetTables().Hideout.Areas;
            var customisation = databaseServer.GetTables().Hideout.Customisation;
            var production = databaseServer.GetTables().Hideout.Production;
            var qte = databaseServer.GetTables().Hideout.Qte;
            var settings = databaseServer.GetTables().Hideout.Settings;

            await converter.ConvertCollectionToSqliteAsync(areas);
            await converter.ConvertObjectToSqliteAsync(customisation);
            await converter.ConvertObjectToSqliteAsync(production);
            await converter.ConvertCollectionToSqliteAsync(qte);
            await converter.ConvertObjectToSqliteAsync(settings);

            // Figure out our source and target directories
            // TODO: write db somewhere? or is that handled above?
            var projectDir = Directory.GetParent("./").Parent.Parent.Parent.Parent.Parent;
            }
    }
}

