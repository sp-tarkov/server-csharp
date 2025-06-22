using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Enums;

[EftEnumConverter]
public enum RepeatableQuestType
{
    Elimination,
    Completion,
    Exploration,
    Pickup
}
