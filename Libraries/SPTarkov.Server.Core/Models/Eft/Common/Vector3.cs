using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

/// <summary>
/// This struct is used instead of <see cref="System.Numerics.Vector3"/>
/// so the JSON representation can use lowercase member names ("x", "y", "z").
/// </summary>
public readonly struct Vector3
{
    [JsonPropertyName("x")]
    public readonly float X;

    [JsonPropertyName("y")]
    public readonly float Y;

    [JsonPropertyName("z")]
    public readonly float Z;
}
