using SPTarkov.Server.Core.Models.Spt.Config;

namespace SPTarkov.Server.Core.Generators.Weather;

public interface IWeatherPreset
{
    public bool CanHandle(WeatherPreset preset);
    public Models.Eft.Weather.Weather Generate(PresetWeights weatherWeights);
}
