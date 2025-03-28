using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record CustomisationStorage
{
    // Customisation.json/itemId
    [JsonPropertyName("id")]
    public string Id
    {
        get;
        set;
    }

    [JsonPropertyName("source")]
    public string? Source
    {
        get;
        set;
    }

    [JsonPropertyName("type")]
    public string? Type
    {
        get;
        set;
    }
}

public record CustomisationType
{
    public const string SUITE = "suite";
    public const string DOG_TAG = "dogTag";
    public const string HEAD = "head";
    public const string VOICE = "voice";
    public const string GESTURE = "gesture";
    public const string ENVIRONMENT = "environment";
    public const string WALL = "wall";
    public const string FLOOR = "floor";
    public const string CEILING = "ceiling";
    public const string LIGHT = "light";
    public const string SHOOTING_RANGE_MARK = "shootingRangeMark";
    public const string CAT = "cat";
    public const string MANNEQUIN_POSE = "mannequinPose";
}

public record CustomisationTypeId
{
    public const string CUSTOMIZATION = "5cbdb4a2e2b501000d352ae2";
    public const string BODY_PARTS = "5cd943c31388ce000a659df5";
    public const string BODY = "5cc0868e14c02e000c6bea68";
    public const string FEET = "5cc0869814c02e000a4cad94";
    public const string HANDS = "5cc086a314c02e000c6bea69";
    public const string HEAD = "5cc085e214c02e000c6bea67";
    public const string SUITS = "5cd943b21388ce03a44dc2a2";
    public const string LOWER = "5cd944d01388ce000a659df9";
    public const string UPPER = "5cd944ca1388ce03a44dc2a4";
    public const string DOG_TAGS = "6746fafabafff8500804880e";
    public const string VOICE = "5fc100cf95572123ae738483";
    public const string GESTURES = "6751848eba5968fd800a01d6";
    public const string ENVIRONMENT_UI = "67584ea0ff58ff0e7909e435";
    public const string FLOOR = "67373f170eca6e03ab0d5391";
    public const string ITEM_SLOT = "67373f520eca6e03ab0d5397";
    public const string LIGHT = "67373f286cadad262309e862";
    public const string POSTER_SLOT = "67373f4b5a5ee73f2a081bb3";
    public const string SHOOTING_RANGE_MARK = "67373f330eca6e03ab0d5394";
    public const string WALL = "67373f1e5a5ee73f2a081baf";
    public const string HIDEOUT = "67373ef90eca6e03ab0d538c";
    public const string CEILING = "673b3f595bf6b605c90fcdc2";
    public const string MANNEQUIN_POSE = "675ff48ce8d2356707079617";
}

public record CustomisationSource
{
    public const string QUEST = "quest";
    public const string PRESTIGE = "prestige";
    public const string ACHIEVEMENT = "achievement";
    public const string UNLOCKED_IN_GAME = "unlockedInGame";
    public const string PAID = "paid";
    public const string DROP = "drop";
    public const string DEFAULT = "default";
}
