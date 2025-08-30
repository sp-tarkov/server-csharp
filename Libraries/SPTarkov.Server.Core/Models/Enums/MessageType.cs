namespace SPTarkov.Server.Core.Models.Enums;

public enum MessageType
{
    UserMessage = 1,
    NpcTraderMessage,
    AuctionMessage,
    FleamarketMessage,
    AdminMessage,
    GroupChatMessage,
    SystemMessage,
    InsuranceReturn,
    GlobalChat,
    QuestStart,
    QuestFail,
    QuestSuccess,
    MessageWithItems,
    InitialSupport,
    BtrItemsDelivery,
}
