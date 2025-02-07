namespace Core.Utils.Json;

public class ListOrT<T>(List<T>? list, T? item)
{
    public List<T>? List
    {
        get;
    } = list;

    public T? Item
    {
        get;
    } = item;

    public bool IsItem
    {
        get
        {
            return Item != null;
        }
    }

    public bool IsList
    {
        get
        {
            return List != null;
        }
    }
}
