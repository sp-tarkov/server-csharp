using System.Text.Json;

namespace Core.Services;

public class I18nService
{
    private List<string> _locales;
    private Dictionary<string, string> _fallbacks;
    private string _defaultLocale;
    private string _directory;

    private string _setLocale;

    private Dictionary<string, Dictionary<string, string>> _loadedLocales = new();

    public I18nService(List<string> locales, Dictionary<string, string> fallbacks, string defaultLocale, string directory)
    {
        _locales = locales;
        _fallbacks = fallbacks;
        _defaultLocale = defaultLocale;
        _directory = directory;

        Initialize();
    }

    private void Initialize()
    {
        var files = Directory.GetFiles(_directory, "*.json");
        if (files.Length == 0)
            throw new Exception($"Localisation files in directory {_directory} not found.");
        foreach (var file in files)
            _loadedLocales.Add(Path.GetFileNameWithoutExtension(file),
                JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(file)) ?? new Dictionary<string, string>());

        if (!_loadedLocales.ContainsKey(_defaultLocale))
            throw new Exception($"The default locale '{_defaultLocale}' does not exist on the loaded locales.");
    }

    public void SetLocale(string locale)
    {
        if (_loadedLocales.ContainsKey(locale))
        {
            _setLocale = locale;
        }
        else
        {
            var fallback = _fallbacks.Where(kv => locale.StartsWith(kv.Key.Replace("*", "")));
            if (fallback.Any())
            {
                var foundFallbackLocale = fallback.First().Value;
                if (!_loadedLocales.ContainsKey(foundFallbackLocale))
                    throw new Exception($"Locale '{locale}' was not defined, and the found fallback locale did not match any of the loaded locales.");
                _setLocale = foundFallbackLocale;
            }

            _setLocale = _defaultLocale;
        }
    }

    public string GetLocalised(string key)
    {
        if (!_loadedLocales.TryGetValue(_setLocale, out var locales))
            return key;
        if (!locales.TryGetValue(key, out var value))
        {
            _loadedLocales.TryGetValue(_defaultLocale, out var defaults);
            defaults.TryGetValue(key, out value);
            return value ?? key;
        }

        return value;
    }

    public string GetLocalised(string key, params object[] args)
    {
        // TODO: Deal with arguments
        return GetLocalised(key);
    }
}