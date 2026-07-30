using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Services.Locales;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Json;

namespace SPTarkov.Server.Web.Services;

[Injectable(InjectionType.Singleton)]
public class WebLocalizationService(
    ISptLogger<WebLocalizationService> logger,
    LocaleService localeService,
    FileUtil fileUtil,
    JsonUtil jsonUtil
) : AbstractLocalisationService<WebLocalizationService>(logger, localeService)
{
    private const string DefaultLocale = "en";
    private const string LocaleDirectory = "./SPT_Data/database/locales/web";

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
}
