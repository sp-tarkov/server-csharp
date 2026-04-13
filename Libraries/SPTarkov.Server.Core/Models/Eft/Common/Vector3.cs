using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

/// <summary>
/// This struct is used instead of <see cref="System.Numerics.Vector3"/>
/// so the JSON representation can use lowercase member names ("x", "y", "z").
/// </summary>
public readonly struct Vector3
{
    [JsonPropertyName("x")]
    public float X { get; init; }

    [JsonPropertyName("y")]
    public float Y { get; init; }

    [JsonPropertyName("z")]
    public float Z { get; init; }

    [JsonConstructor]
    public Vector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }
}
