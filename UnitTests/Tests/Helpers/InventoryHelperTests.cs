using NUnit.Framework;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace UnitTests.Tests.Helpers;

[TestFixture]
public class InventoryHelperTests
{
    private InventoryHelper _helper;
    private PresetHelper _presetHelper;

    [OneTimeSetUp]
    public void Initialize()
    {
        _helper = DI.GetInstance().GetService<InventoryHelper>();
        _presetHelper = DI.GetInstance().GetService<PresetHelper>();
    }

    [Test]
    public void GetItemSize_vss_val()
    {
        var vssValPreset = _presetHelper.GetDefaultPreset(ItemTpl.MARKSMANRIFLE_VSS_VINTOREZ_9X39_SPECIAL_SNIPER_RIFLE);

        var result = _helper.GetItemSize(
            ItemTpl.MARKSMANRIFLE_VSS_VINTOREZ_9X39_SPECIAL_SNIPER_RIFLE,
            vssValPreset.Parent,
            vssValPreset.Items
        );

        Assert.AreEqual(5, result.Item1);
        Assert.AreEqual(2, result.Item2);
    }

    [Test]
    public void GetItemSize_m4a1()
    {
        var vssValPreset = _presetHelper.GetDefaultPreset(ItemTpl.ASSAULTRIFLE_COLT_M4A1_556X45_ASSAULT_RIFLE);

        var result = _helper.GetItemSize(ItemTpl.ASSAULTRIFLE_COLT_M4A1_556X45_ASSAULT_RIFLE, vssValPreset.Parent, vssValPreset.Items);

        Assert.AreEqual(5, result.Item1);
        Assert.AreEqual(2, result.Item2);
    }

    [Test]
    public void GetItemSize_glock_17()
    {
        var vssValPreset = _presetHelper.GetDefaultPreset(ItemTpl.PISTOL_GLOCK_17_9X19);

        var result = _helper.GetItemSize(ItemTpl.PISTOL_GLOCK_17_9X19, vssValPreset.Parent, vssValPreset.Items);

        Assert.AreEqual(2, result.Item1);
        Assert.AreEqual(1, result.Item2);
    }

    [Test]
    public void GetItemSize_custom_vpo_136_6x2()
    {
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

        var result = _helper.GetItemSize(root.Template, rootWeaponId, weaponWithChildren);

        Assert.AreEqual(6, result.Item1);
        Assert.AreEqual(2, result.Item2);
    }

    [Test]
    public void GetItemSize_uzi_unfolded()
    {
        var rootWeaponId = new MongoId();

        var weaponWithChildren = new List<Item>();
        var root = new Item { Id = rootWeaponId, Template = ItemTpl.SMG_IWI_UZI_9X19_SUBMACHINE_GUN };
        weaponWithChildren.Add(root);

        var stock = new Item
        {
            Id = new MongoId(),
            Template = "6699249f3c4fda6471005cba",
            ParentId = root.Id,
            SlotId = "mod_stock",
        };
        weaponWithChildren.Add(stock);

        var magazine = new Item
        {
            Id = new MongoId(),
            Template = "669927203c4fda6471005cbe",
            ParentId = root.Id,
            SlotId = "mod_magazine",
        };
        weaponWithChildren.Add(magazine);

        var result = _helper.GetItemSize(ItemTpl.SMG_IWI_UZI_9X19_SUBMACHINE_GUN, rootWeaponId, weaponWithChildren);

        Assert.AreEqual(3, result.Item1);
        Assert.AreEqual(2, result.Item2);
    }

    [Test]
    public void GetItemSize_uzi_folded()
    {
        var rootWeaponId = new MongoId();

        var weaponWithChildren = new List<Item>();
        var root = new Item { Id = rootWeaponId, Template = ItemTpl.SMG_IWI_UZI_9X19_SUBMACHINE_GUN };
        weaponWithChildren.Add(root);

        var stock = new Item
        {
            Id = new MongoId(),
            Template = "6699249f3c4fda6471005cba",
            ParentId = root.Id,
            SlotId = "mod_stock",
            Upd = new Upd { Foldable = new UpdFoldable { Folded = true } },
        };
        weaponWithChildren.Add(stock);

        var magazine = new Item
        {
            Id = new MongoId(),
            Template = "669927203c4fda6471005cbe",
            ParentId = root.Id,
            SlotId = "mod_magazine",
        };
        weaponWithChildren.Add(magazine);

        var result = _helper.GetItemSize(ItemTpl.SMG_IWI_UZI_9X19_SUBMACHINE_GUN, rootWeaponId, weaponWithChildren);

        Assert.AreEqual(2, result.Item1);
        Assert.AreEqual(2, result.Item2);
    }
}
