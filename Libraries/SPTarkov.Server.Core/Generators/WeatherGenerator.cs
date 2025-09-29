using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Weather;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Weather;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Core.Generators;

[Injectable]
public class WeatherGenerator(
    ISptLogger<WeatherGenerator> logger,
    TimeUtil timeUtil,
    WeatherHelper weatherHelper,
    ConfigServer configServer,
    WeightedRandomHelper weightedRandomHelper,
    RandomUtil randomUtil,
    IEnumerable<IWeatherPresetGenerator> weatherGenerators,
    ICloner cloner
)
{
    protected readonly WeatherConfig WeatherConfig = configServer.GetConfig<WeatherConfig>();

    /// <summary>
    /// Generate a weather object to send to client
    /// </summary>
    /// <param name="currentSeason">What season is weather being generated for</param>
    /// <param name="presetWeights">Weather preset weights to pick from</param>
    /// <param name="timestamp">Optional - Current time</param>
    /// <param name="previousPreset">Optional -What weather preset was last generated</param>
    /// <returns>Weather</returns>
    public Weather GenerateWeather(
        Season currentSeason,
        ref Dictionary<WeatherPreset, double> presetWeights,
        long? timestamp = null,
        WeatherPreset? previousPreset = null
    )
    {
        if (!presetWeights.Any())
        {
            // Get fresh cloned weights from config
            presetWeights = cloner.Clone(GetWeatherPresetWeightsBySeason(currentSeason));
        }

        // Only process when we have weights + there was previous preset chosen
        if (previousPreset.HasValue)
        {
            // We know last picked preset, Adjust weights
            // Make it less likely to be picked now
            presetWeights[previousPreset.Value] -= 1;
            logger.Info($"{previousPreset.Value} weight reduced by: 1 to: {presetWeights[previousPreset.Value]}");
        }

        // Assign value to previousPreset to be picked up next loop
        previousPreset = weightedRandomHelper.GetWeightedValue(presetWeights);
        logger.Warning($"Chose: {previousPreset}");

        // Check if chosen preset has been exhausted and reset if necessary
        if (presetWeights[previousPreset.Value] <= 0)
        {
            logger.Info($"{previousPreset.Value} is 0, resetting weights");
            // Flag for fresh presets
            presetWeights.Clear();
        }

        return GenerateWeatherByPreset(previousPreset.Value, timestamp);
    }

    public Dictionary<WeatherPreset, double> GetWeatherPresetWeightsBySeason(Season currentSeason)
    {
        return !WeatherConfig.Weather.WeatherPresetWeight.TryGetValue(currentSeason.ToString(), out var weights)
            ? WeatherConfig.Weather.WeatherPresetWeight.GetValueOrDefault("default")
            : weights;
    }

    protected Weather GenerateWeatherByPreset(WeatherPreset chosenPreset, long? timestamp)
    {
        var generator = weatherGenerators.FirstOrDefault(generator => generator.CanHandle(chosenPreset));
        if (generator is null)
        {
            logger.Warning($"Unable to find weather generator for: {chosenPreset}, falling back to sunny");

            generator = weatherGenerators.FirstOrDefault(generator => generator.CanHandle(WeatherPreset.SUNNY));
        }

        var presetWeights = GetWeatherWeightsByPreset(chosenPreset);
        var result = generator.Generate(presetWeights);

        // Set time values in result using now or passed in timestamp
        SetCurrentDateTime(result, timestamp);

        // Must occur after SetCurrentDateTime()
        result.Temperature = GetRaidTemperature(presetWeights, result.SptInRaidTimestamp ?? 0);

        // Needed by RaidWeatherService
        result.SptChosenPreset = chosenPreset;

        return result;
    }

    protected PresetWeights GetWeatherWeightsByPreset(WeatherPreset weatherPreset)
    {
        if (!WeatherConfig.Weather.PresetWeights.TryGetValue(weatherPreset.ToString(), out var value))
        {
            return WeatherConfig.Weather.PresetWeights["default"];
        }

        return value;
    }

    /// <summary>
    ///     Choose a temperature for the raid based on time of day
    /// </summary>
    /// <param name="weather"> What season Tarkov is currently in </param>
    /// <param name="inRaidTimestamp"> What time is the raid running at </param>
    /// <returns> Timestamp </returns>
    protected double GetRaidTemperature(PresetWeights weather, long inRaidTimestamp)
    {
        // Convert timestamp to date so we can get current hour and check if its day or night
        var currentRaidTime = new DateTime(inRaidTimestamp);
        var minMax = weatherHelper.IsHourAtNightTime(currentRaidTime.Hour) ? weather.Temp.Night : weather.Temp.Day;

        return Math.Round(randomUtil.GetDouble(minMax.Min, minMax.Max), 2);
    }

    /// <summary>
    ///     Set Weather date/time/timestamp values to now
    /// </summary>
    /// <param name="weather"> Object to update </param>
    /// <param name="timestamp"> Optional, timestamp used </param>
    protected void SetCurrentDateTime(Weather weather, long? timestamp = null)
    {
        var inRaidTime = timestamp is null ? weatherHelper.GetInRaidTime() : weatherHelper.GetInRaidTime(timestamp.Value);
        var normalTime = inRaidTime.GetBsgFormattedWeatherTime();
        var formattedDate = (timestamp.HasValue ? timeUtil.GetDateTimeFromTimeStamp(timestamp.Value) : DateTime.UtcNow).FormatToBsgDate();
        var datetimeBsgFormat = $"{formattedDate} {normalTime}";

        weather.Timestamp = timestamp ?? timeUtil.GetTimeStamp(); // matches weather.date
        weather.Date = formattedDate; // matches weather.timestamp
        weather.Time = datetimeBsgFormat; // matches weather.timestamp
        weather.SptInRaidTimestamp = weather.Timestamp;
    }
}
