namespace Hystrix.Test
{
    using System;
    using Netflix.Hystrix;
    using Netflix.Hystrix.ThreadPool;

    internal class MockingHystrixThreadPoolProperties : IHystrixThreadPoolProperties
    {
        private readonly HystrixThreadPoolPropertiesSetter setter;

        public MockingHystrixThreadPoolProperties(HystrixThreadPoolPropertiesSetter setter)
        {
            if (setter == null)
                throw new ArgumentNullException("setter");

            this.setter = setter;
        }

        public IHystrixProperty<int> CoreSize
        {
            get { return HystrixPropertyFactory.AsProperty(this.setter.CoreSize.Value); }
        }
        public IHystrixProperty<TimeSpan> KeepAliveTime
        {
            get { return HystrixPropertyFactory.AsProperty(this.setter.KeepAliveTime.Value); }
        }
        public IHystrixProperty<int> MaxQueueSize
        {
            get { return HystrixPropertyFactory.AsProperty(this.setter.MaxQueueSize.Value); }
        }
        public IHystrixProperty<int> QueueSizeRejectionThreshold
        {
            get { return HystrixPropertyFactory.AsProperty(this.setter.QueueSizeRejectionThreshold.Value); }
        }
        public IHystrixProperty<int> MetricsRollingStatisticalWindowInMilliseconds
        {
            get { return HystrixPropertyFactory.AsProperty(this.setter.MetricsRollingStatisticalWindowInMilliseconds.Value); }
        }
        public IHystrixProperty<int> MetricsRollingStatisticalWindowBuckets
        {
            get { return HystrixPropertyFactory.AsProperty(this.setter.MetricsRollingStatisticalWindowBuckets.Value); }
        }
    }
}
