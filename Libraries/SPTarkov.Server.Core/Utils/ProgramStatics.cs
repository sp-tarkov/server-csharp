using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Utils;

public static partial class ProgramStatics
{
    private static bool _debug;
    private static bool _compiled;
    private static bool _mods;

    public static void Initialize()
    {
        var _entryType = BuildType ?? EntryType.LOCAL;

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
#if DEBUG
                _debug = true;
#endif
                _compiled = false;
                _mods = true;
                break;
        }

        Console.WriteLine($"SPTarkov.Server.Core: entrytype: {_entryType}");
        Console.WriteLine($"SPTarkov.Server.Core: debug: {_debug}");
        Console.WriteLine($"SPTarkov.Server.Core: compiled: {_compiled}");
        Console.WriteLine($"SPTarkov.Server.Core: mods: {_mods}");
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
