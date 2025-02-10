using Core.DI;
using Core.Models.Utils;
using SptCommon.Annotations;

namespace _9OnUpdate
{
    // Flag class as being OnLoad and give it a load priority, check `OnLoadOrder` for list of possible choices
    [Injectable(InjectableTypeOverride = typeof(IOnUpdate), TypePriority = OnUpdateOrder.PostSptUpdate)] // Can also give it an int value for more fine-grained control
    [Injectable(InjectableTypeOverride = typeof(OnUpdateExample))]
    public class OnUpdateExample : IOnUpdate // Must implement the IOnUpdate interface
    {
        private readonly ISptLogger<OnUpdateExample> _logger;

        public OnUpdateExample(
            ISptLogger<OnUpdateExample> logger)
        {
            _logger = logger;
        }

        public bool OnUpdate(long timeSinceLastRun)
        {
            // Can do work here
            _logger.Success($"Mod running update after SPT updates have run!");

            return true; // Return true for a success, false for failure
        }

        public string GetRoute()
        {
            return "mod-update-example";
        }
    }
}
