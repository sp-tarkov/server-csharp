using NUnit.Framework;
using SPTarkov.Server.Core.Generators;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;

namespace UnitTests.Tests.Generators;

[TestFixture]
public class BotWeaponGeneratorTests
{
    private BotWeaponGenerator _botWeaponGenerator;
    private DatabaseService _databaseService;
    private InventoryHelper _inventoryHelper;
    private SaveServer _saveServer;

    [OneTimeSetUp]
    public void Initialize()
    {
        _botWeaponGenerator = DI.GetInstance().GetService<BotWeaponGenerator>();
        _databaseService = DI.GetInstance().GetService<DatabaseService>();
        _inventoryHelper = DI.GetInstance().GetService<InventoryHelper>();
        _saveServer = DI.GetInstance().GetService<SaveServer>();
    }

    [Test]
    public void GenerateWeaponByTpl_generate_m4_pmc()
    {
        var usecTemplate = _databaseService.GetBots().Types["usec"];
        var botTemplateInventory = usecTemplate.BotInventory;

        // Create profile stub to allow `GenerateWeaponByTpl` to work
        var sessionId = new MongoId();
        _saveServer.CreateProfile(new Info() { ProfileId = sessionId });

        var weaponTpl = ItemTpl.ASSAULTRIFLE_COLT_M4A1_556X45_ASSAULT_RIFLE;
        const string slotName = "FirstPrimaryWeapon";
        var weaponModChances = usecTemplate.BotChances.WeaponModsChances;
        foreach (var (key, _) in weaponModChances)
        {
            // Set all mods to 100%
            weaponModChances[key] = 100d;
        }

        var weaponParentId = new MongoId();

        for (var i = 0; i < 100; i++)
        {
            var result = _botWeaponGenerator.GenerateWeaponByTpl(
                sessionId,
                weaponTpl,
                slotName,
                botTemplateInventory,
                weaponParentId,
                weaponModChances,
                "pmcUSEC",
                true,
                69
            );

            var itemSize = _inventoryHelper.GetItemSize(
                weaponTpl,
                result.Weapon[0].Id,
                result.Weapon
            );

            Assert.AreEqual(weaponTpl, result.WeaponTemplate.Id);

            // Ensure it's bigger than just weapon lower
            Assert.AreNotEqual(2, itemSize.Item1);
            Assert.AreNotEqual(1, itemSize.Item2);
        }
    }
}
