using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Exceptions.Database;
using SPTarkov.Server.Core.Models.Spt.Server;

namespace SPTarkov.Server.Core.Servers;

[Injectable(InjectionType.Singleton)]
public class DatabaseServer
{
    protected DatabaseTables? TableData { get; private set; }

    /// <summary>
    /// Gets the database tables.
    /// </summary>
    /// <returns>The database tables if they have been initialized.</returns>
    /// <exception cref="DatabaseNullException">Thrown when the database tables have not been initialized.</exception>
    public DatabaseTables GetTables()
    {
        if (TableData is null)
        {
            throw new DatabaseNullException("Database tables have not been initialized!");
        }

        return TableData;
    }

    /// <summary>
    /// Sets the database tables for this instance. Can only be called once.
    /// </summary>
    /// <param name="tables">The database tables to set.</param>
    /// <exception cref="DatabaseTablesAlreadySetException">Thrown if the database tables are already set.</exception>
    internal void SetTables(DatabaseTables tables)
    {
        if (TableData is null)
        {
            TableData = tables;
        }
        else
        {
            throw new DatabaseTablesAlreadySetException("The database is already initialized!");
        }
    }
}
