namespace Netflix.Hystrix.Strategy.Properties
{
    using System;

    public class HystrixPropertiesCollapserDefault : IHystrixCollapserProperties
    {
        private const int DefaultMaxRequestsInBatch = Int32.MaxValue;
        private static readonly TimeSpan DefaultTimerDelay = TimeSpan.FromMilliseconds(10);
        private const bool DefaultRequestCacheEnabled = true;

        public IHystrixProperty<bool> RequestCachingEnabled { get; private set; }
        public IHystrixProperty<int> MaxRequestsInBatch { get; private set; }
        public IHystrixProperty<TimeSpan> TimerDelay { get; private set; }

        public HystrixPropertiesCollapserDefault(HystrixCollapserPropertiesSetter setter)
        {
            RequestCachingEnabled = HystrixPropertyFactory.AsProperty(setter.RequestCacheEnabled, DefaultRequestCacheEnabled);
            MaxRequestsInBatch = HystrixPropertyFactory.AsProperty(setter.MaxRequestsInBatch, DefaultMaxRequestsInBatch);
            TimerDelay = HystrixPropertyFactory.AsProperty(setter.TimerDelay, DefaultTimerDelay);
        }
    }
}
