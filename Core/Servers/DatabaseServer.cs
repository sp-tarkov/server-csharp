using Core.Annotations;
using Core.Models.Spt.Server;

namespace Core.Servers;

[Injectable(InjectionType.Singleton)]
public class DatabaseServer
{
    protected DatabaseTables tableData = new();

    public DatabaseTables GetTables() => tableData;

    public void SetTables(DatabaseTables tables) => tableData = tables;
}