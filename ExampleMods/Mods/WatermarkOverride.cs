using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using Core.Utils;
using SptCommon.Annotations;

namespace ExampleMods.Mods;

[Injectable(InjectableTypeOverride = typeof(Watermark))]
public class WatermarkOverride : Watermark // was testing overriding with primary constructors, works fine from what i can see
{
    public WatermarkOverride(ISptLogger<Watermark> logger,
        ConfigServer configServer,
        LocalisationService localisationService,
        WatermarkLocale watermarkLocale)
        : base(logger,
        configServer,
        localisationService,
        watermarkLocale)
    {
    }

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
