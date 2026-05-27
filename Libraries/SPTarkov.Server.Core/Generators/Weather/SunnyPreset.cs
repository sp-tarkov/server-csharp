using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Generators.Weather;

[Injectable]
public class SunnyPreset(WeightedRandomHelper weightedRandomHelper, RandomUtil randomUtil)
    : AbstractWeatherPreset(weightedRandomHelper, randomUtil)
{
    public override bool CanHandle(WeatherPreset preset)
    {
        return preset == WeatherPreset.SUNNY;
    }

    public override Models.Eft.Weather.Weather Generate(PresetWeights weatherWeights)
    {
        var result = new Models.Eft.Weather.Weather
        {
            Pressure = GetRandomDouble(weatherWeights.Pressure.Min, weatherWeights.Pressure.Max),
            Temperature = 0, // Handled in caller
            Fog = GetWeightedFog(weatherWeights),
            RainIntensity = 0,
            Rain = 0,
            WindGustiness = GetRandomDouble(weatherWeights.WindGustiness.Min, weatherWeights.WindGustiness.Max, 2),
            WindDirection = GetWeightedWindDirection(weatherWeights),
            WindSpeed = GetWeightedWindSpeed(weatherWeights),
            Cloud = GetWeightedClouds(weatherWeights),
            Time = string.Empty,
            Date = string.Empty,
            Timestamp = 0,
            SptInRaidTimestamp = 0, // Handled in caller
        };

        return result;
    }
}
