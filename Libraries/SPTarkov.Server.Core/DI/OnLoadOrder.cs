namespace SPTarkov.Server.Core.DI;

public static class OnLoadOrder
{
    public const int Watermark = 0;
    public const int PreSptModLoader = 10000;
    public const int Database = 20000;
    public const int GameCallbacks = 30000;
    public const int PostDBModLoader = 40000;
    public const int TraderRegistration = 50000;
    public const int HandbookCallbacks = 60000;
    public const int SaveCallbacks = 70000;
    public const int TraderCallbacks = 80000;
    public const int PresetCallbacks = 90000;
    public const int RagfairCallbacks = 100000;
    public const int PostSptModLoader = 110000;
}
