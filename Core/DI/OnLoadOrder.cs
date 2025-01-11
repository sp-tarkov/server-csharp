namespace Core.DI;

public static class OnLoadOrder
{
    public const int Database = 0;
    public const int PostDBModLoader = 1;
    public const int HandbookCallbacks = 2;
    public const int HttpCallbacks = 3;
    public const int PresetCallbacks = 4;
    public const int SaveCallbacks = 5;
    public const int TraderCallbacks = 6;
    public const int RagfairPriceService = 7;
    public const int RagfairCallbacks = 8;
    public const int ModCallbacks = 9;
    public const int GameCallbacks = 10;
}
