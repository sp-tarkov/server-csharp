using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Utils;

public static partial class ProgramStatics
{
    public static string? _sptVersion = "0.0.0";
    public static string? _commit = "a12b34";
    public static double? _buildTime = 0000000000;
    public static EntryType? BuildType = EntryType.LOCAL;
}
