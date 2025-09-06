namespace SPTarkov.Server.Core.Exceptions.Helpers;

public class InRaidHelperException : Exception
{
    public InRaidHelperException(string message)
        : base(message) { }

    public InRaidHelperException(string message, Exception innerException)
        : base(message, innerException) { }
}
