namespace Elders.Hystrix.NET.Test
{
    using System;
    using Elders.Hystrix.NET;

    internal class MockingHystrixCommandProperties : IHystrixCommandProperties
    {
        private readonly HystrixCommandPropertiesSetter setter;

        public MockingHystrixCommandProperties(HystrixCommandPropertiesSetter setter)
        {
            if (setter == null)
                throw new ArgumentNullException("setter");

            this.setter = setter;
        }

        public IHystrixProperty<bool> CircuitBreakerEnabled
        {
            get { return HystrixPropertyFactory.AsProperty(this.setter.CircuitBreakerEnabled.Value); }
        }
        public IHystrixProperty<int> CircuitBreakerErrorThresholdPercentage
        {
            get { return HystrixPropertyFactory.AsProperty(this.setter.CircuitBreakerErrorThresholdPercentage.Value); }
        }
        public IHystrixProperty<bool> CircuitBreakerForceClosed
        {
            get { return HystrixPropertyFactory.AsProperty(this.setter.CircuitBreakerForceClosed.Value); }
        }
        public IHystrixProperty<bool> CircuitBreakerForceOpen
        {
            get { return HystrixPropertyFactory.AsProperty(this.setter.CircuitBreakerForceOpen.Value); }
        }
        public IHystrixProperty<int> CircuitBreakerRequestVolumeThreshold
        {
            get { return HystrixPropertyFactory.AsProperty(this.setter.CircuitBreakerRequestVolumeThreshold.Value); }
        }
        public IHystrixProperty<TimeSpan> CircuitBreakerSleepWindow
        {
            get { return HystrixPropertyFactory.AsProperty(this.setter.CircuitBreakerSleepWindow.Value); }
        }
        public IHystrixProperty<int> ExecutionIsolationSemaphoreMaxConcurrentRequests
        {
            get { return HystrixPropertyFactory.AsProperty(this.setter.ExecutionIsolationSemaphoreMaxConcurrentRequests.Value); }
        }
        public IHystrixProperty<ExecutionIsolationStrategy> ExecutionIsolationStrategy
        {
            get { return HystrixPropertyFactory.AsProperty(this.setter.ExecutionIsolationStrategy.Value); }
        }
        public IHystrixProperty<bool> ExecutionIsolationThreadInterruptOnTimeout
        {
            get { return HystrixPropertyFactory.AsProperty(this.setter.ExecutionIsolationThreadInterruptOnTimeout.Value); }
        }
        public IHystrixProperty<string> ExecutionIsolationThreadPoolKeyOverride
        {
            get { return HystrixPropertyFactory.NullProperty<string>(); }
        }
        public IHystrixProperty<TimeSpan> ExecutionIsolationThreadTimeout
        {
            get { return HystrixPropertyFactory.AsProperty(this.setter.ExecutionIsolationThreadTimeout.Value); }
        }
        public IHystrixProperty<int> FallbackIsolationSemaphoreMaxConcurrentRequests
        {
            get { return HystrixPropertyFactory.AsProperty(this.setter.FallbackIsolationSemaphoreMaxConcurrentRequests.Value); }
        }
        public IHystrixProperty<bool> FallbackEnabled
        {
            get { return HystrixPropertyFactory.AsProperty(this.setter.FallbackEnabled.Value); }
        }
        public IHystrixProperty<TimeSpan> MetricsHealthSnapshotInterval
        {
            get { return HystrixPropertyFactory.AsProperty(this.setter.MetricsHealthSnapshotInterval.Value); }
        }
        public IHystrixProperty<int> MetricsRollingPercentileBucketSize
        {
            get { return HystrixPropertyFactory.AsProperty(this.setter.MetricsRollingPercentileBucketSize.Value); }
        }
        public IHystrixProperty<bool> MetricsRollingPercentileEnabled
        {
            get { return HystrixPropertyFactory.AsProperty(this.setter.MetricsRollingPercentileEnabled.Value); }
        }
        public IHystrixProperty<int> MetricsRollingPercentileWindowInMilliseconds
        {
            get { return HystrixPropertyFactory.AsProperty(this.setter.MetricsRollingPercentileWindowInMilliseconds.Value); }
        }
        public IHystrixProperty<int> MetricsRollingPercentileWindowBuckets
        {
            get { return HystrixPropertyFactory.AsProperty(this.setter.MetricsRollingPercentileWindowBuckets.Value); }
        }
        public IHystrixProperty<int> MetricsRollingStatisticalWindowInMilliseconds
        {
            get { return HystrixPropertyFactory.AsProperty(this.setter.MetricsRollingStatisticalWindowInMilliseconds.Value); }
        }
        public IHystrixProperty<int> MetricsRollingStatisticalWindowBuckets
        {
            get { return HystrixPropertyFactory.AsProperty(this.setter.MetricsRollingStatisticalWindowBuckets.Value); }
        }
        public IHystrixProperty<bool> RequestCacheEnabled
        {
            get { return HystrixPropertyFactory.AsProperty(this.setter.RequestCacheEnabled.Value); }
        }
        public IHystrixProperty<bool> RequestLogEnabled
        {
            get { return HystrixPropertyFactory.AsProperty(this.setter.RequestLogEnabled.Value); }
        }
    }
}
