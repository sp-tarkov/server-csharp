using System.Reflection;
using Core.Helpers;
using Core.Models.External;
using Core.Models.Utils;
using Core.Utils;
using SptCommon.Annotations;

namespace _5ReadCustomJsonConfig
{
    [Injectable]
    public class ReadJsonConfig : IPreSptLoadMod
    {
        private readonly ISptLogger<ReadJsonConfig> _logger;
        private readonly ModHelper _modHelper;

        public ReadJsonConfig(
            ISptLogger<ReadJsonConfig> logger,
            ModHelper modHelper)
        {
            _logger = logger;
            _modHelper = modHelper;
        }

        public void PreSptLoad()
        {
            // This will get us the full path to the mod, e.g. C:\spt\user\mods\5ReadCustomJsonConfig-0.0.1
            var pathToMod = _modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());

            // We give the path to the mod folder and the file we want to get, giving us the config, supply the files 'type' between the diamond brackets
            var config = _modHelper.GetJsonDataFromFile<ModConfig>(pathToMod, "config.json");

            _logger.Success($"Read property: 'ExampleProperty' from config with value: {config.ExampleProperty}");
        }
    }

    // This class should represent your config structure
    public class ModConfig
    {
        public string ExampleProperty
        {
            get; set;
        }
    }
}
