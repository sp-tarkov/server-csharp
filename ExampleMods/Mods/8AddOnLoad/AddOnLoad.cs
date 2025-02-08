using Core.DI;
using Core.Models.Utils;
using SptCommon.Annotations;

namespace ExampleMods.Mods._8AddOnLoad
{
    // Flag class as being OnLoad and give it a load priority, check `OnLoadOrder` for list of possible choices
    [Injectable(InjectableTypeOverride = typeof(IOnLoad), TypePriority = OnLoadOrder.PostSptDatabase)]
    [Injectable(InjectableTypeOverride = typeof(AddOnLoad))]
    public class AddOnLoad : IOnLoad // Must implement the IOnLoad interface
    {
        private readonly ISptLogger<AddOnLoad> _logger;

        public AddOnLoad(
            ISptLogger<AddOnLoad> logger)
        {
            _logger = logger;
        }

        public Task OnLoad()
        {
            // Can do work here
            _logger.Success($"Mod loaded after database!");

            return Task.CompletedTask;
        }

        public string GetRoute()
        {
            return "mod-load-example";
        }
    }
}
