using Core.DI;
using Core.Models.Utils;
using SptCommon.Annotations;

namespace _8OnLoad
{
    // Flag class as being OnLoad and give it a load priority, check `OnLoadOrder` for list of possible choices
    [Injectable(InjectableTypeOverride = typeof(IOnLoad), TypePriority = OnLoadOrder.PostSptDatabase)] // Can also give an int value for fine-grained control
    [Injectable(InjectableTypeOverride = typeof(OnLoadExample))]
    public class OnLoadExample : IOnLoad // Must implement the IOnLoad interface
    {
        private readonly ISptLogger<OnLoadExample> _logger;

        public OnLoadExample(
            ISptLogger<OnLoadExample> logger)
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
