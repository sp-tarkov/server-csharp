namespace Core.DI;

public static class OnLoadOrder
{
    public const int Database = 0;
    public const int PostDBModLoader = 1;
    public const int HandbookCallbacks = 2;
    public const int PresetCallbacks = 3;
    public const int SaveCallbacks = 4;
    public const int TraderCallbacks = 5;
    public const int RagfairPriceService = 6;
    public const int RagfairCallbacks = 7;
    public const int ModCallbacks = 8;
    public const int GameCallbacks = 9;
    public const int HttpCallbacks = 10;
}
