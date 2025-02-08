using Core.Models.External;
using Core.Models.Utils;

namespace ExampleMods.Mods._7UseMultipleClasses
{
    /// <summary>
    /// Having multiple classes can make keeping your code maintainable easier, you can split related code into their own class
    /// </summary>
    public class UseMultipleClasses: IPostDBLoadMod
    {
        private readonly ISptLogger<UseMultipleClasses> _logger;

        public UseMultipleClasses(
            ISptLogger<UseMultipleClasses> _logger)
        {
            this._logger = _logger;
        }

        public void PostDBLoad()
        {
            // We create an instance of the other class
            var otherClass = new SecondClass();

            // We call the "GetText" method that exists in the other class
            var text = otherClass.GetText();

            // Log the result to the server console
            _logger.Info($"The SecondClass returned the text: {text}");
        }
    }
}
