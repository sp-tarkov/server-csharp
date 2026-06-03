using System.Collections.Frozen;
using SPTarkov.Common.Extensions;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Utils.Json;

namespace SPTarkov.Server.Core.Services.Locales;

public abstract class AbstractLocalisationService<T>(ISptLogger<T> logger, LocaleService localeService)
{
    public string ServerLocale { get; protected set; } = localeService.GetDesiredServerLocale();
    public bool LocalesHydrated { get; protected set; }

    protected Dictionary<string, LazyLoad<Dictionary<string, string>>> LoadedLocales { get; set; } = [];

    private const string DefaultLocale = "en";
    private readonly FrozenDictionary<string, string> _localeFallbacks = localeService.GetLocaleFallbacks().ToFrozenDictionary();

    protected abstract void LoadLocales();

    public void SetServerLocaleByKey(string locale)
    {
        if (!LocalesHydrated)
        {
            LoadLocales();
        }

        if (LoadedLocales.ContainsKey(locale))
        {
            ServerLocale = locale;
        }
        else
        {
            var fallback = _localeFallbacks.Where(kv => locale.StartsWith(kv.Key.Replace("*", "")));
            if (fallback.Any())
            {
                var foundFallbackLocale = fallback.First().Value;
                if (!LoadedLocales.ContainsKey(foundFallbackLocale))
                {
                    throw new Exception(
                        $"Locale '{locale}' was not defined, and the found fallback locale did not match any of the loaded locales."
                    );
                }

                ServerLocale = foundFallbackLocale;
                return;
            }

            ServerLocale = DefaultLocale;
        }
    }

    /// <summary>
    ///     Get a localised value using the passed in key
    /// </summary>
    /// <param name="key"> Key to look up locale for </param>
    /// <param name="args"> optional arguments </param>
    /// <returns> Localised string </returns>
    public string GetText(string key, object? args = null)
    {
        return args is null ? GetLocalisedValue(key) : GetLocalised(key, args);
    }

    /// <summary>
    ///     Get a localised value using the passed in key
    /// </summary>
    /// <param name="key"> Key to look up locale for </param>
    /// <param name="value"> Value to localize </param>
    /// <returns> Localised string </returns>
    public string GetText<TL>(string key, TL value)
        where TL : IConvertible?
    {
        return GetLocalised(key, value);
    }

    /// <summary>
    ///     Get all locale keys
    /// </summary>
    /// <returns> Generic collection of keys </returns>
    public IEnumerable<string> GetLocaleKeys()
    {
        if (!LocalesHydrated)
        {
            LoadLocales();
        }

        return LoadedLocales["en"].Value?.Keys ?? Enumerable.Empty<string>();
    }

    public string GetLocalisedValue(string key)
    {
        if (!LocalesHydrated)
        {
            LoadLocales();
        }

        // get loaded locales for set key
        if (!LoadedLocales.TryGetValue(ServerLocale, out var locales))
        {
            // if we are unable to get the "loadedLocales" for the set locale, return the key
            return key;
        }

        // searching through loaded locales for given key
        if (!locales.Value.TryGetValue(key, out var value))
        {
            // if the key is not found in loaded locales
            // check if the key is found in the default locale
            LoadedLocales.TryGetValue(DefaultLocale, out var defaults);
            if (!defaults.Value.TryGetValue(key, out value))
            {
                value = localeService.GetLocaleDb(DefaultLocale).FirstOrDefault(x => x.Key == key).Value;
            }

            return value ?? key;
        }

        // if the key is found in the server locale, return the value
        return value;
    }

    protected string GetLocalised(string key, object? args)
    {
        var rawLocalizedString = GetLocalisedValue(key);
        if (args == null)
        {
            return rawLocalizedString;
        }

        var typeProperties = args.GetType().GetProperties();

        foreach (var propertyInfo in typeProperties)
        {
            var localizedName = $"{{{{{propertyInfo.GetJsonName()}}}}}";
            if (rawLocalizedString.Contains(localizedName))
            {
                rawLocalizedString = rawLocalizedString.Replace(localizedName, propertyInfo.GetValue(args)?.ToString() ?? string.Empty);
            }
        }

        return rawLocalizedString;
    }

    protected string GetLocalised<TL>(string key, TL? value)
        where TL : IConvertible?
    {
        var rawLocalizedString = GetLocalisedValue(key);
        return rawLocalizedString.Replace("%s", value?.ToString() ?? string.Empty);
    }

    // gets the localized string directly
    protected string GetLocalised<TL>(string key)
    {
        return GetLocalisedValue(key);
    }
}
