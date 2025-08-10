namespace SPTarkov.Server.Core.Exceptions.Items;

public class ItemHelperException : Exception
{
    public ItemHelperException(string message)
        : base(message) { }

    public ItemHelperException(string message, Exception innerException)
        : base(message, innerException) { }
}
