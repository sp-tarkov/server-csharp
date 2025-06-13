using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Models.Spt.Location;

namespace SPTarkov.Server.Core.Models.Spt.Services
{
    public class ProfileActivityData
    {
        public long ClientStartedTimestamp { get; set; }
        public long LastActive { get; set; }
        public ProfileActivityRaidData? RaidData { get; set; } = null;
    }

    public class ProfileActivityRaidData
    {
        public GetRaidConfigurationRequestData? RaidConfiguration { get; set; } = null;
        public RaidChanges? RaidAdjustments { get; set; } = null;
        public LocationTransit? LocationTransit { get; set; } = null;
    }

}
