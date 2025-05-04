﻿using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Dialog;

public record RemoveMailMessageRequest : IRequestData
{
    [JsonPropertyName("dialogId")]
    public string? DialogId { get; set; }
}
