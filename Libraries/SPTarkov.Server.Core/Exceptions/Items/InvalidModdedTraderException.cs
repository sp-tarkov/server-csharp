namespace SPTarkov.Server.Core.Exceptions.Items;

public class InvalidModdedTraderException : Exception
{
    public InvalidModdedTraderException(string message)
        : base(message) { }

    public InvalidModdedTraderException(string message, Exception innerException)
        : base(message, innerException) { }

    public override string? StackTrace
    {
        get { return null; }
    }
}
