using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public readonly struct Vector2
{
    [JsonPropertyName("x")]
    public readonly float X;

    [JsonPropertyName("y")]
    public readonly float Y;
}
