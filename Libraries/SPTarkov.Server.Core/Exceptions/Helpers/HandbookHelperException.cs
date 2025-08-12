namespace SPTarkov.Server.Core.Exceptions.Helpers;

public class HandbookHelperException : Exception
{
    public HandbookHelperException(string message)
        : base(message) { }

    public HandbookHelperException(string message, Exception innerException)
        : base(message, innerException) { }
}
