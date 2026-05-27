namespace SPTarkov.Server.Core.Generators.Weapons;

public interface IInventoryMagGen
{
    public int GetPriority();
    public bool CanHandleInventoryMagGen(InventoryMagGen inventoryMagGen);
    public void Process(InventoryMagGen inventoryMagGen);
}
