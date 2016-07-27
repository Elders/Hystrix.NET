namespace Elders.Hystrix.NET.Strategy.Properties
{
    using System;

    public class HystrixPropertiesCommandDefault : IHystrixCommandProperties
    {
        private const int DefaultMetricsRollingStatisticalWindowInMilliseconds = 10000;// default => statisticalWindow: 10000 = 10 seconds (and default of 10 buckets so each bucket is 1 second)
        private const int DefaultMetricsRollingStatisticalWindowBuckets = 10;// default => statisticalWindowBuckets: 10 = 10 buckets in a 10 second window so each bucket is 1 second
        private const int DefaultCircuitBreakerRequestVolumeThreshold = 20;// default => statisticalWindowVolumeThreshold: 20 requests in 10 seconds must occur before statistics matter
        private static readonly TimeSpan DefaultCircuitBreakerSleepWindow = TimeSpan.FromSeconds(5.0);// default => sleepWindow: 5000 = 5 seconds that we will sleep before trying again after tripping the circuit
        private const int DefaultCircuitBreakerErrorThresholdPercentage = 50;// default => errorThresholdPercentage = 50 = if 50%+ of requests in 10 seconds are failures or latent when we will trip the circuit
        private const bool DefaultCircuitBreakerForceOpen = false;// default => forceCircuitOpen = false (we want to allow traffic)
        private const bool DefaultCircuitBreakerForceClosed = false;// default => ignoreErrors = false 
        private static readonly TimeSpan DefaultExecutionIsolationThreadTimeout = TimeSpan.FromSeconds(1.0); // default => executionTimeoutInMilliseconds: 1000 = 1 second
        private const ExecutionIsolationStrategy DefaultExecutionIsolationStrategy = Elders.Hystrix.NET.ExecutionIsolationStrategy.Thread;
        private const bool DefaultExecutionIsolationThreadInterruptOnTimeout = true;
        private const bool DefaultMetricsRollingPercentileEnabled = true;
        private const bool DefaultRequestCacheEnabled = true;
        private const int DefaultFallbackIsolationSemaphoreMaxConcurrentRequests = 10;
        private const bool DefaultFallbackEnabled = true;
        private const int DefaultExecutionIsolationSemaphoreMaxConcurrentRequests = 10;
        private const bool DefaultRequestLogEnabled = true;
        private const bool DefaultCircuitBreakerEnabled = true;
        private const int DefaultMetricsRollingPercentileWindowInMilliseconds = 60000; // default to 1 minute for RollingPercentile 
        private const int DefaultMetricsRollingPercentileWindowBuckets = 6; // default to 6 buckets (10 seconds each in 60 second window)
        private const int DefaultMetricsRollingPercentileBucketSize = 100; // default to 100 values max per bucket
        private static readonly TimeSpan DefaultMetricsHealthSnapshotInterval = TimeSpan.FromSeconds(0.5); // default to 500ms as max frequency between allowing snapshots of health (error percentage etc)

        public IHystrixProperty<bool> CircuitBreakerEnabled { get; private set; }
        public IHystrixProperty<int> CircuitBreakerErrorThresholdPercentage { get; private set; }
        public IHystrixProperty<bool> CircuitBreakerForceClosed { get; private set; }
        public IHystrixProperty<bool> CircuitBreakerForceOpen { get; private set; }
        public IHystrixProperty<int> CircuitBreakerRequestVolumeThreshold { get; private set; }
        public IHystrixProperty<TimeSpan> CircuitBreakerSleepWindow { get; private set; }
        public IHystrixProperty<int> ExecutionIsolationSemaphoreMaxConcurrentRequests { get; private set; }
        public IHystrixProperty<ExecutionIsolationStrategy> ExecutionIsolationStrategy { get; private set; }
        public IHystrixProperty<bool> ExecutionIsolationThreadInterruptOnTimeout { get; private set; }
        public IHystrixProperty<string> ExecutionIsolationThreadPoolKeyOverride { get; private set; }
        public IHystrixProperty<TimeSpan> ExecutionIsolationThreadTimeout { get; private set; }
        public IHystrixProperty<int> FallbackIsolationSemaphoreMaxConcurrentRequests { get; private set; }
        public IHystrixProperty<bool> FallbackEnabled { get; private set; }
        public IHystrixProperty<TimeSpan> MetricsHealthSnapshotInterval { get; private set; }
        public IHystrixProperty<int> MetricsRollingPercentileBucketSize { get; private set; }
        public IHystrixProperty<bool> MetricsRollingPercentileEnabled { get; private set; }
        public IHystrixProperty<int> MetricsRollingPercentileWindowInMilliseconds { get; private set; }
        public IHystrixProperty<int> MetricsRollingPercentileWindowBuckets { get; private set; }
        public IHystrixProperty<int> MetricsRollingStatisticalWindowInMilliseconds { get; private set; }
        public IHystrixProperty<int> MetricsRollingStatisticalWindowBuckets { get; private set; }
        public IHystrixProperty<bool> RequestCacheEnabled { get; private set; }
        public IHystrixProperty<bool> RequestLogEnabled { get; private set; }

        public HystrixPropertiesCommandDefault(HystrixCommandPropertiesSetter setter)
        {
            CircuitBreakerEnabled = HystrixPropertyFactory.AsProperty(setter.CircuitBreakerEnabled, DefaultCircuitBreakerEnabled);
            CircuitBreakerErrorThresholdPercentage = HystrixPropertyFactory.AsProperty(setter.CircuitBreakerErrorThresholdPercentage, DefaultCircuitBreakerErrorThresholdPercentage);
            CircuitBreakerForceClosed = HystrixPropertyFactory.AsProperty(setter.CircuitBreakerForceClosed, DefaultCircuitBreakerForceClosed);
            CircuitBreakerForceOpen = HystrixPropertyFactory.AsProperty(setter.CircuitBreakerForceOpen, DefaultCircuitBreakerForceOpen);
            CircuitBreakerRequestVolumeThreshold = HystrixPropertyFactory.AsProperty(setter.CircuitBreakerRequestVolumeThreshold, DefaultCircuitBreakerRequestVolumeThreshold);
            CircuitBreakerSleepWindow = HystrixPropertyFactory.AsProperty(setter.CircuitBreakerSleepWindow, DefaultCircuitBreakerSleepWindow);
            ExecutionIsolationSemaphoreMaxConcurrentRequests = HystrixPropertyFactory.AsProperty(setter.ExecutionIsolationSemaphoreMaxConcurrentRequests, DefaultExecutionIsolationSemaphoreMaxConcurrentRequests);
            ExecutionIsolationStrategy = HystrixPropertyFactory.AsProperty(setter.ExecutionIsolationStrategy, DefaultExecutionIsolationStrategy);
            ExecutionIsolationThreadInterruptOnTimeout = HystrixPropertyFactory.AsProperty(setter.ExecutionIsolationThreadInterruptOnTimeout, DefaultExecutionIsolationThreadInterruptOnTimeout);
            ExecutionIsolationThreadPoolKeyOverride = HystrixPropertyFactory.AsProperty<string>((string)null);
            ExecutionIsolationThreadTimeout = HystrixPropertyFactory.AsProperty(setter.ExecutionIsolationThreadTimeout, DefaultExecutionIsolationThreadTimeout);
            FallbackIsolationSemaphoreMaxConcurrentRequests = HystrixPropertyFactory.AsProperty(setter.FallbackIsolationSemaphoreMaxConcurrentRequests, DefaultFallbackIsolationSemaphoreMaxConcurrentRequests);
            FallbackEnabled = HystrixPropertyFactory.AsProperty(setter.FallbackEnabled, DefaultFallbackEnabled);
            MetricsHealthSnapshotInterval = HystrixPropertyFactory.AsProperty(setter.MetricsHealthSnapshotInterval, DefaultMetricsHealthSnapshotInterval);
            MetricsRollingPercentileBucketSize = HystrixPropertyFactory.AsProperty(setter.MetricsRollingPercentileBucketSize, DefaultMetricsRollingPercentileBucketSize);
            MetricsRollingPercentileEnabled = HystrixPropertyFactory.AsProperty(setter.MetricsRollingPercentileEnabled, DefaultMetricsRollingPercentileEnabled);
            MetricsRollingPercentileWindowInMilliseconds = HystrixPropertyFactory.AsProperty(setter.MetricsRollingPercentileWindowInMilliseconds, DefaultMetricsRollingPercentileWindowInMilliseconds);
            MetricsRollingPercentileWindowBuckets = HystrixPropertyFactory.AsProperty(setter.MetricsRollingPercentileWindowBuckets, DefaultMetricsRollingPercentileWindowBuckets);
            MetricsRollingStatisticalWindowInMilliseconds = HystrixPropertyFactory.AsProperty(setter.MetricsRollingStatisticalWindowInMilliseconds, DefaultMetricsRollingStatisticalWindowInMilliseconds);
            MetricsRollingStatisticalWindowBuckets = HystrixPropertyFactory.AsProperty(setter.MetricsRollingStatisticalWindowBuckets, DefaultMetricsRollingStatisticalWindowBuckets);
            RequestCacheEnabled = HystrixPropertyFactory.AsProperty(setter.RequestCacheEnabled, DefaultRequestCacheEnabled);
            RequestLogEnabled = HystrixPropertyFactory.AsProperty(setter.RequestLogEnabled, DefaultRequestLogEnabled);
        }
    }
}
