namespace Core.Utils
{
    public class ProgressWriter
    {
        private readonly int _total;
        private readonly int? _maxBarLength;
        private readonly string? _barFillChar;
        private readonly string? _barEmptyChar;

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
            _maxBarLength  = 25;
            _barFillChar  = "\u2593";
            _barEmptyChar = "\u2591";
        }

        public void Increment()
        {
            // TODO - implement
        }
    }


}
