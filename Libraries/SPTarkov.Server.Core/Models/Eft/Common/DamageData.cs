using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record DamageData
{
    public int? Amount
    {
        get;
        set;
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BodyPartColliderType BodyPartColliderType
    {
        get;
        set;
    }
}
