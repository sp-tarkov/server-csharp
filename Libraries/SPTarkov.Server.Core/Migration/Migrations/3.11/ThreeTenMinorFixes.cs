using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Profile;

namespace SPTarkov.Server.Core.Migration.Migrations
{
    /// <summary>
    /// In the minor versions of 3.10 or somewhere in between these properties were added, it's possible that a profile has not updated
    /// To these thus never having received them, re-add them here.
    /// </summary>
    [Injectable]
    public class ThreeTenMinorFixes : AbstractProfileMigration
    {
        public override string FromVersion
        {
            get { return "~3.10"; }
        }

        public override string ToVersion
        {
            get { return "3.11"; }
        }

        public override string MigrationName
        {
            get { return "ThreeTenMinorFixes-SPTSharp"; }
        }

        public override IEnumerable<Type> PrerequisiteMigrations
        {
            get { return [typeof(HideoutSeed)]; }
        }

        public override bool CanMigrate(JsonObject profile, IEnumerable<IProfileMigration> previouslyRanMigrations)
        {
            var cultistRewardsMissing = profile["spt"]?["cultistRewards"] == null;
            var friendProfileIdsMissing = profile["friends"] == null;

            return cultistRewardsMissing || friendProfileIdsMissing;
        }

        public override bool PostMigrate(SptProfile profile)
        {
            if (profile.SptData!.CultistRewards == null)
            {
                profile.SptData.CultistRewards = [];
            }

            profile.FriendProfileIds ??= [];

            return base.PostMigrate(profile);
        }
    }
}
