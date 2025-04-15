using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Utils;

public static partial class ProgramStatics
{
    private static bool _debug;
    private static bool _compiled;
    private static bool _mods;

    public static void Initialize()
    {
        var _entryType = ProgramStatics.BuildType ?? EntryType.LOCAL;

        Console.WriteLine($"Entry type: {_entryType}");
        Console.WriteLine($"SPT Version: {_sptVersion}");
        Console.WriteLine($"Commit: {_commit}");
        Console.WriteLine($"Build time: {_buildTime}");

        switch (_entryType)
        {
            case EntryType.RELEASE:
                _debug = false;
                _compiled = true;
                _mods = true;
                break;
            case EntryType.BLEEDING_EDGE:
                _debug = true;
                _compiled = true;
                _mods = false;
                break;
            case EntryType.DEBUG:
            case EntryType.BLEEDING_EDGE_MODS:
                _debug = true;
                _compiled = true;
                _mods = true;
                break;
            case EntryType.LOCAL:
            default:
                _debug = true;
                _compiled = false;
                _mods = true;
                break;
        }
    }

    // Public Static Getters
    public static EntryType? ENTRY_TYPE()
    {
        return BuildType;
    }

    public static bool DEBUG()
    {
        return _debug;
    }

    public static bool COMPILED()
    {
        return _compiled;
    }

    public static bool MODS()
    {
        return _mods;
    }

    public static string? SPT_VERSION()
    {
        return _sptVersion;
    }

    public static string? COMMIT()
    {
        return _commit;
    }

    public static double? BUILD_TIME()
    {
        return _buildTime;
    }
}
