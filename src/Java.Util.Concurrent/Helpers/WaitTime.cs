using System;

namespace Java.Util.Concurrent.Helpers
{
    internal class WaitTime
    {
	    internal static readonly TimeSpan MaxValue = TimeSpan.FromMilliseconds(int.MaxValue);
	    internal static readonly TimeSpan Forever = TimeSpan.FromMilliseconds(-1);

        internal static TimeSpan Cap(TimeSpan waitTime)
        {
            return waitTime > MaxValue ? MaxValue : waitTime;
        }

        internal static DateTime Deadline(TimeSpan waitTime)
        {
            var now = DateTime.UtcNow;
            return (DateTime.MaxValue - now < waitTime) ? DateTime.MaxValue : now.Add(waitTime);
        }
    }
}
