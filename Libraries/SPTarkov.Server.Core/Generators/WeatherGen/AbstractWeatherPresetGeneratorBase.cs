using System.Globalization;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Weather;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Weather;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Generators.WeatherGen;

public abstract class AbstractWeatherPresetGeneratorBase(WeightedRandomHelper weightedRandomHelper, RandomUtil randomUtil)
    : IWeatherPresetGenerator
{
    public abstract bool CanHandle(WeatherPreset preset);

    public abstract Weather Generate(PresetWeights weatherWeights);

    protected WindDirection GetWeightedWindDirection(PresetWeights weather)
    {
        return weightedRandomHelper.GetWeightedValue(weather.WindDirection);
    }

    protected double GetWeightedClouds(PresetWeights weather)
    {
        return double.Parse(weightedRandomHelper.GetWeightedValue(weather.Clouds), CultureInfo.InvariantCulture);
    }

    protected double GetWeightedWindSpeed(PresetWeights weather)
    {
        return double.Parse(weightedRandomHelper.GetWeightedValue(weather.WindSpeed), CultureInfo.InvariantCulture);
    }

    protected double GetWeightedFog(PresetWeights weather)
    {
        return double.Parse(weightedRandomHelper.GetWeightedValue(weather.Fog), CultureInfo.InvariantCulture);
    }

    protected double GetWeightedRain(PresetWeights weather)
    {
        return double.Parse(weightedRandomHelper.GetWeightedValue(weather.Rain), CultureInfo.InvariantCulture);
    }

    protected double GetRandomDouble(double min, double max, int precision = 3)
    {
        return Math.Round(randomUtil.GetDouble(min, max), precision);
    }
}
