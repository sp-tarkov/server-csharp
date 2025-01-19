using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SptCommon.Annotations;
using Core.Models.Logging;
using Core.Models.Utils;

namespace ExampleMods.Mods
{
    [Injectable]
    public class Logging
    {
        private readonly ISptLogger<Logging> _logger;

        // Constructor - Inject a 'ISptLogger' with your mods Class in the diamond brackets
        public Logging(
            ISptLogger<Logging> logger)
        {
            // Save the logger we're injecting into a private variable that is scoped to this class (only this class has access to it)
            _logger = logger;

            // Not 100% necessary but let's split our code out into a method to make it easier to read
            Run();
        }

        public void Run()
        {
            // We can access the logger to assigned in the constructor here
            _logger.Success("This is a success message");
            _logger.Warning("This is a warning message");
            _logger.Error("This is an error message");
            _logger.Info("This is an info message");
            _logger.Critical("this is a critical message");

            // Logging with colors requires you to 'pass' the text color and background color
            _logger.LogWithColor("This is a message with custom colors", LogTextColor.Red, LogBackgroundColor.Black);
            _logger.Debug("This is a debug message that gets written to the log file, not the console");
        }
    }
}
