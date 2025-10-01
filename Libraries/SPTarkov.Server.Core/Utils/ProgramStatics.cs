using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Logging;
using Version = SemanticVersioning.Version;

namespace SPTarkov.Server.Core.Utils;

public static partial class ProgramStatics
{
    private static bool _debug;
    private static bool _compiled;
    private static bool _mods;

    public static void Initialize()
    {
        switch (BuildType)
        {
            case EntryType.RELEASE:
                _debug = false;
                _compiled = true;
                _mods = true;
                break;
            case EntryType.BLEEDINGEDGE:
                _debug = true;
                _compiled = true;
                _mods = false;
                break;
            case EntryType.DEBUG:
            case EntryType.BLEEDINGEDGEMODS:
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

#if DEBUG
        Console.WriteLine($"SPTarkov.Server.Core: entrytype: {BuildType}");
        Console.WriteLine($"SPTarkov.Server.Core: debug: {_debug}");
        Console.WriteLine($"SPTarkov.Server.Core: compiled: {_compiled}");
        Console.WriteLine($"SPTarkov.Server.Core: mods: {_mods}");
#endif
    }

    // Public Static Getters

    /// <summary>
    /// What type of release is this
    /// </summary>
    /// <returns></returns>
    public static EntryType ENTRY_TYPE()
    {
        return BuildType;
    }

    /// <summary>
    /// Server is running in debug mode
    /// </summary>
    /// <returns></returns>
    public static bool DEBUG()
    {
        return _debug;
    }

    public static bool COMPILED()
    {
        return _compiled;
    }

    /// <summary>
    /// Are mods enable for the server
    /// </summary>
    /// <returns></returns>
    public static bool MODS()
    {
        return _mods;
    }

    public static Version SPT_VERSION()
    {
        return SptVersion;
    }

    public static string COMMIT()
    {
        return Commit;
    }

    /// <summary>
    /// Timestamp of server build date
    /// </summary>
    /// <returns></returns>
    public static double BUILD_TIME()
    {
        return BuildTime;
    }

    public static LogTextColor BUILD_TEXT_COLOR()
    {
        return BuildType switch
        {
            EntryType.RELEASE => LogTextColor.Yellow,
            EntryType.LOCAL or EntryType.DEBUG => LogTextColor.Cyan,
            EntryType.BLEEDINGEDGE or EntryType.BLEEDINGEDGEMODS => LogTextColor.Magenta,
            _ => LogTextColor.Yellow,
        };
    }
}
