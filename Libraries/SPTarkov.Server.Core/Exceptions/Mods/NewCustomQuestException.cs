namespace SPTarkov.Server.Core.Exceptions.Mods;

public class NewCustomQuestException : Exception
{
    public NewCustomQuestException(string message)
        : base(message) { }

    public NewCustomQuestException(string message, Exception innerException)
        : base(message, innerException) { }
}
