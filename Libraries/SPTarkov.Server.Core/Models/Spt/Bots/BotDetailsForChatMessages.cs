using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Models.Spt.Bots;

public record BotDetailsForChatMessages
{
    public string Nickname
    {
        get;
        set;
    }

    public DogtagSide Side
    {
        get;
        set;
    }

    public int? Aid
    {
        get;
        set;
    }

    public int? Level
    {
        get;
        set;
    }

    public MemberCategory? Type
    {
        get;
        set;
    }
}
