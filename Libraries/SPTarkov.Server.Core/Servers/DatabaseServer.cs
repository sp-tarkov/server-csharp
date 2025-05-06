using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Spt.Server;

namespace SPTarkov.Server.Core.Servers;

[Injectable(InjectionType.Singleton)]
public class DatabaseServer
{
    protected DatabaseTables tableData = new();

    public DatabaseTables GetTables()
    {
        return tableData;
    }

    public void SetTables(DatabaseTables tables)
    {
        tableData = tables;
    }
}
