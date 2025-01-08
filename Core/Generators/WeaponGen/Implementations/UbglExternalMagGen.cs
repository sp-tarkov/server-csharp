namespace Core.Generators.WeaponGen.Implementations;

public class UbglExternalMagGen : InventoryMagGen
{
    public UbglExternalMagGen()
    {
    }

    public int GetPriority()
    {
        return 1;
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