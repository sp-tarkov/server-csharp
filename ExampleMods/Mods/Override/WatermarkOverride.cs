using Core.Annotations;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using Core.Utils;

namespace ExampleMods.Mods.Override;

[Injectable(InjectableTypeOverride = typeof(Watermark))]
public class WatermarkOverride : Watermark
{
    public WatermarkOverride(
        ILogger logger,
        ConfigServer configServer,
        LocalisationService localisationService,
        WatermarkLocale watermarkLocale
    ) : base(logger, configServer, localisationService, watermarkLocale)
    {
    }

    public override void Initialize()
    {
        Console.WriteLine("This is a watermark mod override!");
        base.Initialize();
    }
}
