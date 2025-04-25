using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record UpdSight
{
    [JsonPropertyName("ScopesCurrentCalibPointIndexes")]
    public List<int>? ScopesCurrentCalibPointIndexes
    {
        get;
        set;
    }

    [JsonPropertyName("ScopesSelectedModes")]
    public List<int>? ScopesSelectedModes
    {
        get;
        set;
    }

    [JsonPropertyName("SelectedScope")]
    public int? SelectedScope
    {
        get;
        set;
    }

    public double? ScopeZoomValue
    {
        get;
        set;
    }
}
