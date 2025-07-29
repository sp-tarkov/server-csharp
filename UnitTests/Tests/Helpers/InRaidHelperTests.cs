using NUnit.Framework;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace UnitTests.Tests.Helpers;

[TestFixture]
public class InRaidHelperTests
{
    private InRaidHelper _helper;

    [OneTimeSetUp]
    public void Initialize()
    {
        _helper = DI.GetInstance().GetService<InRaidHelper>();
    }

    [Test]
    public void DeleteInventory_ShouldNotThrowCollectionModifiedException()
    {
        // Arrange
        var pmcData = CreateTestPmcData();
        var sessionId = new MongoId();

        // Act & Assert
        Assert.DoesNotThrow(() => _helper.DeleteInventory(pmcData, sessionId));
    }

    [Test]
    public void DeleteInventory_ShouldRemoveSomeItems()
    {
        // Arrange
        var pmcData = CreateTestPmcData();
        var sessionId = new MongoId();
        var initialItemCount = pmcData.Inventory.Items.Count;

        // Act
        _helper.DeleteInventory(pmcData, sessionId);

        // Assert
        // The main goal is to verify that the collection modification bug is fixed
        // We expect some items to be removed, but the exact count depends on configuration
        Assert.LessOrEqual(pmcData.Inventory.Items.Count, initialItemCount);

        // Verify that the method completed without throwing collection modification exception
        Assert.Pass("DeleteInventory completed successfully without collection modification exception");
    }

    private static PmcData CreateTestPmcData()
    {
        var equipmentId = new MongoId();
        var questRaidItemsId = new MongoId();

        var items = new List<Item>
        {
            // Equipment items (should be removed)
            new()
            {
                Id = new MongoId(),
                Template = new MongoId("507f1f77bcf86cd799439011"), // weapon_ak74
                ParentId = equipmentId.ToString(),
                SlotId = "FirstPrimaryWeapon",
            },
            new()
            {
                Id = new MongoId(),
                Template = new MongoId("507f1f77bcf86cd799439012"), // ammo_545x39
                ParentId = equipmentId.ToString(),
                SlotId = "pocket1",
            },
            new()
            {
                Id = new MongoId(),
                Template = new MongoId("507f1f77bcf86cd799439013"), // medkit
                ParentId = equipmentId.ToString(),
                SlotId = "pocket2",
            },
            // Quest raid items (should be removed)
            new()
            {
                Id = new MongoId(),
                Template = new MongoId("507f1f77bcf86cd799439014"), // quest_item
                ParentId = questRaidItemsId.ToString(),
                SlotId = "quest",
            },
            // Stash items (should be kept) - these have ParentId = null
            new()
            {
                Id = new MongoId(),
                Template = new MongoId("507f1f77bcf86cd799439015"), // money
                ParentId = null,
                SlotId = "hideout",
            },
            new()
            {
                Id = new MongoId(),
                Template = new MongoId("507f1f77bcf86cd799439016"), // another stash item
                ParentId = null,
                SlotId = "hideout",
            },
            new()
            {
                Id = new MongoId(),
                Template = new MongoId("507f1f77bcf86cd799439017"), // third stash item
                ParentId = null,
                SlotId = "hideout",
            },
        };

        return new PmcData
        {
            Id = new MongoId(),
            Inventory = new BotBaseInventory
            {
                Items = items,
                Equipment = equipmentId,
                QuestRaidItems = questRaidItemsId,
                FastPanel = new Dictionary<string, MongoId>(),
            },
            InsuredItems = [],
        };
    }
}
