using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Weather;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Weather;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Generators.WeatherGen;

public abstract record AbstractWeatherPresetGeneratorBase(WeightedRandomHelper WeightedRandomHelper, RandomUtil RandomUtil)
    : IWeatherPresetGenerator
{
    public abstract bool CanHandle(WeatherPreset preset);

    public abstract Weather Generate(PresetWeights weatherWeights);

    protected WindDirection GetWeightedWindDirection(PresetWeights weather)
    {
        return WeightedRandomHelper.WeightedRandom(weather.WindDirection.Values, weather.WindDirection.Weights).Item;
    }

    protected double GetWeightedClouds(PresetWeights weather)
    {
        return double.Parse(WeightedRandomHelper.GetWeightedValue(weather.Clouds));
    }

    protected double GetWeightedWindSpeed(PresetWeights weather)
    {
        return WeightedRandomHelper.WeightedRandom(weather.WindSpeed.Values, weather.WindSpeed.Weights).Item;
    }

    protected double GetWeightedFog(PresetWeights weather)
    {
        return WeightedRandomHelper.WeightedRandom(weather.Fog.Values, weather.Fog.Weights).Item;
    }

    protected double GetWeightedRain(PresetWeights weather)
    {
        return WeightedRandomHelper.WeightedRandom(weather.Rain.Values, weather.Rain.Weights).Item;
    }

    protected double GetRandomDouble(double min, double max, int precision = 3)
    {
        return Math.Round(RandomUtil.GetDouble(min, max), precision);
    }
}
