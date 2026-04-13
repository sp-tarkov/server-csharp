using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

/// <summary>
/// This struct is used instead of <see cref="System.Numerics.Vector2"/>
/// so the JSON representation can use lowercase member names ("x", "y").
/// </summary>
public readonly struct Vector2
{
    [JsonPropertyName("x")]
    public readonly float X;

    [JsonPropertyName("y")]
    public readonly float Y;
}
