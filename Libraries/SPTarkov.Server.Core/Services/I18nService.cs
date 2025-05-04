using SPTarkov.Common.Extensions;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Json;

namespace SPTarkov.Server.Core.Services;

public class I18nService
{
    private readonly string _defaultLocale;
    private readonly string _directory;
    private readonly Dictionary<string, string> _fallbacks;
    private readonly FileUtil _fileUtil;
    private readonly JsonUtil _jsonUtil;

    private readonly Dictionary<string, LazyLoad<Dictionary<string, string>>> _loadedLocales =
        new();
    private readonly LocaleService _localeService;
    private HashSet<string> _locales;
    private string? _setLocale;

    public I18nService(
        FileUtil fileUtil,
        JsonUtil jsonUtil,
        HashSet<string> locales,
        Dictionary<string, string> fallbacks,
        string defaultLocale,
        string directory,
        LocaleService localeService
    )
    {
        _locales = locales;
        _fallbacks = fallbacks;
        _defaultLocale = defaultLocale;
        _directory = directory;
        _jsonUtil = jsonUtil;
        _fileUtil = fileUtil;
        _localeService = localeService;

        Initialize();
    }

    private void Initialize()
    {
        var files = _fileUtil
            .GetFiles(_directory, true)
            .Where(f =>
            {
                return _fileUtil.GetFileExtension(f) == "json";
            })
            .ToList();
        if (files.Count == 0)
        {
            throw new Exception($"Localisation files in directory {_directory} not found.");
        }

        foreach (var file in files)
        {
            _loadedLocales.Add(
                _fileUtil.StripExtension(file),
                new LazyLoad<Dictionary<string, string>>(() =>
                {
                    return _jsonUtil.DeserializeFromFile<Dictionary<string, string>>(file)
                        ?? new Dictionary<string, string>();
                })
            );
        }

        if (!_loadedLocales.ContainsKey(_defaultLocale))
        {
            throw new Exception(
                $"The default locale '{_defaultLocale}' does not exist on the loaded locales."
            );
        }
    }

    public void SetLocaleByKey(string locale)
    {
        if (_loadedLocales.ContainsKey(locale))
        {
            _setLocale = locale;
        }
        else
        {
            var fallback = _fallbacks.Where(kv =>
            {
                return locale.StartsWith(kv.Key.Replace("*", ""));
            });
            if (fallback.Any())
            {
                var foundFallbackLocale = fallback.First().Value;
                if (!_loadedLocales.ContainsKey(foundFallbackLocale))
                {
                    throw new Exception(
                        $"Locale '{locale}' was not defined, and the found fallback locale did not match any of the loaded locales."
                    );
                }

                _setLocale = foundFallbackLocale;
            }

            _setLocale = _defaultLocale;
        }
    }

    public string GetLocalisedValue(string key)
    {
        // get loaded locales for set key
        if (!_loadedLocales.TryGetValue(_setLocale, out var locales))
        {
            // if we are unable to get the "loadedLocales" for the set locale, return the key
            return key;
        }

        // searching through loaded locales for given key
        if (!locales.Value.TryGetValue(key, out var value))
        {
            // if the key is not found in loaded locales
            // check if the key is found in the default locale
            _loadedLocales.TryGetValue(_defaultLocale, out var defaults);
            if (!defaults.Value.TryGetValue(key, out value))
            {
                value = _localeService
                    .GetLocaleDb(_defaultLocale)
                    .FirstOrDefault(x =>
                    {
                        return x.Key == key;
                    })
                    .Value;
            }

            return value ?? key;
        }

        // if the key is found in the server locale, return the value
        return value;
    }

    public string GetLocalised<T>(string key)
    {
        return GetLocalisedValue(key);
    }

    public string GetLocalised(string key, object? args)
    {
        var rawLocalizedString = GetLocalisedValue(key);
        if (args == null)
        {
            return rawLocalizedString;
        }

        var typeToCheck = args.GetType();
        var typeProps = typeToCheck.GetProperties();

        foreach (var propertyInfo in args.GetType().GetProperties())
        {
            var localizedName = $"{{{{{propertyInfo.GetJsonName()}}}}}";
            if (rawLocalizedString.Contains(localizedName))
            {
                rawLocalizedString = rawLocalizedString.Replace(
                    localizedName,
                    propertyInfo.GetValue(args)?.ToString() ?? string.Empty
                );
            }
        }

        return rawLocalizedString;
    }

    public string GetLocalised<T>(string key, T? value)
        where T : IConvertible
    {
        var rawLocalizedString = GetLocalisedValue(key);
        return rawLocalizedString.Replace("%s", value?.ToString());
    }

    public List<string> GetLocalisedKeys()
    {
        return _loadedLocales["en"].Value?.Keys.ToList()!;
    }
}
