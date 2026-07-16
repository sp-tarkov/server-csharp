using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Spt.Config;

namespace SPTarkov.Server.Core.Migration.Migrations;

/// <summary>
/// Chatbots used to share a single hardcoded AID (1234566).
/// This broke certain UI elements as they didn't expect multiple users with the same id
/// </summary>
[Injectable]
public sealed class FixChatBotAids(CoreConfig coreConfig) : AbstractProfileMigration
{
    public override string MigrationName
    {
        get { return "FixChatBotAids410"; }
    }

    public override bool CanMigrate(JsonObject profile, IEnumerable<IProfileMigration> previouslyRanMigrations)
    {
        return GetStaleBotUsers(profile).Any();
    }

    public override JsonObject? Migrate(JsonObject profile)
    {
        foreach (var (user, expectedAid) in GetStaleBotUsers(profile))
        {
            user["aid"] = expectedAid;
        }

        return base.Migrate(profile);
    }

    /// <summary>
    /// Find every persisted chatbot user entry whose AID doesn't match the AID currently configured for that bot.
    /// </summary>
    private IEnumerable<(JsonObject user, int expectedAid)> GetStaleBotUsers(JsonObject profile)
    {
        if (!profile.TryGetObject(out var dialogues, "dialogues"))
        {
            yield break;
        }

        var chatbotFeatures = coreConfig.Features.ChatbotFeatures;
        foreach (var (botKey, botId) in chatbotFeatures.Ids)
        {
            if (!chatbotFeatures.Aids.TryGetValue(botKey, out var expectedAid))
            {
                continue;
            }

            if (!dialogues.TryGetObject(out var dialogue, botId.ToString()) || !dialogue.TryGetArray(out var users, "Users"))
            {
                continue;
            }

            // Only touch the bot's own entry, the player is also listed in this dialogue's Users
            foreach (var user in users)
            {
                if (
                    user is JsonObject userObject
                    && userObject.TryGetValue<string>(out var userId, "_id")
                    && userId == botId.ToString()
                    && (!userObject.TryGetValue<int>(out var userAid, "aid") || userAid != expectedAid)
                )
                {
                    yield return (userObject, expectedAid);
                }
            }
        }
    }
}
