using Core.Models.Spt.Server;
using SptCommon.Annotations;

namespace Core.Servers;

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
