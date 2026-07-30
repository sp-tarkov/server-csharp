namespace SPTarkov.Server.Core.DI;

public static class OnLoadOrder
{
    public const int Watermark = 0;
    public const int Preload = 100000;
    public const int GameCallbacks = 200000;
    public const int TraderRegistration = 300000;
    public const int Routers = 400000;
    public const int HandbookCallbacks = 500000;
    public const int SaveCallbacks = 600000;
    public const int TraderCallbacks = 700000;
    public const int PresetCallbacks = 800000;
    public const int RagfairCallbacks = 900000;
    public const int PostLoad = 1000000;
}
