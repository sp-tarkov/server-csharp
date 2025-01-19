using Core.Utils;
using SptCommon.Extensions;

namespace Core.Services;

public class I18nService
{
    private List<string> _locales;
    private Dictionary<string, string> _fallbacks;
    private string _defaultLocale;
    private string _directory;
    private JsonUtil _jsonUtil;
    private FileUtil _fileUtil;
    private string _setLocale;

    private Dictionary<string, Dictionary<string, string>> _loadedLocales = new();
    // TODO: try convert to primary ctor
    public I18nService(
        FileUtil fileUtil,
        JsonUtil jsonUtil,
        List<string> locales,
        Dictionary<string, string> fallbacks,
        string defaultLocale,
        string directory
    )
    {
        _locales = locales;
        _fallbacks = fallbacks;
        _defaultLocale = defaultLocale;
        _directory = directory;
        _jsonUtil = jsonUtil;
        _fileUtil = fileUtil;

        Initialize();
    }

    private void Initialize()
    {
        var files = _fileUtil.GetFiles(_directory, true).Where(f => _fileUtil.GetFileExtension(f) == "json").ToList();
        if (files.Count == 0)
            throw new Exception($"Localisation files in directory {_directory} not found.");
        foreach (var file in files)
            _loadedLocales.Add(_fileUtil.StripExtension(file),
                _jsonUtil.Deserialize<Dictionary<string, string>>(_fileUtil.ReadFile(file)) ??
                new Dictionary<string, string>());

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
                    throw new Exception(
                        $"Locale '{locale}' was not defined, and the found fallback locale did not match any of the loaded locales.");
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

    public string GetLocalised<T>(string key)
    {
        return GetLocalised(key);
    }

    public string GetLocalised(string key, object? args)
    {
        var rawLocalizedString = GetLocalised(key);
        if (args == null)
        {
            return rawLocalizedString;
        }

        foreach (var propertyInfo in args.GetType().GetProperties())
        {
            var localizedName = $"{{{{{propertyInfo.GetJsonName()}}}}}";
            if (rawLocalizedString.Contains(localizedName))
            {
                rawLocalizedString.Replace(localizedName, propertyInfo.GetValue(args, null)?.ToString() ?? string.Empty);
            }
        }
        return rawLocalizedString;
    }

    public string GetLocalised<T>(string key, T value) where T : IConvertible
    {
        var rawLocalizedString = GetLocalised(key);
        return rawLocalizedString.Replace("%s", value.ToString());
    }
}
