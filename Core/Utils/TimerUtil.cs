using System.Diagnostics;
using Core.Annotations;

namespace Core.Utils
{
    [Injectable]
    public class TimerUtil
    {
        protected Stopwatch _stopwatch;

        public TimerUtil()
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        public int Stop(string unit = "sec")
        {
            _stopwatch.Stop();
            var timePassed = _stopwatch.Elapsed;

            return unit switch
            {
                "ns" => timePassed.Nanoseconds,
                "ms" => timePassed.Milliseconds,
                _ => timePassed.Seconds
            };
        }
    }
}
