using Core.Annotations;

namespace Core.Generators.WeaponGen.Implementations;

[Injectable]
public class InternalMagazineInventoryMagGen : InventoryMagGen
{
    public InternalMagazineInventoryMagGen()
    {
    }

    public int GetPriority()
    {
        return 0;
    }

    public bool CanHandleInventoryMagGen(InventoryMagGen inventoryMagGen)
    {
        throw new NotImplementedException();
    }

    public void Process(InventoryMagGen inventoryMagGen)
    {
        throw new NotImplementedException();
    }
}
