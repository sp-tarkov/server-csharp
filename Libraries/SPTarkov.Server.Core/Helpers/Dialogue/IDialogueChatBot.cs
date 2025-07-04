using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;

namespace SPTarkov.Server.Core.Helpers.Dialogue;

public interface IDialogueChatBot
{
    public UserDialogInfo GetChatBot();

    /// <summary>
    /// Handles messages for the chatbot. If a message can't be handled, <see cref="string.Empty"/> should be used.
    /// </summary>
    /// <returns>The response of the bot, or <see cref="string.Empty"/> if the request could not be handled.</returns>
    public ValueTask<string> HandleMessage(string sessionId, SendMessageRequest request);
}
