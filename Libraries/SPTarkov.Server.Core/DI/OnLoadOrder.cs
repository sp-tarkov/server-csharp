namespace SPTarkov.Server.Core.DI;

public static class OnLoadOrder
{
    public const int PreSPTDatabase = 0;
    public const int Database = 1;
    public const int PostSptDatabase = 2;
    public const int GameCallbacks = 100;
    public const int PostDBModLoader = 200;
    public const int HandbookCallbacks = 300;
    public const int HttpCallbacks = 400;
    public const int SaveCallbacks = 500;
    public const int TraderCallbacks = 600;
    public const int PostSptModLoader = 700;
    public const int PresetCallbacks = 800;
    public const int RagfairPriceService = 900;
    public const int RagfairCallbacks = 1000;
    public const int PostServerLoad = 9999;
}
