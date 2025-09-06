namespace SPTarkov.Server.Core.Exceptions.Helpers;

public class DurabilityHelperException : Exception
{
    public DurabilityHelperException(string message)
        : base(message) { }

    public DurabilityHelperException(string message, Exception innerException)
        : base(message, innerException) { }
}
