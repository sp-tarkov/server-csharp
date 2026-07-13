namespace SPTarkov.Server.Core.Migration;

public sealed class ProfileMigrationContext
{
    private readonly Dictionary<string, object?> _items = [];

    public void Set<T>(string key, T value)
    {
        _items[key] = value;
    }

    public T Get<T>(string key, T defaultValue = default!)
    {
        if (!_items.TryGetValue(key, out var value) || value is not T typedValue)
        {
            return defaultValue;
        }

        return typedValue;
    }
}
