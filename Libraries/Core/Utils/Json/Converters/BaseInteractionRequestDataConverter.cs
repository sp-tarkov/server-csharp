using System.Text.Json;
using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Request;
using Core.Models.Eft.Customization;

namespace Core.Utils.Json.Converters;

public class BaseInteractionRequestDataConverter : JsonConverter<BaseInteractionRequestData>
{
    public override BaseInteractionRequestData? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDocument = JsonDocument.ParseValue(ref reader);
        var jsonText = jsonDocument.RootElement.GetRawText();
        var value = JsonSerializer.Deserialize<BaseInteractionRequestData>(jsonText, options);
        return ConvertToCorrectType(value, jsonText, options);
    }

    private BaseInteractionRequestData? ConvertToCorrectType(BaseInteractionRequestData? value, string jsonText, JsonSerializerOptions options)
    {
        switch (value.Action)
        {
            case "CustomizationBuy":
                return JsonSerializer.Deserialize<BuyClothingRequestData>(jsonText, options);
            case "CustomizationSet":
                return JsonSerializer.Deserialize<CustomizationSetRequest>(jsonText, options);
            
            /////////////////////////////////////////// 
            
            
        }
    }

    public override void Write(Utf8JsonWriter writer, BaseInteractionRequestData value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}
