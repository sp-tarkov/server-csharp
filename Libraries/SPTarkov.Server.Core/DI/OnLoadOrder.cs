namespace SPTarkov.Server.Core.DI;

public static class OnLoadOrder
{
    public const int Watermark = 0;
    public const int PreSptModLoader = 1000;
    public const int Database = 2000;
    public const int GameCallbacks = 3000;
    public const int PostDBModLoader = 4000;
    public const int TraderRegistration = 5000;
    public const int HandbookCallbacks = 6000;
    public const int HttpCallbacks = 7000;
    public const int SaveCallbacks = 8000;
    public const int TraderCallbacks = 9000;
    public const int PresetCallbacks = 10000;
    public const int RagfairCallbacks = 11000;
    public const int PostSptModLoader = 12000;
}
