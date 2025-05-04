namespace SPTarkov.Server.Core.Utils.Json;

public class ListOrT<T>(List<T>? list, T? item)
{
    // Do not remove, its used by the cloner
    // ReSharper disable once UnusedMember.Local
    private ListOrT()
        : this(null, default) { }

    public List<T>? List
    {
        get;
        // Do not remove, its used by the cloner
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        private set;
    } = list;

    public T? Item
    {
        get;
        // do not remove, its used by the cloner
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        private set;
    } = item;

    public bool IsItem
    {
        get { return Item != null; }
    }

    public bool IsList
    {
        get { return List != null; }
    }
}
