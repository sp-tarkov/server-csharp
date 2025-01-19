using SptCommon.Annotations;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using Core.Utils;

namespace ExampleMods.Mods.Override;

[Injectable(InjectableTypeOverride = typeof(Watermark))]
public class WatermarkOverride(
    ISptLogger<Watermark> _logger,
    ConfigServer _configServer,
    LocalisationService _localisationService,
    WatermarkLocale _watermarkLocale
) : Watermark(_logger, _configServer, _localisationService, _watermarkLocale) // was testing overriding with primary constructors, works fine from what i can see
{
    public override void Initialize()
    {
        _logger.Success("This is a watermark mod override!");
        base.Initialize();
    }

    // public override string GetVersionTag(bool withEftVersion = false)
    // {
    //     // _logger.Success("asdasdasda");
    //     return base.GetVersionTag(withEftVersion);
    // }
}
