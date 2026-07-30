namespace Ceciler.Errors;

public static class ErrorCodes
{
    public static readonly Error ParameterCount = new(1, "This application expects 2 parameters. Ie: ./myTarget.dll ./myPatch.dll");

    public static readonly Error InvalidParameterValue = new(
        2,
        "Parameters {0} is invalid, file not found. Ie: ./myTarget.dll ./myPatch.dll"
    );

    public static readonly Error UnexpectedError = new(3, "Unexpected error. Exception:\n{0}");
    public static readonly Error PatchError = new(4, "Patch {0} failed! Check exception log for more details.");
    public static readonly Error PatchesNotFound = new(
        5,
        "Patches not found for {0}. Make sure the arguments are on the right order.Ie: ./myTarget.dll ./myPatch.dll "
    );
}
