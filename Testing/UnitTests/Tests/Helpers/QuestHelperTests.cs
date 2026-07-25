using NUnit.Framework;
using SPTarkov.Server.Core.Helpers.Quest;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Servers;

namespace UnitTests.Tests.Helpers;

[TestFixture]
public class QuestHelperTests
{
    private const int GoodTimesLevelRequirement = 27;

    private QuestHelper _questHelper = default!;
    private SaveServer _saveServer = default!;

    [SetUp]
    public void Setup()
    {
        _questHelper = DI.GetInstance().GetService<QuestHelper>();
        _saveServer = DI.GetInstance().GetService<SaveServer>();
    }

    [Test]
    public void GetClientQuests_StandardProfile_DoesNotShowEodWhitelistedQuest()
    {
        var sessionId = AddProfileFulfillingGoodTimes(GameEditions.STANDARD);

        var quests = _questHelper.GetClientQuests(sessionId);

        Assert.That(
            quests.Any(q => q.Id == QuestTpl.THE_GOOD_TIMES_PART_1),
            Is.False,
            "Standard profile must not see an EOD-whitelisted quest"
        );
    }

    [Test]
    public void GetClientQuests_EodProfile_ShowsEodWhitelistedQuest()
    {
        var sessionId = AddProfileFulfillingGoodTimes(GameEditions.EDGE_OF_DARKNESS);

        var quests = _questHelper.GetClientQuests(sessionId);

        Assert.That(
            quests.Any(q => q.Id == QuestTpl.THE_GOOD_TIMES_PART_1),
            Is.True,
            "EOD profile should see the whitelisted quest once prerequisites are met"
        );
    }

    private MongoId AddProfileFulfillingGoodTimes(string gameVersion)
    {
        var sessionId = new MongoId();

        var pmc = new PmcData
        {
            Info = new SPTarkov.Server.Core.Models.Eft.Common.Tables.Info
            {
                Side = "Usec",
                Level = GoodTimesLevelRequirement,
                GameVersion = gameVersion,
            },
            Quests =
            [
                new QuestStatus
                {
                    QId = QuestTpl.SHOOTING_CANS,
                    StartTime = 0,
                    Status = QuestStatusEnum.Success,
                    StatusTimers = new Dictionary<QuestStatusEnum, double> { { QuestStatusEnum.Success, 0 } },
                },
            ],
            TradersInfo = new Dictionary<MongoId, TraderInfo>
            {
                {
                    Traders.PRAPOR,
                    new TraderInfo
                    {
                        LoyaltyLevel = 4,
                        Standing = 999,
                        Unlocked = true,
                    }
                },
            },
        };

        var profile = new SptProfile
        {
            ProfileInfo = new SPTarkov.Server.Core.Models.Eft.Profile.Info { ProfileId = sessionId },
            CharacterData = new Characters { PmcData = pmc },
        };

        _saveServer.AddProfile(profile);

        return sessionId;
    }
}
