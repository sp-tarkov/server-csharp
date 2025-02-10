using Core.Models.External;
using Core.Models.Utils;
using Core.Utils;
using SptCommon.Annotations;

namespace _4ReadCustomJson5Config
{
    [Injectable]
    public class ReadJson5Config: IPreSptLoadMod
    {
        private readonly ISptLogger<ReadJson5Config> _logger;
        private readonly FileUtil _fileUtil;
        private readonly JsonUtil _jsonUtil;

        public ReadJson5Config(
            ISptLogger<ReadJson5Config> logger,
            FileUtil fileUtil,
            JsonUtil jsonUtil)
        {
            _logger = logger;
            _fileUtil = fileUtil;
            _jsonUtil = jsonUtil;
        }

        public void PreSptLoad()
        {
            // Read the content of the config file into a string
            var rawContent = _fileUtil.ReadFile("config.json5");

            // Take the string above and deserialise it into a config file with a type (defined between the diamond brackets)
            var config = _jsonUtil.Deserialize<ModConfig>(rawContent);

            _logger.Success($"Read property: 'ExampleProperty' from config with value: {config.ExampleProperty}");
        }
    }

    // This class should represent your config structure
    public class ModConfig
    {
        public string ExampleProperty { get; set; }
    }
}
