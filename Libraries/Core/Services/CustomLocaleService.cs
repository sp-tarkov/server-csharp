using Core.Models.Utils;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Services
{
    [Injectable]
    public class CustomLocaleService(
        ISptLogger<CustomLocaleService> logger,
        FileUtil fileUtil,
        DatabaseService databaseService
        )
    {

        /// <summary>
        /// Path should link to a folder containing every locale that should be added to the server locales
        /// e.g. en.json for english, fr.json for french
        /// Inside each JSON should be a Dictionary of the locale key and localised text
        /// </summary>
        /// <param name="pathToServerLocales">A path to a folder that contains locales to add to SPT</param>
        public void AdServerLocales(string pathToServerLocales)
        {

        }

        /// <summary>
        /// Path should link to a folder containing every locale that should be added to the game locales
        /// e.g. en.json for english, fr.json for french
        /// Inside each JSON should be a Dictionary of the locale key and localised text
        /// </summary>
        /// <param name="pathToGameLocales">A path to a folder that contains locales to add to SPT</param>
        public void AddGameLocales(string pathToGameLocales)
        {

        }
    }
}
