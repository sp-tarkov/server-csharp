using NUnit.Framework;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using UnitTests.Mock;

namespace UnitTests.Tests.Services;

[TestFixture]
public class ProfileHelperTests
{
    private ProfileHelper _sut;

    [SetUp]
    public void Setup()
    {
        _sut = new ProfileHelper(
            new MockLogger<ProfileHelper>(),
            new SPTarkov.Server.Core.Utils.Cloners.FastCloner(),
            DI.GetInstance().GetService<SaveServer>(),
            DI.GetInstance().GetService<DatabaseService>(),
            DI.GetInstance().GetService<Watermark>(),
            DI.GetInstance().GetService<TimeUtil>(),
            DI.GetInstance().GetService<ServerLocalisationService>(),
            DI.GetInstance().GetService<ConfigServer>()
        );
    }

    [Test]
    [TestCaseSource(nameof(GetAdjustSkillExpForLowLevelsTestData))]
    public void AdjustSkillExpForLowLevels(double startingProgress, double addedProgress, double expectedAdjustedProgress)
    {
        var result = _sut.AdjustSkillExpForLowLevels(startingProgress, addedProgress);
        Assert.AreEqual(expectedAdjustedProgress, result, 0.001);
    }

    private static IEnumerable<double[]> GetAdjustSkillExpForLowLevelsTestData()
    {
        // tests for levels 1-10 with +1.0 progress added
        yield return [0000, 01.0, 10.000]; //  1,  10.0 XP on UI, +1 => 1.0/ 10.0 = 10.000/100 internal
        yield return [0100, 01.0, 05.000]; //  2,  20.0 XP on UI, +1 => 1.0/ 20.0 =  5.000/100 internal
        yield return [0200, 01.0, 03.333]; //  3,  30.0 XP on UI, +1 => 1.0/ 30.0 =  3.333/100 internal
        yield return [0300, 01.0, 02.500]; //  4,  40.0 XP on UI, +1 => 1.0/ 40.0 =  2.500/100 internal
        yield return [0400, 01.0, 02.000]; //  5,  50.0 XP on UI, +1 => 1.0/ 50.0 =  2.000/100 internal
        yield return [0500, 01.0, 01.667]; //  6,  60.0 XP on UI, +1 => 1.0/ 60.0 =  1.667/100 internal
        yield return [0600, 01.0, 01.428]; //  7,  70.0 XP on UI, +1 => 1.0/ 70.0 =  1.428/100 internal
        yield return [0700, 01.0, 01.250]; //  8,  80.0 XP on UI, +1 => 1.0/ 80.0 =  1.250/100 internal
        yield return [0800, 01.0, 01.111]; //  9,  90.0 XP on UI, +1 => 1.0/ 90.0 =  1.111/100 internal
        yield return [0900, 01.0, 01.000]; // 10, 100.0 XP on UI, +1 => 1.0/100.0 =  1.000/100 internal => no scaling
        yield return [1000, 01.0, 01.000]; // 11, 100.0 XP on UI, +1 => 1.0/100.0 =  1.000/100 internal => no scaling
        // level boundary tests for partial progress with +4.0 progress added
        yield return [0098, 04.0, 21.000]; //  1-> 2,   98 =  9.8/ 10, +4.0 =  9.8-> 10 (+2, -0.2), remaining +3.8 (3.8/ 20) = 19.000/100 => 21.000 total
        yield return [0198, 04.0, 14.000]; //  2-> 3,  198 = 19.6/ 20, +4.0 = 19.6-> 20 (+2, -0.4), remaining +3.6 (3.6/ 30) = 12.000/100 => 14.000 total
        yield return [0298, 04.0, 10.500]; //  3-> 4,  298 = 29.4/ 30, +4.0 = 29.4-> 30 (+2, -0.6), remaining +3.4 (3.4/ 40) =  8.500/100 => 10.500 total
        yield return [0398, 04.0, 08.400]; //  4-> 5,  398 = 39.2/ 40, +4.0 = 39.2-> 40 (+2, -0.8), remaining +3.2 (3.2/ 50) =  6.400/100 =>  8.400 total
        yield return [0498, 04.0, 07.000]; //  5-> 6,  498 = 49.0/ 50, +4.0 = 49.0-> 50 (+2, -1.0), remaining +3.0 (3.0/ 60) =  5.000/100 =>  7.000 total
        yield return [0598, 04.0, 06.000]; //  6-> 7,  598 = 58.8/ 60, +4.0 = 58.8-> 60 (+2, -1.2), remaining +2.8 (2.8/ 70) =  4.000/100 =>  6.000 total
        yield return [0698, 04.0, 05.250]; //  7-> 8,  698 = 68.6/ 70, +4.0 = 68.6-> 70 (+2, -1.4), remaining +2.6 (2.6/ 80) =  3.250/100 =>  5.250 total
        yield return [0798, 04.0, 04.667]; //  8-> 9,  798 = 78.4/ 80, +4.0 = 78.4-> 80 (+2, -1.6), remaining +2.4 (2.4/ 90) =  2.667/100 =>  4.667 total
        yield return [0898, 04.0, 04.200]; //  9->10,  898 = 88.2/ 90, +4.0 = 88.2-> 90 (+2, -1.8), remaining +2.2 (2.2/100) =  2.200/100 =>  4.200 total
        yield return [0998, 04.0, 04.000]; // 10->11,  998 = 98.0/100, +4.0 = 98.0->100 (+2, -2.0), remaining +2.0 (2.0/100) =  2.000/100 =>  4.000 total
        yield return [1098, 04.0, 04.000]; // 11->12, 1098 = 98.0/100, +4.0 = 98.0->100 (+2, -2.0), remaining +2.0 (2.0/100) =  2.000/100 =>  4.000 total
        // test multi level boundary jumps
        yield return [050, 30.0, 166.667]; //  1-> 3,   50 =  5.0/ 10, +3.0 =  5.0-> 10 (+50, -5.0), remaining +25: 0.0->20 (+100, -20), remaining +5 (5.0/30) = 16.667/100 => 166.667 total
    }
}
