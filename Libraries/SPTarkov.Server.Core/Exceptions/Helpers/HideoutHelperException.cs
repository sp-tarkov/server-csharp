namespace SPTarkov.Server.Core.Exceptions.Helpers;

public class HideoutHelperException : Exception
{
    public HideoutHelperException(string message)
        : base(message) { }

    public HideoutHelperException(string message, Exception innerException)
        : base(message, innerException) { }
}
