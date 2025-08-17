namespace SPTarkov.Server.Core.Exceptions.Database;

public class DatabaseNullException : Exception
{
    public DatabaseNullException(string message)
        : base(message) { }

    public DatabaseNullException(string message, Exception innerException)
        : base(message, innerException) { }
}
