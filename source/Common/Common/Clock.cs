using System;
using System.Diagnostics;

namespace AspNetSkeleton.Common
{
    public interface IClock
    {
        DateTime UtcNow { get; }
        long TimestampTicks { get; }
        long TicksPerSecond { get; }
    }

    public class Clock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;

        public long TimestampTicks => Stopwatch.GetTimestamp();

        public long TicksPerSecond => Stopwatch.Frequency;
    }
}
