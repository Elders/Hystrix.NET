namespace Netflix.Hystrix
{
    using System;

    public interface IHystrixCollapserProperties
    {
        IHystrixProperty<bool> RequestCachingEnabled { get; }
        IHystrixProperty<int> MaxRequestsInBatch { get; }
        IHystrixProperty<TimeSpan> TimerDelay { get; }
    }
}
