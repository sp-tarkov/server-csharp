using Core.Annotations;
using Core.Models.Eft.Common.Tables;

namespace Core.Generators.WeaponGen.Implementations;

[Injectable]
public class ExternalInventoryMagGen : InventoryMagGen
{
    public ExternalInventoryMagGen()
    {
    }

    public int GetPriority()
    {
        return 99;
    }

    public bool CanHandleInventoryMagGen(InventoryMagGen inventoryMagGen)
    {
        throw new NotImplementedException();
    }

    public void Process(InventoryMagGen inventoryMagGen)
    {
        throw new NotImplementedException();
    }

    public TemplateItem? GetRandomExternalMagazineForInternalMagazineGun(string weaponTpl, List<string> magazineBlacklist)
    {
        throw new NotImplementedException();
    }
}
