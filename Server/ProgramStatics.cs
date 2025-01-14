namespace Server
{
    public static class ProgramStatics
    {
        private static EntryType _entryType;

        private static bool _debug;
        private static bool _compiled;
        private static bool _mods;

        private static string _sptVersion;
        private static string _commit;
        private static double _buildTime;

        private static BuildInfo buildInfo; // TODO get from buildinfo.json

        public static void Initialize()
        {
            ProgramStatics._entryType = buildInfo.entryType.Value;


            ProgramStatics._sptVersion = buildInfo.sptVersion ?? "";
            ProgramStatics._commit = buildInfo.commit ?? "";
            ProgramStatics._buildTime = buildInfo.buildTime ?? 0;

            switch (ProgramStatics._entryType)
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
            return ProgramStatics._debug;
        }
        public static bool COMPILED()
        {
            return ProgramStatics._compiled;
        }
        public static bool MODS()
        {
            return ProgramStatics._mods;
        }
        public static string SPT_VERSION()
        {
            return ProgramStatics._sptVersion;
        }
        public static string COMMIT()
        {
            return ProgramStatics._commit;
        }
        public static double BUILD_TIME()
        {
            return ProgramStatics._buildTime;
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
        public EntryType? entryType { get; set; }
        public string? sptVersion { get; set; }
        public string? commit { get; set; }
        public double? buildTime { get; set; }
    }
}
