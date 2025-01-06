using System.Text.Json;

namespace Core.Services;

public class I18nService
{
    private string[] _locales;
    private Dictionary<string, string> _fallbacks;
    private string _defaultLocale;
    private string _directory;

    private string _setLocale;
    
    private Dictionary<string, Dictionary<string, string>> _loadedLocales = new();

    public I18nService(string[] locales, Dictionary<string, string> fallbacks, string defaultLocale, string directory)
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
        {
            _loadedLocales.Add(Path.GetFileName(file),
                JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(file)) ?? new Dictionary<string, string>());
        }
    }

    public void SetLocale(string locale)
    {
        if (_loadedLocales.ContainsKey(locale))
            _setLocale = locale;
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
        }
    }

    public string GetLocalised(string key)
    {
        return null;
    }

    public string GetLocalised(string key, params object[] args)
    {
        return null;
    }
}