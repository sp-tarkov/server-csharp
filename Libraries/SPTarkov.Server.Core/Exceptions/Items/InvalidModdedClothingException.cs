namespace SPTarkov.Server.Core.Exceptions.Items;

public class InvalidModdedClothingException : Exception
{
    public InvalidModdedClothingException(string message)
        : base(message) { }

    public InvalidModdedClothingException(string message, Exception innerException)
        : base(message, innerException) { }

    public override string? StackTrace
    {
        get { return null; }
    }
}
