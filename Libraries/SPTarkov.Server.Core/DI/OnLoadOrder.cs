namespace SPTarkov.Server.Core.DI;

public static class OnLoadOrder
{
    public const int Watermark = 0;
    public const int PreSptModLoader = 100000;
    public const int Database = 200000;
    public const int GameCallbacks = 300000;
    public const int PostDBModLoader = 400000;
    public const int TraderRegistration = 500000;
    public const int Routers = 600000;
    public const int HandbookCallbacks = 700000;
    public const int SaveCallbacks = 800000;
    public const int TraderCallbacks = 900000;
    public const int PresetCallbacks = 1000000;
    public const int RagfairCallbacks = 1100000;
    public const int PostSptModLoader = 1200000;
}
