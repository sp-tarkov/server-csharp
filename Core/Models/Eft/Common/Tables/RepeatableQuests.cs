using System.Text.Json.Serialization;

namespace Core.Models.Eft.Common.Tables;

public class RepeatableQuest : Quest
{
    [JsonPropertyName("changeCost")]
    public List<ChangeCost> ChangeCost { get; set; }
    
    [JsonPropertyName("changeStandingCost")]
    public int ChangeStandingCost { get; set; }
    
    [JsonPropertyName("sptRepatableGroupName")]
    public string SptRepatableGroupName { get; set; }
    
    [JsonPropertyName("acceptanceAndFinishingSource")]
    public string AcceptanceAndFinishingSource { get; set; }
    
    [JsonPropertyName("progressSource")]
    public string ProgressSource { get; set; }
    
    [JsonPropertyName("rankingModes")]
    public List<string> RankingModes { get; set; }
    
    [JsonPropertyName("gameModes")]
    public List<string> GameModes { get; set; }
    
    [JsonPropertyName("arenaLocations")]
    public List<string> ArenaLocations { get; set; }
    
    [JsonPropertyName("questStatus")]
    public RepeatableQuestStatus QuestStatus { get; set; }
}

public class RepeatableQuestDatabase
{
    [JsonPropertyName("templates")]
    public RepeatableTemplates Templates { get; set; }
    
    [JsonPropertyName("rewards")]
    public RewardOptions Rewards { get; set; }
    
    [JsonPropertyName("data")]
    public Options Data { get; set; }
    
    [JsonPropertyName("samples")]
    public List<SampleQuests> Samples { get; set; }
}

public class RepeatableQuestStatus
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    [JsonPropertyName("uid")]
    public string Uid { get; set; }
    
    [JsonPropertyName("qid")]
    public string Qid { get; set; }
    
    [JsonPropertyName("startTime")]
    public long StartTime { get; set; }
    
    [JsonPropertyName("status")]
    public int Status { get; set; }
    
    [JsonPropertyName("statusTimers")]
    public object StatusTimers { get; set; } // Use object for any type
}

public class RepeatableTemplates
{
    [JsonPropertyName("Elimination")]
    public Quest Elimination { get; set; }
    
    [JsonPropertyName("Completion")]
    public Quest Completion { get; set; }
    
    [JsonPropertyName("Exploration")]
    public Quest Exploration { get; set; }
}

public class PmcDataRepeatableQuest
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("unavailableTime")]
    public string? UnavailableTime { get; set; }
    
    [JsonPropertyName("activeQuests")]
    public List<RepeatableQuest> ActiveQuests { get; set; }
    
    [JsonPropertyName("inactiveQuests")]
    public List<RepeatableQuest> InactiveQuests { get; set; }
    
    [JsonPropertyName("endTime")]
    public long EndTime { get; set; }
    
    [JsonPropertyName("changeRequirement")]
    public Dictionary<string, ChangeRequirement> ChangeRequirement { get; set; }
    
    [JsonPropertyName("freeChanges")]
    public int FreeChanges { get; set; }
    
    [JsonPropertyName("freeChangesAvailable")]
    public int FreeChangesAvailable { get; set; }
}

public class ChangeRequirement
{
    [JsonPropertyName("changeCost")]
    public List<ChangeCost> ChangeCost { get; set; }
    
    [JsonPropertyName("changeStandingCost")]
    public int ChangeStandingCost { get; set; }
}

public class ChangeCost
{
    [JsonPropertyName("templateId")]
    public string TemplateId { get; set; }
    
    [JsonPropertyName("count")]
    public int Count { get; set; }
}

// Config Options

public class RewardOptions
{
    [JsonPropertyName("itemsBlacklist")]
    public List<string> ItemsBlacklist { get; set; }
}

public class Options
{
    [JsonPropertyName("Completion")]
    public CompletionFilter Completion { get; set; }
}

public class CompletionFilter
{
    [JsonPropertyName("itemsBlacklist")]
    public List<ItemsBlacklist> ItemsBlacklist { get; set; }
    
    [JsonPropertyName("itemsWhitelist")]
    public List<ItemsWhitelist> ItemsWhitelist { get; set; }
}

public class ItemsBlacklist
{
    [JsonPropertyName("minPlayerLevel")]
    public int MinPlayerLevel { get; set; }
    
    [JsonPropertyName("itemIds")]
    public List<string> ItemIds { get; set; }
}

public class ItemsWhitelist
{
    [JsonPropertyName("minPlayerLevel")]
    public int MinPlayerLevel { get; set; }
    
    [JsonPropertyName("itemIds")]
    public List<string> ItemIds { get; set; }
}

public class SampleQuests
{
    [JsonPropertyName("_id")]
    public string Id { get; set; }
    
    [JsonPropertyName("traderId")]
    public string TraderId { get; set; }
    
    [JsonPropertyName("location")]
    public string Location { get; set; }
    
    [JsonPropertyName("image")]
    public string Image { get; set; }
    
    [JsonPropertyName("type")]
    public string Type { get; set; }
    
    [JsonPropertyName("isKey")]
    public bool IsKey { get; set; }
    
    [JsonPropertyName("restartable")]
    public bool Restartable { get; set; }
    
    [JsonPropertyName("instantComplete")]
    public bool InstantComplete { get; set; }
    
    [JsonPropertyName("secretQuest")]
    public bool SecretQuest { get; set; }
    
    [JsonPropertyName("canShowNotificationsInGame")]
    public bool CanShowNotificationsInGame { get; set; }
    
    [JsonPropertyName("rewards")]
    public QuestRewards Rewards { get; set; }
    
    [JsonPropertyName("conditions")]
    public QuestConditionTypes Conditions { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("note")]
    public string Note { get; set; }
    
    [JsonPropertyName("description")]
    public string Description { get; set; }
    
    [JsonPropertyName("successMessageText")]
    public string SuccessMessageText { get; set; }
    
    [JsonPropertyName("failMessageText")]
    public string FailMessageText { get; set; }
    
    [JsonPropertyName("startedMessageText")]
    public string StartedMessageText { get; set; }
    
    [JsonPropertyName("templateId")]
    public string TemplateId { get; set; }
}