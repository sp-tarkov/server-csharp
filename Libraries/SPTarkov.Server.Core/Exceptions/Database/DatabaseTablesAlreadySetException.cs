namespace SPTarkov.Server.Core.Exceptions.Database;

internal class DatabaseTablesAlreadySetException : Exception
{
    public DatabaseTablesAlreadySetException(string message)
        : base(message) { }

    public DatabaseTablesAlreadySetException(string message, Exception innerException)
        : base(message, innerException) { }
}
