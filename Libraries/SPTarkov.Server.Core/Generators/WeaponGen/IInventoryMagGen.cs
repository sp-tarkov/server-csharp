namespace SPTarkov.Server.Core.Generators.WeaponGen;

public interface IInventoryMagGen
{
    public int GetPriority();
    public bool CanHandleInventoryMagGen(InventoryMagGen inventoryMagGen);
    public void Process(InventoryMagGen inventoryMagGen);
}
