namespace SPTarkov.Server.Exceptions;

public sealed class ModLoaderException : Exception
{
    public ModLoaderException(string message)
        : base(message) { }

    public ModLoaderException(string message, Exception innerException)
        : base(message, innerException) { }
}
