using System.Collections.Frozen;
using SPTarkov.Common.Extensions;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Json;

namespace SPTarkov.Server.Core.Services.Locales;

/// <summary>
///     Handles translating server text into different languages
/// </summary>
[Injectable(InjectionType.Singleton)]
public class ServerLocalisationService(
    ISptLogger<ServerLocalisationService> logger,
    RandomUtil randomUtil,
    LocaleService localeService,
    JsonUtil jsonUtil,
    FileUtil fileUtil
) : AbstractLocalisationService<ServerLocalisationService>(logger, localeService)
{
    private const string DefaultLocale = "en";
    private const string LocaleDirectory = "./SPT_Data/database/locales/server";

    protected override void LoadLocales()
    {
        if (LocalesHydrated)
        {
            return;
        }

        var files = fileUtil.GetFiles(LocaleDirectory, true).Where(f => fileUtil.GetFileExtension(f) == "json");

        if (!files.Any())
        {
            throw new Exception($"Localisation files in directory {LocaleDirectory} not found.");
        }

        foreach (var file in files)
        {
            LoadedLocales.Add(
                fileUtil.StripExtension(file),
                new LazyLoad<Dictionary<string, string>>(() => jsonUtil.DeserializeFromFile<Dictionary<string, string>>(file) ?? [])
            );
        }

        if (!LoadedLocales.ContainsKey(DefaultLocale))
        {
            throw new Exception($"The default locale '{DefaultLocale}' does not exist on the loaded locales.");
        }

        LocalesHydrated = true;
    }

    /// <summary>
    ///     From the provided partial key, find all keys that start with text and choose a random match
    /// </summary>
    /// <param name="partialKey"> Key to match locale keys on </param>
    /// <returns> Locale text </returns>
    public string GetRandomTextThatMatchesPartialKey(string partialKey)
    {
        var matchingKeys = GetLocaleKeys().Where(x => x.Contains(partialKey));

        if (!matchingKeys.Any())
        {
            logger.Warning($"No locale keys found for: {partialKey}");

            return string.Empty;
        }

        return GetText(randomUtil.GetArrayValue(matchingKeys));
    }
}
