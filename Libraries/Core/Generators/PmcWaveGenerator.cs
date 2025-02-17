using Core.Models.Eft.Common;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Generators
{
    [Injectable]
    public class PmcWaveGenerator
    {
        protected ISptLogger<PmcWaveGenerator> _logger;
        protected RandomUtil _randomUtil;
        protected DatabaseService _databaseService;
        protected ConfigServer _configServer;
        protected PmcConfig _pmcConfig;

        public PmcWaveGenerator(
            ISptLogger<PmcWaveGenerator> _logger,
            RandomUtil _randomUtil,
            DatabaseService _databaseService,
            ConfigServer _configServer
            )
        {
            this._logger = _logger;
            this._randomUtil = _randomUtil;
            this._databaseService = _databaseService;
            this._configServer = _configServer;
            _pmcConfig = _configServer.GetConfig<PmcConfig>();
        }

        public void AddPmcWaveToLocation(string locationId, BossLocationSpawn waveToAdd)
        {
            _pmcConfig.CustomPmcWaves[locationId].Add(waveToAdd);
        }

        /**
     * Add custom boss and normal waves to maps found in config/location.json to db
     */
        public void ApplyWaveChangesToAllMaps() {
            foreach (var location in _pmcConfig.CustomPmcWaves) {
                ApplyWaveChangesToMapByName(location.Key);
            }
        }

        public void ApplyWaveChangesToMapByName(string name) {
            if (!_pmcConfig.CustomPmcWaves.TryGetValue(name, out var pmcWavesToAdd)) {
                return;
            }

            var location = _databaseService.GetLocation(name);
            if (location is null) {
                return;
            }

            location.Base.BossLocationSpawn.AddRange(pmcWavesToAdd);
        }

        public void ApplyWaveChangesToMap(LocationBase location) {
            if (!_pmcConfig.CustomPmcWaves.TryGetValue(location.Id.ToLower(), out var pmcWavesToAdd))
            {
                return;
            }

            location.BossLocationSpawn.AddRange(pmcWavesToAdd);
        }
    }
}
