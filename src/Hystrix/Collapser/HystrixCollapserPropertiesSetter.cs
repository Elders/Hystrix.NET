namespace Netflix.Hystrix
{
    using System;

    public class HystrixCollapserPropertiesSetter
    {
        public bool? CollapsingEnabled { get; private set; }
        public int? MaxRequestsInBatch { get; private set; }
        public TimeSpan? TimerDelay { get; private set; }
        public bool? RequestCacheEnabled { get; private set; }

        public HystrixCollapserPropertiesSetter WithCollapsingEnabled(bool value)
        {
            CollapsingEnabled = value;
            return this;
        }
        public HystrixCollapserPropertiesSetter WithMaxRequestsInBatch(int value)
        {
            MaxRequestsInBatch = value;
            return this;
        }
        public HystrixCollapserPropertiesSetter WithTimerDelay(TimeSpan value)
        {
            TimerDelay = value;
            return this;
        }
        public HystrixCollapserPropertiesSetter WithRequestCacheEnabled(bool value)
        {
            RequestCacheEnabled = value;
            return this;
        }
    }
}
