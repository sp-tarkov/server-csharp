namespace SPTarkov.Server.Core.DI;

public static class OnLoadOrder
{
    public const int Database = 0;
    public const int GameCallbacks = 100;
    public const int PostDBModLoader = 200;
    public const int TraderRegistration = 300;
    public const int HandbookCallbacks = 400;
    public const int HttpCallbacks = 500;
    public const int SaveCallbacks = 600;
    public const int TraderCallbacks = 700;
    public const int PresetCallbacks = 800;
    public const int RagfairCallbacks = 900;
    public const int PostSptModLoader = 1000;
}
