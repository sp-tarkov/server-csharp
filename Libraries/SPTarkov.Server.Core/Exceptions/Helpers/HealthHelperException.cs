namespace SPTarkov.Server.Core.Exceptions.Helpers;

public class HealthHelperException : Exception
{
    public HealthHelperException(string message)
        : base(message) { }

    public HealthHelperException(string message, Exception innerException)
        : base(message, innerException) { }
}
