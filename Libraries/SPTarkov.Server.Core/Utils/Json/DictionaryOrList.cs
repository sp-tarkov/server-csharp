namespace SPTarkov.Server.Core.Utils.Json;

public class DictionaryOrList<K, V>(Dictionary<K, V>? dictionary, List<V>? list)
{
    public Dictionary<K, V>? Dictionary
    {
        get;
    } = dictionary;

    public List<V>? List
    {
        get;
    } = list;

    public bool IsList
    {
        get
        {
            return List != null;
        }
    }

    public bool IsDictionary
    {
        get
        {
            return Dictionary != null;
        }
    }
}
