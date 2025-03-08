namespace SPTarkov.Server.Core.Utils;

public static class ProgramStatics
{
    private static EntryType _entryType;

    private static bool _debug;
    private static bool _compiled;
    private static bool _mods;

    private static string? _sptVersion;
    private static string? _commit;
    private static double? _buildTime;

    private static BuildInfo buildInfo; // TODO get from buildinfo.json

    public static void Initialize()
    {
        buildInfo = new BuildInfo();
        _entryType = buildInfo.entryType.GetValueOrDefault(EntryType.LOCAL);

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
            default: // EntryType.LOCAL
                _debug = true;
                _compiled = false;
                _mods = true;
                break;
        }
    }

    // Public Static Getters
    public static EntryType ENTRY_TYPE()
    {
        return _entryType;
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

public enum EntryType
{
    LOCAL,
    DEBUG,
    RELEASE,
    BLEEDING_EDGE,
    BLEEDING_EDGE_MODS
}

public class BuildInfo
{
    public BuildInfo()
    {
        sptVersion = "";
        commit = "";
        buildTime = 0;
    }

    public EntryType? entryType
    {
        get;
        set;
    }

    public string? sptVersion
    {
        get;
        set;
    }

    public string? commit
    {
        get;
        set;
    }

    public double? buildTime
    {
        get;
        set;
    }
}
