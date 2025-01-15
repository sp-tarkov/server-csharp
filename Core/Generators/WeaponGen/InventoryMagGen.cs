using Core.Annotations;
using Core.Models.Eft.Common.Tables;

namespace Core.Generators.WeaponGen;

[Injectable]
public class InventoryMagGen
{
    private GenerationData _magCounts;
    private TemplateItem _magazineTemplate;
    private TemplateItem _weaponTemplate;
    private TemplateItem _ammoTemplate;
    private BotBaseInventory _pmcInventory;

    public InventoryMagGen()
    {
    }

    public InventoryMagGen
    (
        GenerationData magCounts,
        TemplateItem magazineTemplate,
        TemplateItem weaponTemplate,
        TemplateItem ammoTemplate,
        BotBaseInventory pmcInventory
    )
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
