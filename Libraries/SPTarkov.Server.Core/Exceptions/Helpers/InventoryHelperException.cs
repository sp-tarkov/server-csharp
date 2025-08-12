namespace SPTarkov.Server.Core.Exceptions.Helpers;

public class InventoryHelperException : Exception
{
    public InventoryHelperException(string message)
        : base(message) { }

    public InventoryHelperException(string message, Exception innerException)
        : base(message, innerException) { }
}
