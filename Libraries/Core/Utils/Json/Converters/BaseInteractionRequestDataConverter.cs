using System.Text.Json;
using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Request;
using Core.Models.Eft.Customization;
using Core.Models.Eft.Health;
using Core.Models.Eft.Hideout;
using Core.Models.Eft.Insurance;
using Core.Models.Enums;

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
            case "Eat":
                return JsonSerializer.Deserialize<OffraidEatRequestData>(jsonText, options);
            case "Heal":
                return JsonSerializer.Deserialize<OffraidHealRequestData>(jsonText, options);
            case "RestoreHealth":
                return JsonSerializer.Deserialize<HealthTreatmentRequestData>(jsonText, options);
            case HideoutEventActions.HIDEOUT_UPGRADE:
                return JsonSerializer.Deserialize<HideoutUpgradeRequestData>(jsonText, options);
            case HideoutEventActions.HIDEOUT_UPGRADE_COMPLETE:
                return JsonSerializer.Deserialize<HideoutUpgradeCompleteRequestData>(jsonText, options);
            case HideoutEventActions.HIDEOUT_PUT_ITEMS_IN_AREA_SLOTS:
                return JsonSerializer.Deserialize<HideoutPutItemInRequestData>(jsonText, options);
            case HideoutEventActions.HIDEOUT_TAKE_ITEMS_FROM_AREA_SLOTS:
                return JsonSerializer.Deserialize<HideoutTakeItemOutRequestData>(jsonText, options);
            case HideoutEventActions.HIDEOUT_TOGGLE_AREA:
                return JsonSerializer.Deserialize<HideoutToggleAreaRequestData>(jsonText, options);
            case HideoutEventActions.HIDEOUT_SINGLE_PRODUCTION_START:
                return JsonSerializer.Deserialize<HideoutSingleProductionStartRequestData>(jsonText, options);
            case HideoutEventActions.HIDEOUT_SCAV_CASE_PRODUCTION_START:
                return JsonSerializer.Deserialize<HideoutScavCaseStartRequestData>(jsonText, options);
            case HideoutEventActions.HIDEOUT_CONTINUOUS_PRODUCTION_START:
                return JsonSerializer.Deserialize<HideoutContinuousProductionStartRequestData>(jsonText, options);
            case HideoutEventActions.HIDEOUT_TAKE_PRODUCTION:
                return JsonSerializer.Deserialize<HideoutTakeProductionRequestData>(jsonText, options);
            case HideoutEventActions.HIDEOUT_RECORD_SHOOTING_RANGE_POINTS:
                return JsonSerializer.Deserialize<RecordShootingRangePoints>(jsonText, options);
            case HideoutEventActions.HIDEOUT_IMPROVE_AREA:
                return JsonSerializer.Deserialize<HideoutImproveAreaRequestData>(jsonText, options);
            case HideoutEventActions.HIDEOUT_CANCEL_PRODUCTION_COMMAND:
                return JsonSerializer.Deserialize<HideoutImproveAreaRequestData>(jsonText, options);
            case HideoutEventActions.HIDEOUT_CIRCLE_OF_CULTIST_PRODUCTION_START:
                return JsonSerializer.Deserialize<HideoutCircleOfCultistProductionStartRequestData>(jsonText, options);
            case HideoutEventActions.HIDEOUT_DELETE_PRODUCTION_COMMAND:
                return JsonSerializer.Deserialize<HideoutDeleteProductionRequestData>(jsonText, options);
            case HideoutEventActions.HIDEOUT_CUSTOMIZATION_APPLY_COMMAND:
                return JsonSerializer.Deserialize<HideoutCustomizationApplyRequestData>(jsonText, options);
            case HideoutEventActions.HIDEOUT_CUSTOMIZATION_SET_MANNEQUIN_POSE:
                return JsonSerializer.Deserialize<HideoutCustomizationSetMannequinPoseRequest>(jsonText, options);
            case "Insure":
                return JsonSerializer.Deserialize<InsureRequestData>(jsonText, options);
            
            /////////////////////////////////////////// 
            
            
        }
    }

    public override void Write(Utf8JsonWriter writer, BaseInteractionRequestData value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}
