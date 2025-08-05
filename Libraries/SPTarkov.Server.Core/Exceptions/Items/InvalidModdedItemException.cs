namespace SPTarkov.Server.Core.Exceptions.Items;

public class InvalidModdedItemException : Exception
{
    public InvalidModdedItemException(string message)
        : base(message) { }

    public InvalidModdedItemException(string message, Exception innerException)
        : base(message, innerException) { }

    public override string? StackTrace
    {
        get { return null; }
    }
}
