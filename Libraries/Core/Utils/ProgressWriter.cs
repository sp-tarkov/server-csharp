namespace Core.Utils;

public class ProgressWriter
{
    protected string? _barEmptyChar;
    protected string? _barFillChar;
    protected int? _maxBarLength;
    protected int _total;

    public ProgressWriter(int total, int maxBarLength, string barFillChar, string barEmptyChar)
    {
        _total = total;
        _maxBarLength = maxBarLength;
        _barFillChar = barFillChar;
        _barEmptyChar = barEmptyChar;
    }

    public ProgressWriter(int total)
    {
        _total = total;
        _maxBarLength = 25;
        _barFillChar = "\u2593";
        _barEmptyChar = "\u2591";
    }

    public void Increment()
    {
        // TODO - implement
    }
}
