using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Generators.Weather;

[Injectable]
public class RainyPreset(WeightedRandomHelper weightedRandomHelper, RandomUtil randomUtil)
    : AbstractWeatherPreset(weightedRandomHelper, randomUtil)
{
    public override bool CanHandle(WeatherPreset preset)
    {
        return preset == WeatherPreset.RAINY;
    }

    public override Models.Eft.Weather.Weather Generate(PresetWeights weatherWeights)
    {
        var clouds = GetWeightedClouds(weatherWeights);

        var result = new Models.Eft.Weather.Weather
        {
            Pressure = GetRandomDouble(weatherWeights.Pressure.Min, weatherWeights.Pressure.Max),
            Temperature = 0, // // Handled in caller
            Fog = GetWeightedFog(weatherWeights),
            RainIntensity = GetRandomDouble(weatherWeights.RainIntensity.Min, weatherWeights.RainIntensity.Max),
            Rain = GetWeightedRain(weatherWeights),
            WindGustiness = GetRandomDouble(weatherWeights.WindGustiness.Min, weatherWeights.WindGustiness.Max, 2),
            WindDirection = GetWeightedWindDirection(weatherWeights),
            WindSpeed = GetWeightedWindSpeed(weatherWeights),
            Cloud = clouds,
            Time = string.Empty,
            Date = string.Empty,
            Timestamp = 0,
            SptInRaidTimestamp = 0, // Handled in caller
        };

        return result;
    }
}
