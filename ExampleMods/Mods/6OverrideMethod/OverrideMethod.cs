using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using Core.Utils;
using SptCommon.Annotations;

namespace ExampleMods.Mods._6ReplaceMethod
{
    [Injectable(InjectableTypeOverride = typeof(Watermark))]
    public class OverrideMethod: Watermark
    {
        public OverrideMethod(
            ISptLogger<Watermark> logger, // The logger needs to use the same type as the overriden type (in this case, Watermark)
            ConfigServer configServer,
            LocalisationService localisationService,
            WatermarkLocale watermarkLocale)
            : base(logger, configServer, localisationService, watermarkLocale) // You must provide the parameters the overridden type requires
        { }

        public override void Initialize()
        {
            // We add a log message to the init method
            _logger.Success("This is a watermark mod override!");

            // This runs the original method (optional)
            base.Initialize();
        }
    }
}
