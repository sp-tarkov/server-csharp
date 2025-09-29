using SPTarkov.Server.Core.Models.Spt.Config;

namespace SPTarkov.Server.Core.Models.Spt.Weather;

public interface IWeatherPresetGenerator
{
    public bool CanHandle(WeatherPreset preset);
    public Eft.Weather.Weather Generate(PresetWeights weatherWeights);
}
