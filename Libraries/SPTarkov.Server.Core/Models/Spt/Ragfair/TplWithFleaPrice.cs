using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Spt.Ragfair;

public record TplWithFleaPrice
{
    [JsonPropertyName("tpl")]
    public MongoId Tpl { get; set; }

    /// <summary>
    ///     Roubles
    /// </summary>
    [JsonPropertyName("price")]
    public double Price { get; set; }
}
