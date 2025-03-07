namespace SPTarkov.Server.Core.Models.Enums;

public record Money
{
    public const string ROUBLES = "5449016a4bdc2d6f028b456f";
    public const string EUROS = "569668774bdc2da2298b4568";
    public const string DOLLARS = "5696686a4bdc2da3298b456a";
    public const string GP = "5d235b4d86f7742e017bc88a";

    public static HashSet<string> GetMoneyTpls()
    {
        return [ROUBLES, EUROS, DOLLARS, GP];
    }
}
