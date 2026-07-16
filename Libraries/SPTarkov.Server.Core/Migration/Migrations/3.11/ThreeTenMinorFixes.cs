using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Eft.Profile;

namespace SPTarkov.Server.Core.Migration.Migrations._3._11;

/// <summary>
/// In the minor versions of 3.10 or somewhere in between these properties were added, it's possible that a profile has not updated
/// To these thus never having received them, re-add them here.
/// </summary>
[Injectable]
public sealed class ThreeTenMinorFixes : AbstractProfileMigration
{
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
        var cultistRewardsMissing = !profile.TryGetNode(out _, "spt", "cultistRewards");
        var friendProfileIdsMissing = !profile.TryGetNode(out _, "friends");

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
