using NUnit.Framework;
using SPTarkov.Server.Core.Generators;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;

namespace UnitTests.Tests.Helpers;

[TestFixture]
public class BotGeneratorHelperTests
{
    private BotGeneratorHelper _botGeneratorHelper;
    private BotLootGenerator _botLootGenerator;

    [OneTimeSetUp]
    public void Initialize()
    {
        _botGeneratorHelper = DI.GetInstance().GetService<BotGeneratorHelper>();
        _botLootGenerator = DI.GetInstance().GetService<BotLootGenerator>();
    }

    #region AddItemWithChildrenToEquipmentSlot

    [Test]
    public void AddItemWithChildrenToEquipmentSlot_fit_vertical()
    {
        var stashId = new MongoId();
        var equipmentId = new MongoId();
        var botInventory = new BotBaseInventory
        {
            Items = [],
            Stash = stashId,
            Equipment = equipmentId,
        };

        // Create backpack on player
        var backpack = new Item
        {
            Id = new MongoId(),
            // Has a 3grids, first is a 3hx5v grid
            Template = ItemTpl.BACKPACK_EBERLESTOCK_G2_GUNSLINGER_II_BACKPACK_DRY_EARTH,
            ParentId = equipmentId,
            SlotId = "Backpack",
        };
        botInventory.Items.Add(backpack);

        var rootWeaponId = new MongoId();
        var weaponWithChildren = CreateMp18(rootWeaponId);

        var result = _botGeneratorHelper.AddItemWithChildrenToEquipmentSlot(
            [EquipmentSlots.Backpack],
            rootWeaponId,
            ItemTpl.SHOTGUN_MP18_762X54R_SINGLESHOT_RIFLE,
            weaponWithChildren,
            botInventory
        );

        Assert.AreEqual(ItemAddedResult.SUCCESS, result);

        var weaponRoot = weaponWithChildren.FirstOrDefault(item => item.Id == rootWeaponId);
        Assert.AreEqual((weaponRoot.Location as ItemLocation).X, 0);
        Assert.AreEqual((weaponRoot.Location as ItemLocation).Y, 0);
        Assert.AreEqual((weaponRoot.Location as ItemLocation).R, ItemRotation.Vertical);
    }

    private static List<Item> CreateMp18(MongoId rootWeaponId)
    {
        var weaponWithChildren = new List<Item>();
        var weaponRoot = new Item { Id = rootWeaponId, Template = ItemTpl.SHOTGUN_MP18_762X54R_SINGLESHOT_RIFLE };
        weaponWithChildren.Add(weaponRoot);
        var weaponStock = new Item
        {
            Id = new MongoId(),
            Template = ItemTpl.STOCK_MP18_WOODEN,
            ParentId = weaponRoot.Id,
            SlotId = "mod_stock",
        };
        weaponWithChildren.Add(weaponStock);
        var weaponBarrel = new Item
        {
            Id = new MongoId(),
            Template = ItemTpl.BARREL_MP18_762X54R_600MM,
            ParentId = weaponRoot.Id,
            SlotId = "mod_barrel",
        };
        weaponWithChildren.Add(weaponBarrel);

        return weaponWithChildren;
    }

    [Test]
    public void AddItemWithChildrenToEquipmentSlot_fit_horizontal()
    {
        var stashId = new MongoId();
        var equipmentId = new MongoId();
        var botInventory = new BotBaseInventory
        {
            Items = [],
            Stash = stashId,
            Equipment = equipmentId,
        };

        // Create backpack on player
        var backpack = new Item
        {
            Id = new MongoId(),
            Template = ItemTpl.BACKPACK_ANA_TACTICAL_BETA_2_BATTLE_BACKPACK_OLIVE_DRAB,
            ParentId = equipmentId,
            SlotId = "Backpack",
        };
        botInventory.Items.Add(backpack);

        var rootWeaponId = new MongoId();
        var weaponWithChildren = CreateMp18(rootWeaponId);

        var result = _botGeneratorHelper.AddItemWithChildrenToEquipmentSlot(
            [EquipmentSlots.Backpack],
            rootWeaponId,
            ItemTpl.SHOTGUN_MP18_762X54R_SINGLESHOT_RIFLE,
            weaponWithChildren,
            botInventory
        );

        var tplsToAdd = new Dictionary<MongoId, double>
        {
            { ItemTpl.BARTER_MALBORO_CIGARETTES, 1 },
            { ItemTpl.FOREGRIP_SAKO_TRG_M10_GRIP_PAD, 1 },
            { ItemTpl.BARTER_GOLD_SKULL_RING, 1 },
            { ItemTpl.BARTER_PACK_OF_NAILS, 1 },
        };
        _botLootGenerator.AddLootFromPool(tplsToAdd, [EquipmentSlots.Backpack], 4, botInventory, "assault", null);

        Assert.AreEqual(ItemAddedResult.SUCCESS, result);

        var weaponRoot = weaponWithChildren.FirstOrDefault(item => item.Id == rootWeaponId);
        Assert.AreEqual((weaponRoot.Location as ItemLocation).X, 0);
        Assert.AreEqual((weaponRoot.Location as ItemLocation).Y, 0);
        Assert.AreEqual((weaponRoot.Location as ItemLocation).R, ItemRotation.Horizontal);
        foreach (var item in botInventory.Items.Where(i => tplsToAdd.ContainsKey(i.Template)))
        {
            var location = item.Location as ItemLocation;
            Assert.True(location.X >= 0 && location.X <= 3, "Error! An item was misplaced on the X axis inside the item grid!");
            Assert.AreEqual(1, location.Y, "Error! An item was misplaced on the Y axis inside the item grid!");
        }
    }

    /// <summary>
    /// Backpack with one bullet in top row, blocking gun from being placed at 0,0
    /// </summary>
    [Test]
    public void AddItemWithChildrenToEquipmentSlot_fit_vertical_with_items_in_backpack()
    {
        var botInventory = new BotBaseInventory { Items = [] };
        var backpack = new Item
        {
            Id = new MongoId(),
            // Has a 3hx5v grid first
            Template = ItemTpl.BACKPACK_EBERLESTOCK_G2_GUNSLINGER_II_BACKPACK_DRY_EARTH,
            SlotId = "Backpack",
        };
        botInventory.Items.Add(backpack);

        botInventory.Items.Add(
            new Item
            {
                Id = new MongoId(),
                Template = ItemTpl.AMMO_762X25TT_AKBS,
                ParentId = backpack.Id,
                SlotId = "main",
                Location = new ItemLocation
                {
                    X = 0,
                    Y = 0,
                    R = ItemRotation.Horizontal,
                },
                Upd = new Upd { StackObjectsCount = 1 },
            }
        );

        var rootWeaponId = new MongoId();
        var weaponWithChildren = CreateMp18(rootWeaponId);

        var result = _botGeneratorHelper.AddItemWithChildrenToEquipmentSlot(
            [EquipmentSlots.Backpack],
            rootWeaponId,
            ItemTpl.SHOTGUN_MP18_762X54R_SINGLESHOT_RIFLE,
            weaponWithChildren,
            botInventory
        );

        Assert.AreEqual(ItemAddedResult.SUCCESS, result);

        var weaponRoot = weaponWithChildren.FirstOrDefault(item => item.Id == rootWeaponId);
        Assert.AreEqual((weaponRoot.Location as ItemLocation).X, 1);
        Assert.AreEqual((weaponRoot.Location as ItemLocation).Y, 0);
        Assert.AreEqual((weaponRoot.Location as ItemLocation).R, ItemRotation.Vertical);
    }

    /// <summary>
    /// No space for gun
    /// </summary>
    [Test]
    public void AddItemWithChildrenToEquipmentSlot_no_space_in_first_grid_choose_second_grid()
    {
        var botInventory = new BotBaseInventory { Items = [] };
        var backpack = new Item
        {
            Id = new MongoId(),
            // Has a 3hx5v grid first
            Template = ItemTpl.BACKPACK_EBERLESTOCK_G2_GUNSLINGER_II_BACKPACK_DRY_EARTH,
            SlotId = "Backpack",
        };
        botInventory.Items.Add(backpack);

        botInventory.Items.AddRange(
            new Item
            {
                Id = new MongoId(),
                Template = ItemTpl.AMMO_762X25TT_AKBS,
                ParentId = backpack.Id,
                SlotId = "main",
                Location = new ItemLocation
                {
                    X = 0,
                    Y = 0,
                    R = ItemRotation.Horizontal,
                },
                Upd = new Upd { StackObjectsCount = 1 },
            },
            new Item
            {
                Id = new MongoId(),
                Template = ItemTpl.AMMO_762X25TT_AKBS,
                ParentId = backpack.Id,
                SlotId = "main",
                Location = new ItemLocation
                {
                    X = 1,
                    Y = 0,
                    R = ItemRotation.Horizontal,
                },
                Upd = new Upd { StackObjectsCount = 1 },
            },
            new Item
            {
                Id = new MongoId(),
                Template = ItemTpl.AMMO_762X25TT_AKBS,
                ParentId = backpack.Id,
                SlotId = "main",
                Location = new ItemLocation
                {
                    X = 2,
                    Y = 0,
                    R = ItemRotation.Horizontal,
                },
                Upd = new Upd { StackObjectsCount = 1 },
            }
        );

        var rootWeaponId = new MongoId();
        var weaponWithChildren = CreateMp18(rootWeaponId);

        var result = _botGeneratorHelper.AddItemWithChildrenToEquipmentSlot(
            [EquipmentSlots.Backpack],
            rootWeaponId,
            ItemTpl.SHOTGUN_MP18_762X54R_SINGLESHOT_RIFLE,
            weaponWithChildren,
            botInventory
        );

        Assert.AreEqual(ItemAddedResult.SUCCESS, result);
        var weaponRoot = weaponWithChildren.FirstOrDefault(item => item.Id == rootWeaponId);
        Assert.AreEqual("1", weaponRoot.SlotId);
        Assert.AreEqual((weaponRoot.Location as ItemLocation).X, 0);
        Assert.AreEqual((weaponRoot.Location as ItemLocation).Y, 0);
        Assert.AreEqual((weaponRoot.Location as ItemLocation).R, ItemRotation.Vertical);
    }

    /// <summary>
    /// No space for gun
    /// </summary>
    [Test]
    public void AddItemWithChildrenToEquipmentSlot_no_space()
    {
        var botInventory = new BotBaseInventory { Items = [] };
        var backpack = new Item
        {
            Id = new MongoId(),
            // Has a 4hx5v grid first
            Template = ItemTpl.BACKPACK_WARTECH_BERKUT_BB102_BACKPACK_ATACS_FG,
            SlotId = "Backpack",
        };
        botInventory.Items.Add(backpack);

        botInventory.Items.AddRange(
            new Item
            {
                Id = new MongoId(),
                Template = ItemTpl.AMMO_762X25TT_AKBS,
                ParentId = backpack.Id,
                SlotId = "main",
                Location = new ItemLocation
                {
                    X = 0,
                    Y = 0,
                    R = ItemRotation.Horizontal,
                },
                Upd = new Upd { StackObjectsCount = 1 },
            },
            new Item
            {
                Id = new MongoId(),
                Template = ItemTpl.AMMO_762X25TT_AKBS,
                ParentId = backpack.Id,
                SlotId = "main",
                Location = new ItemLocation
                {
                    X = 1,
                    Y = 0,
                    R = ItemRotation.Horizontal,
                },
                Upd = new Upd { StackObjectsCount = 1 },
            },
            new Item
            {
                Id = new MongoId(),
                Template = ItemTpl.AMMO_762X25TT_AKBS,
                ParentId = backpack.Id,
                SlotId = "main",
                Location = new ItemLocation
                {
                    X = 2,
                    Y = 0,
                    R = ItemRotation.Horizontal,
                },
                Upd = new Upd { StackObjectsCount = 1 },
            },
            new Item
            {
                Id = new MongoId(),
                Template = ItemTpl.AMMO_762X25TT_AKBS,
                ParentId = backpack.Id,
                SlotId = "main",
                Location = new ItemLocation
                {
                    X = 3,
                    Y = 0,
                    R = ItemRotation.Horizontal,
                },
                Upd = new Upd { StackObjectsCount = 1 },
            }
        );

        var rootWeaponId = new MongoId();
        var weaponWithChildren = CreateMp18(rootWeaponId);

        var result = _botGeneratorHelper.AddItemWithChildrenToEquipmentSlot(
            [EquipmentSlots.Backpack],
            rootWeaponId,
            ItemTpl.SHOTGUN_MP18_762X54R_SINGLESHOT_RIFLE,
            weaponWithChildren,
            botInventory
        );

        Assert.AreEqual(ItemAddedResult.NO_SPACE, result);
    }

    /// <summary>
    /// Fill all slots except for a 2x6 rectangle, with the top right corner filled, result should be no space
    /// </summary>
    [Test]
    public void AddItemWithChildrenToEquipmentSlot_custom_gun_no_space()
    {
        var botInventory = new BotBaseInventory { Items = [] };
        var backpack = new Item
        {
            Id = new MongoId(),
            // Has a 4hx5v grid first
            Template = ItemTpl.BACKPACK_GRUPPA_99_T30_BACKPACK_BLACK,
            SlotId = "Backpack",
        };
        botInventory.Items.Add(backpack);

        var takenSlots = new List<XY>
        {
            new() { X = 1, Y = 0 },
            new() { X = 2, Y = 0 },
            new() { X = 3, Y = 0 },
            new() { X = 4, Y = 0 },
            new() { X = 2, Y = 1 },
            new() { X = 3, Y = 1 },
            new() { X = 4, Y = 1 },
            new() { X = 2, Y = 2 },
            new() { X = 3, Y = 2 },
            new() { X = 4, Y = 2 },
            new() { X = 2, Y = 3 },
            new() { X = 3, Y = 3 },
            new() { X = 4, Y = 3 },
            new() { X = 2, Y = 4 },
            new() { X = 3, Y = 4 },
            new() { X = 4, Y = 4 },
            new() { X = 2, Y = 5 },
            new() { X = 3, Y = 5 },
            new() { X = 4, Y = 5 },
        };
        foreach (var takenSlot in takenSlots)
        {
            botInventory.Items.Add(
                new Item
                {
                    Id = new MongoId(),
                    Template = ItemTpl.AMMO_762X25TT_AKBS,
                    ParentId = backpack.Id,
                    SlotId = "main",
                    Location = new ItemLocation
                    {
                        X = (int)takenSlot.X.Value,
                        Y = (int)takenSlot.Y.Value,
                        R = ItemRotation.Horizontal,
                    },
                    Upd = new Upd { StackObjectsCount = 1 },
                }
            );
        }

        var rootWeaponId = new MongoId();
        var weaponWithChildren = new List<Item>();
        var root = new Item { Id = rootWeaponId, Template = ItemTpl.ASSAULTRIFLE_MOLOT_ARMS_VPO136_VEPRKM_762X39_CARBINE };
        weaponWithChildren.Add(root);

        var stock = new Item
        {
            Id = new MongoId(),
            Template = ItemTpl.STOCK_VPO136_VEPRKM_WOODEN,
            ParentId = root.Id,
            SlotId = "mod_stock",
        };
        weaponWithChildren.Add(stock);

        var magazine = new Item
        {
            Id = new MongoId(),
            Template = ItemTpl.MAGAZINE_366TKM_AK_AL_10RND,
            ParentId = root.Id,
            SlotId = "mod_magazine",
        };
        weaponWithChildren.Add(magazine);

        var muzzle = new Item
        {
            Id = new MongoId(),
            Template = ItemTpl.SILENCER_AKM_HEXAGON_762X39_SOUND_SUPPRESSOR,
            ParentId = root.Id,
            SlotId = "mod_muzzle",
        };
        weaponWithChildren.Add(muzzle);

        var result = _botGeneratorHelper.AddItemWithChildrenToEquipmentSlot(
            [EquipmentSlots.Backpack],
            rootWeaponId,
            root.Template,
            weaponWithChildren,
            botInventory
        );

        Assert.AreEqual(ItemAddedResult.NO_SPACE, result);
    }

    #endregion

    #region GetBotEquipmentRole

    [Test]
    public void GetBotEquipmentRole_assault()
    {
        var result = _botGeneratorHelper.GetBotEquipmentRole("assault");

        Assert.AreEqual("assault", result);
    }

    [Test]
    public void GetBotEquipmentRole_pmcBEAR()
    {
        var result = _botGeneratorHelper.GetBotEquipmentRole("pmcBEAR");

        Assert.AreEqual("pmc", result);
    }

    [Test]
    public void GetBotEquipmentRole_pmcBEAR_lowercase()
    {
        var result = _botGeneratorHelper.GetBotEquipmentRole("pmcbear");

        Assert.AreEqual("pmc", result);
    }

    #endregion
}
