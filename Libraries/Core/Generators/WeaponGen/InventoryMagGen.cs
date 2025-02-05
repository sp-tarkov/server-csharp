using SptCommon.Annotations;
using Core.Models.Eft.Common.Tables;

namespace Core.Generators.WeaponGen;

[Injectable]
public class InventoryMagGen()
{
    private TemplateItem _ammoTemplate;
    private TemplateItem _magazineTemplate;
    private GenerationData _magCounts;
    private BotBaseInventory _pmcInventory;
    private TemplateItem _weaponTemplate;

    public InventoryMagGen
    (
        GenerationData magCounts,
        TemplateItem magazineTemplate,
        TemplateItem weaponTemplate,
        TemplateItem ammoTemplate,
        BotBaseInventory pmcInventory
    ) : this()
    {
        _magCounts = magCounts;
        _magazineTemplate = magazineTemplate;
        _weaponTemplate = weaponTemplate;
        _ammoTemplate = ammoTemplate;
        _pmcInventory = pmcInventory;
    }

    public GenerationData GetMagCount()
    {
        return _magCounts;
    }

    public TemplateItem GetMagazineTemplate()
    {
        return _magazineTemplate;
    }

    public TemplateItem GetWeaponTemplate()
    {
        return _weaponTemplate;
    }

    public TemplateItem GetAmmoTemplate()
    {
        return _ammoTemplate;
    }

    public BotBaseInventory GetPmcInventory()
    {
        return _pmcInventory;
    }
}
