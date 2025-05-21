namespace SPTarkov.Server.Core.Models.Enums;

public enum MessageType
{
    UserMessage = 1,
    NpcTraderMessage = 2,
    AuctionMessage = 3,
    FleamarketMessage = 4,
    AdminMessage = 5,
    GroupChatMessage = 6,
    SystemMessage = 7,
    InsuranceReturn = 8,
    GlobalChat = 9,
    QuestStart = 10,
    QuestFail = 11,
    QuestSuccess = 12,
    MessageWithItems = 13,
    InitialSupport = 14,
    BtrItemsDelivery = 15,
}
