using Core.Helpers;
using System.Reflection;
using Core.Models.External;
using Core.Models.Utils;
using fastJSON5;
using SptCommon.Annotations;

namespace _4ReadCustomJson5Config
{
    [Injectable]
    public class ReadJson5Config: IPreSptLoadMod
    {
        private readonly ISptLogger<ReadJson5Config> _logger;
        private readonly ModHelper _modHelper;

        public ReadJson5Config(
            ISptLogger<ReadJson5Config> logger,
            ModHelper modHelper)
        {
            _logger = logger;
            _modHelper = modHelper;
        }

        public void PreSptLoad()
        {
            var pathToMod = _modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());

            // To use JSON5, you will have to find and provide your own JSON5 library to decode it
            var json5Config = JSON5.ToObject<ModConfig>(_modHelper.GetRawFileData(pathToMod, "config.json5"));

            _logger.Success($"Read property: 'ExampleProperty' from config with value: {json5Config.ExampleProperty}");
        }
    }

    // This class should represent your config structure
    public class ModConfig
    {
        public string ExampleProperty { get; set; }
    }
}
