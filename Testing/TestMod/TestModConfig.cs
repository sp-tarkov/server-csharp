using System.Text.Json.Serialization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Web.Models.Configs;
using SPTarkov.Server.Web.Services;

namespace TestMod;

[Injectable(InjectionType.Singleton)]
public class TestModConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("message")]
    public string Message { get; set; } = "Editable from the SPT config editor.";

    [JsonPropertyName("spawnMultiplier")]
    public double SpawnMultiplier { get; set; } = 1.25;

    [JsonPropertyName("allowedMaps")]
    public List<string> AllowedMaps { get; set; } = ["factory4_day", "bigmap"];

    [JsonPropertyName("features")]
    public Dictionary<string, bool> Features { get; set; } = new() { ["webConfigEditor"] = true, ["sampleFeature"] = false };
}

[Injectable(InjectionType.Singleton)]
public class TestModConfigEditorProvider(TestModConfig config) : IConfigEditorConfigProvider
{
    public IEnumerable<ConfigEditorConfigRegistration> GetConfigs()
    {
        yield return ConfigEditorConfigRegistration.Create(
            "com.sp-tarkov.test-mod",
            "Test Mod Config",
            config,
            Path.Combine("user", "mods", "TestMod", "config.json")
        );
    }
}
