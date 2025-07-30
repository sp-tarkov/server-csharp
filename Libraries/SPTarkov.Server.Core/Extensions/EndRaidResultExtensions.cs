using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Extensions
{
    public static class EndRaidResultExtensions
    {
        private static HashSet<ExitStatus> deathStates = [ExitStatus.KILLED, ExitStatus.MISSINGINACTION, ExitStatus.LEFT];

        /// <summary>
        ///     Checks to see if player survives. run through will return false
        /// </summary>
        /// <param name="results"> Post raid request </param>
        /// <returns> True if survived </returns>
        public static bool IsPlayerSurvived(this EndRaidResult results)
        {
            return results.Result == ExitStatus.SURVIVED;
        }

        /// <summary>
        ///     Is the player dead after a raid - dead = anything other than "survived" / "runner"
        /// </summary>
        /// <param name="results"> Post raid request </param>
        /// <returns> True if dead </returns>
        public static bool IsPlayerDead(this EndRaidResult results)
        {
            return deathStates.Contains(results.Result.Value);
        }

        /// <summary>
        ///     Has the player moved from one map to another
        /// </summary>
        /// <param name="results"> Post raid request </param>
        /// <returns> True if players transferred </returns>
        public static bool IsMapToMapTransfer(this EndRaidResult results)
        {
            return results.Result == ExitStatus.TRANSIT;
        }

        /// <summary>
        ///     Was extract by car
        /// </summary>
        /// <param name="requestResults">Result object from completed raid</param>
        /// <param name="carExtracts">Car extract names</param>
        /// <returns> True if extract was by car </returns>
        public static bool TookCarExtract(this EndRaidResult? requestResults, HashSet<string> carExtracts)
        {
            // exit name is undefined on death
            if (string.IsNullOrEmpty(requestResults?.ExitName))
            {
                return false;
            }

            if (requestResults.ExitName.ToLowerInvariant().Contains("v-ex"))
            {
                return true;
            }

            return carExtracts.Contains(requestResults.ExitName.Trim());
        }

        /// <summary>
        /// Raid exit was via coop extract
        /// </summary>
        /// <param name="raidResult">Result object from completed raid</param>
        /// <param name="coopExtracts"></param>
        /// <returns>True when exit was coop extract</returns>
        public static bool TookCoopExtract(this EndRaidResult? raidResult, HashSet<string> coopExtracts)
        {
            return raidResult?.ExitName is not null && coopExtracts.Contains(raidResult.ExitName.Trim());
        }
    }
}
