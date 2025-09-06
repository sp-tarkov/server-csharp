namespace SPTarkov.Reflection.Patching;

public class PatchException : Exception
{
    public PatchException(string message)
        : base(message) { }

    public PatchException(string message, Exception innerException)
        : base(message, innerException) { }
}
