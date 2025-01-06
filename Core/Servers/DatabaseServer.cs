using Core.Models.Spt.Server;

namespace Core.Servers;

public class DatabaseServer
{
    protected DatabaseTables tableData = new();

    public DatabaseTables GetTables() => tableData;

    public void SetTables(DatabaseTables tables) => tableData = tables;
}