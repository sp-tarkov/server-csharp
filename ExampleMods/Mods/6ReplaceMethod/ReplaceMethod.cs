using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using Core.Utils;
using SptCommon.Annotations;

namespace ExampleMods.Mods._6ReplaceMethod
{
    [Injectable(InjectableTypeOverride = typeof(Watermark))]
    public class ReplaceMethod: Watermark
    {
        public ReplaceMethod(
            ISptLogger<Watermark> logger,
            ConfigServer configServer,
            LocalisationService localisationService,
            WatermarkLocale watermarkLocale)
            : base(logger, configServer, localisationService, watermarkLocale)
        {
            _configServer = configServer;
            _localisationService = localisationService;
            _watermarkLocale = watermarkLocale;
            _logger = logger;
        }

        public override void Initialize()
        {
            // We add a log message to the init method
            _logger.Success("This is a watermark mod override!");

            // This runs the original method (optional)
            base.Initialize();
        }
    }
}
