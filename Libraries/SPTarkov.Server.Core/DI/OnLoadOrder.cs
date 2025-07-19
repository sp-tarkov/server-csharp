namespace SPTarkov.Server.Core.DI;

public static class OnLoadOrder
{
    public const int Watermark = 0;
    public const int Database = 1000;
    public const int GameCallbacks = 2000;
    public const int PostDBModLoader = 3000;
    public const int TraderRegistration = 4000;
    public const int HandbookCallbacks = 5000;
    public const int SaveCallbacks = 6000;
    public const int TraderCallbacks = 7000;
    public const int PresetCallbacks = 8000;
    public const int RagfairCallbacks = 9000;
    public const int PostSptModLoader = 10000;
}
