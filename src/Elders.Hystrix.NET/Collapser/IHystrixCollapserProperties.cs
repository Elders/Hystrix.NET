namespace Elders.Hystrix.NET
{
    using System;

    public interface IHystrixCollapserProperties
    {
        IHystrixProperty<bool> RequestCachingEnabled { get; }
        IHystrixProperty<int> MaxRequestsInBatch { get; }
        IHystrixProperty<TimeSpan> TimerDelay { get; }
    }
}
