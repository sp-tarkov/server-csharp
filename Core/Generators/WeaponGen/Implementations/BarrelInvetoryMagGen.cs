namespace Core.Generators.WeaponGen.Implementations;

public class BarrelInvetoryMagGen : InventoryMagGen
{
    public BarrelInvetoryMagGen()
    {
        
    }

    public int GetPriority()
    {
        return 50;
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