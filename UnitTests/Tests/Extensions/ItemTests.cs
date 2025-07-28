using NUnit.Framework;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Common;
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
}
