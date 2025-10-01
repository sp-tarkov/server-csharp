using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Weather;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Generators.WeatherGen;

[Injectable]
public class RainyWeatherGenerator(WeightedRandomHelper weightedRandomHelper, RandomUtil randomUtil)
    : AbstractWeatherPresetGeneratorBase(weightedRandomHelper, randomUtil)
{
    public override bool CanHandle(WeatherPreset preset)
    {
        return preset == WeatherPreset.RAINY;
    }

    public override Weather Generate(PresetWeights weatherWeights)
    {
        var clouds = GetWeightedClouds(weatherWeights);

        var result = new Weather
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
