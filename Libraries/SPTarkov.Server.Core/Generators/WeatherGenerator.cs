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
    /// <param name="presetWeights">Weather preset weights to pick from (values will be altered when generating more than 1)</param>
    /// <param name="timestamp">Optional - Current time in millisecond ticks</param>
    /// <param name="previousPreset">Optional -What weather preset was last generated</param>
    /// <returns>A generated <see cref="Weather"/> object</returns>
    public Weather GenerateWeather(
        Season currentSeason,
        ref Dictionary<WeatherPreset, double> presetWeights,
        long? timestamp = null,
        WeatherPreset? previousPreset = null
    )
    {
        if (presetWeights.Count == 0)
        {
            // No presets, get fresh cloned weights from config
            presetWeights = cloner.Clone(GetWeatherPresetWeightsBySeason(currentSeason));
        }

        // Only process when we have weights + there was previous preset chosen
        if (previousPreset.HasValue && presetWeights.ContainsKey(previousPreset.Value))
        {
            // We know last picked preset, Adjust weights
            // Make it less likely to be picked now
            // Clamp to 0
            presetWeights[previousPreset.Value] = Math.Max(0, presetWeights[previousPreset.Value] - 1);
        }

        // Assign value to previousPreset to be picked up next loop
        previousPreset = weightedRandomHelper.GetWeightedValue(presetWeights);

        // Check if chosen preset has been exhausted and reset if necessary
        if (presetWeights[previousPreset.Value] == 0)
        {
            // Flag for fresh presets
            presetWeights.Clear();
        }

        return GenerateWeatherByPreset(previousPreset.Value, timestamp);
    }

    /// <summary>
    /// Gets weather property weights for the provided season
    /// </summary>
    /// <param name="currentSeason">Desired season to get weights for</param>
    /// <returns>A dictionary of weather preset weights</returns>
    public Dictionary<WeatherPreset, double> GetWeatherPresetWeightsBySeason(Season currentSeason)
    {
        return WeatherConfig.Weather.WeatherPresetWeight.TryGetValue(currentSeason.ToString(), out var weights)
            ? weights
            : WeatherConfig.Weather.WeatherPresetWeight.GetValueOrDefault("default")!;
    }

    /// <summary>
    /// Creates a <see cref="Weather"/> object that adheres to the chosen preset
    /// </summary>
    /// <param name="chosenPreset">The weather preset chosen to generate</param>
    /// <param name="timestamp">OPTIONAL - generate the weather object with a specific time instead of now</param>
    /// <returns>A generated <see cref="Weather"/> object</returns>
    protected Weather GenerateWeatherByPreset(WeatherPreset chosenPreset, long? timestamp)
    {
        var generator = weatherGenerators.FirstOrDefault(gen => gen.CanHandle(chosenPreset));
        if (generator is null)
        {
            logger.Warning($"Unable to find weather generator for: {chosenPreset}, falling back to sunny");

            generator = weatherGenerators.FirstOrDefault(gen => gen.CanHandle(WeatherPreset.SUNNY));
        }

        var presetWeights = GetWeatherWeightsByPreset(chosenPreset);
        var result = generator.Generate(presetWeights);

        // Set time values in result using now or passed in timestamp
        SetCurrentDateTime(result, timestamp);

        // Must occur after SetCurrentDateTime(), temp depends on timestamp
        result.Temperature = GetRaidTemperature(presetWeights, result.SptInRaidTimestamp ?? 0);

        // Needed by RaidWeatherService
        result.SptChosenPreset = chosenPreset;

        return result;
    }

    /// <summary>
    /// Get the weather preset weights based on passed in preset, get defaults if preset not found in config
    /// </summary>
    /// <param name="weatherPreset">Desired preset</param>
    /// <returns>PresetWeights</returns>
    protected PresetWeights GetWeatherWeightsByPreset(WeatherPreset weatherPreset)
    {
        return WeatherConfig.Weather.PresetWeights.TryGetValue(weatherPreset.ToString(), out var value)
            ? value
            : WeatherConfig.Weather.PresetWeights["default"];
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
