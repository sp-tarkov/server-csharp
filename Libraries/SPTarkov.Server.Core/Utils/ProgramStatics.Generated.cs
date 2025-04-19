using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Utils;

public static partial class ProgramStatics
{
    private static string? _sptVersion = "4.0.0";
    private static string? _commit = "a12b34";
    private static double? _buildTime = 0000000000;
    private static EntryType? BuildType = EntryType.LOCAL;
}
