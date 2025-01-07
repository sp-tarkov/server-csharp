using Core.Models.Eft.Common.Tables;

namespace Core.Generators.WeaponGen.Implementations;

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