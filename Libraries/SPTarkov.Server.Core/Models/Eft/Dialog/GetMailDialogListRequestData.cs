using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Dialog;

public record GetMailDialogListRequestData : IRequestData
{
    [JsonPropertyName("limit")]
    public int? Limit
    {
        get;
        set;
    }

    [JsonPropertyName("offset")]
    public int? Offset
    {
        get;
        set;
    }
}
