using NUnit.Framework;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace UnitTests.Tests.Extensions;

[TestFixture]
public class ItemTests
{
    [SetUp]
    public void Initialize() { }

    [Test]
    public void GetItemWithChildren_one_child_mods_only()
    {
        var testData = new List<Item>();
        var rootItem = new Item { Id = new MongoId(), Template = ItemTpl.AMMOBOX_127X33_COPPER_20RND };
        var childItem = new Item
        {
            Id = new MongoId(),
            Template = ItemTpl.AMMO_127X33_COPPER,
            ParentId = rootItem.Id,
        };
        testData.Add(rootItem);
        testData.Add(childItem);

        var result = testData.GetItemWithChildren(rootItem.Id, true);

        Assert.AreEqual(result[1].Id, childItem.Id);
    }

    [Test]
    public void GetItemWithChildren_mods_only_one_inventory_item()
    {
        var testData = new List<Item>();
        var rootItem = new Item { Id = new MongoId(), Template = ItemTpl.AMMOBOX_127X33_COPPER_20RND };
        var childItem = new Item
        {
            Id = new MongoId(),
            Template = ItemTpl.AMMO_127X33_COPPER,
            ParentId = rootItem.Id,
            Location = 1,
        };
        var childItem2 = new Item
        {
            Id = new MongoId(),
            Template = ItemTpl.AMMO_26X75_GREEN,
            ParentId = rootItem.Id,
        };
        testData.Add(rootItem);
        testData.Add(childItem);
        testData.Add(childItem2);

        var result = testData.GetItemWithChildren(rootItem.Id, true);

        Assert.AreEqual(result[1].Id, childItem2.Id);
        Assert.AreEqual(result.Count, 2);
    }

    [Test]
    public void GetItemWithChildren_mods_and_inventory_item()
    {
        var testData = new List<Item>();
        var rootItem = new Item { Id = new MongoId(), Template = ItemTpl.AMMOBOX_127X33_COPPER_20RND };
        var childItem = new Item
        {
            Id = new MongoId(),
            Template = ItemTpl.AMMO_127X33_COPPER,
            ParentId = rootItem.Id,
            Location = 1,
        };
        var childItem2 = new Item
        {
            Id = new MongoId(),
            Template = ItemTpl.AMMO_26X75_GREEN,
            ParentId = rootItem.Id,
        };
        testData.Add(rootItem);
        testData.Add(childItem);
        testData.Add(childItem2);

        var result = testData.GetItemWithChildren(rootItem.Id, false);

        Assert.AreEqual(result[1].Id, childItem.Id);
        Assert.AreEqual(result.Count, 3);
    }

    [Test]
    public void GetItemWithChildren_mod_with_child()
    {
        var testData = new List<Item>();
        var rootItem = new Item { Id = new MongoId(), Template = ItemTpl.AMMOBOX_127X33_COPPER_20RND };
        var childItem = new Item
        {
            Id = new MongoId(),
            Template = ItemTpl.AMMO_127X33_COPPER,
            ParentId = rootItem.Id,
        };
        var childOfChild = new Item
        {
            Id = new MongoId(),
            Template = ItemTpl.AMMO_26X75_GREEN,
            ParentId = childItem.Id,
        };
        testData.Add(rootItem);
        testData.Add(childItem);
        testData.Add(childOfChild);

        var result = testData.GetItemWithChildren(rootItem.Id, true);

        Assert.AreEqual(result[1].Id, childItem.Id);
        Assert.AreEqual(result.Count, 3);
    }

    [Test]
    public void GetItemWithChildren_no_matching_children()
    {
        var testData = new List<Item>();
        var rootItem = new Item { Id = new MongoId(), Template = ItemTpl.AMMOBOX_127X33_COPPER_20RND };
        var childItem = new Item
        {
            Id = new MongoId(),
            Template = ItemTpl.AMMO_127X33_COPPER,
            ParentId = new MongoId(),
        };
        var childOfChild = new Item
        {
            Id = new MongoId(),
            Template = ItemTpl.AMMO_26X75_GREEN,
            ParentId = childItem.Id,
        };
        testData.Add(rootItem);
        testData.Add(childItem);
        testData.Add(childOfChild);

        var result = testData.GetItemWithChildren(rootItem.Id, true);

        Assert.AreEqual(result[0].Id, rootItem.Id);
        Assert.AreEqual(result.Count, 1);
    }

    [Test]
    public void RemoveFiRStatusFromItemsInContainer_twoItemsInBackpack()
    {
        var profile = new PmcData() { Inventory = new BotBaseInventory() { Items = [] } };
        profile.Inventory.Equipment = new MongoId();

        // Add backpack
        var backpackId = new MongoId();
        profile.Inventory.Items.Add(
            new Item
            {
                Id = backpackId,
                Template = ItemTpl.BACKPACK_HAZARD_4_PILLBOX_BACKPACK_BLACK,
                ParentId = profile.Inventory.Equipment,
                SlotId = "Backpack",
            }
        );

        // Add ifak to first slot in backpack
        var item1Id = new MongoId();
        profile.Inventory.Items.Add(
            new Item
            {
                Id = item1Id,
                Template = ItemTpl.MEDKIT_AFAK_TACTICAL_INDIVIDUAL_FIRST_AID_KIT,
                ParentId = backpackId,
                SlotId = "main",
                Upd = new Upd { SpawnedInSession = true },
                Location = new ItemLocation
                {
                    X = 0,
                    Y = 0,
                    R = ItemRotation.Horizontal,
                },
            }
        );

        // Add wrench to first slot of ifak
        var item2Id = new MongoId();
        profile.Inventory.Items.Add(
            new Item
            {
                Id = item2Id,
                Template = ItemTpl.BARTER_WRENCH,
                ParentId = backpackId,
                SlotId = "main",
                Upd = new Upd { SpawnedInSession = true },
                Location = new ItemLocation
                {
                    X = 1,
                    Y = 0,
                    R = ItemRotation.Horizontal,
                },
            }
        );

        // Add armband to armband slot as root
        var item3Id = new MongoId();
        profile.Inventory.Items.Add(
            new Item
            {
                Id = item3Id,
                Template = ItemTpl.ARMBAND_RED,
                ParentId = profile.Inventory.Equipment,
                SlotId = "Armband",
                Upd = new Upd { SpawnedInSession = true },
            }
        );

        profile.RemoveFiRStatusFromItemsInContainer("Backpack");

        Assert.AreEqual(false, profile.Inventory.Items.FirstOrDefault(item => item.Id == item1Id).Upd.SpawnedInSession);
        Assert.AreEqual(false, profile.Inventory.Items.FirstOrDefault(item => item.Id == item2Id).Upd.SpawnedInSession);
        Assert.AreEqual(true, profile.Inventory.Items.FirstOrDefault(item => item.Id == item3Id).Upd.SpawnedInSession);
    }

    [Test]
    public void RemoveFiRStatusFromItemsInContainer_oneItemWithChildInBackpack()
    {
        var profile = new PmcData { Inventory = new BotBaseInventory { Items = [] } };
        profile.Inventory.Equipment = new MongoId();

        // Add backpack
        var backpackId = new MongoId();
        profile.Inventory.Items.Add(
            new Item
            {
                Id = backpackId,
                Template = ItemTpl.BACKPACK_HAZARD_4_PILLBOX_BACKPACK_BLACK,
                ParentId = profile.Inventory.Equipment,
                SlotId = "Backpack",
            }
        );

        // Add ifak to first slot in backpack
        var item1Id = new MongoId();
        profile.Inventory.Items.Add(
            new Item
            {
                Id = item1Id,
                Template = ItemTpl.MEDKIT_AFAK_TACTICAL_INDIVIDUAL_FIRST_AID_KIT,
                ParentId = backpackId,
                SlotId = "main",
                Upd = new Upd { SpawnedInSession = true },
                Location = new ItemLocation
                {
                    X = 0,
                    Y = 0,
                    R = ItemRotation.Horizontal,
                },
            }
        );

        // Add wrench to first slot of ifak as child
        var item2Id = new MongoId();
        profile.Inventory.Items.Add(
            new Item
            {
                Id = item2Id,
                Template = ItemTpl.BARTER_WRENCH,
                ParentId = item1Id,
                SlotId = "main",
                Upd = new Upd { SpawnedInSession = true },
                Location = new ItemLocation
                {
                    X = 1,
                    Y = 0,
                    R = ItemRotation.Horizontal,
                },
            }
        );

        // Add armband to armband slot as root
        var item3Id = new MongoId();
        profile.Inventory.Items.Add(
            new Item
            {
                Id = item3Id,
                Template = ItemTpl.ARMBAND_RED,
                ParentId = profile.Inventory.Equipment,
                SlotId = "Armband",
                Upd = new Upd { SpawnedInSession = true },
            }
        );

        profile.RemoveFiRStatusFromItemsInContainer("Backpack");

        Assert.AreEqual(false, profile.Inventory.Items.FirstOrDefault(item => item.Id == item1Id).Upd.SpawnedInSession);
        Assert.AreEqual(false, profile.Inventory.Items.FirstOrDefault(item => item.Id == item2Id).Upd.SpawnedInSession);
        Assert.AreEqual(true, profile.Inventory.Items.FirstOrDefault(item => item.Id == item3Id).Upd.SpawnedInSession);
    }
}
