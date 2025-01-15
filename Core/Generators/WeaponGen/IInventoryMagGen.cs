namespace Core.Generators.WeaponGen;

public interface IInventoryMagGen
{
    public abstract int GetPriority();
    public abstract bool CanHandleInventoryMagGen(InventoryMagGen inventoryMagGen);
    public abstract void Process(InventoryMagGen inventoryMagGen);
}
