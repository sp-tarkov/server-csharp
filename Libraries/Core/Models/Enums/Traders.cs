namespace Core.Models.Enums;

public static class Traders
{
    public const string PRAPOR = "54cb50c76803fa8b248b4571";
    public const string THERAPIST = "54cb57776803fa99248b456e";
    public const string FENCE = "579dc571d53a0658a154fbec";
    public const string SKIER = "58330581ace78e27b8b10cee";
    public const string PEACEKEEPER = "5935c25fb3acc3127c3d8cd9";
    public const string MECHANIC = "5a7c2eca46aef81a7ca2145d";
    public const string RAGMAN = "5ac3b934156ae10c4430e83c";
    public const string JAEGER = "5c0647fdd443bc2504c2d371";
    public const string LIGHTHOUSEKEEPER = "638f541a29ffd1183d187f57";
    public const string BTR = "656f0f98d80a697f855d34b1";
    public const string REF = "6617beeaa9cfa777ca915b7c";

    public static Dictionary<TradersEnum, string> TradersDictionary
    {
        get;
        set;
    } = new()
    {
        { TradersEnum.Prapor, "54cb50c76803fa8b248b4571" },
        { TradersEnum.Therapist, "54cb57776803fa99248b456e" },
        { TradersEnum.Fence, "579dc571d53a0658a154fbec" },
        { TradersEnum.Skier, "58330581ace78e27b8b10cee" },
        { TradersEnum.Peacekeeper, "5935c25fb3acc3127c3d8cd9" },
        { TradersEnum.Mechanic, "5a7c2eca46aef81a7ca2145d" },
        { TradersEnum.Ragman, "5ac3b934156ae10c4430e83c" },
        { TradersEnum.Jaeger, "5c0647fdd443bc2504c2d371" },
        { TradersEnum.LighthouseKeeper, "638f541a29ffd1183d187f57" },
        { TradersEnum.Btr, "656f0f98d80a697f855d34b1" },
        { TradersEnum.Ref, "6617beeaa9cfa777ca915b7c" }
    };
}

public enum TradersEnum
{
    Prapor,
    Therapist,
    Fence,
    Skier,
    Peacekeeper,
    Mechanic,
    Ragman,
    Jaeger,
    LighthouseKeeper,
    Btr,
    Ref
}
