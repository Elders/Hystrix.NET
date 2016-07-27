namespace Elders.Hystrix.NET
{
    using System;

    public class HystrixCommandPropertiesSetter
    {
        public bool? CircuitBreakerEnabled { get; private set; }
        public int? CircuitBreakerErrorThresholdPercentage { get; private set; }
        public bool? CircuitBreakerForceClosed { get; private set; }
        public bool? CircuitBreakerForceOpen { get; private set; }
        public int? CircuitBreakerRequestVolumeThreshold { get; private set; }
        public TimeSpan? CircuitBreakerSleepWindow { get; private set; }
        public int? ExecutionIsolationSemaphoreMaxConcurrentRequests { get; private set; }
        public ExecutionIsolationStrategy? ExecutionIsolationStrategy { get; private set; }
        public bool? ExecutionIsolationThreadInterruptOnTimeout { get; private set; }
        public TimeSpan? ExecutionIsolationThreadTimeout { get; private set; }
        public int? FallbackIsolationSemaphoreMaxConcurrentRequests { get; private set; }
        public bool? FallbackEnabled { get; private set; }
        public TimeSpan? MetricsHealthSnapshotInterval { get; private set; }
        public int? MetricsRollingPercentileBucketSize { get; private set; }
        public bool? MetricsRollingPercentileEnabled { get; private set; }
        public int? MetricsRollingPercentileWindowInMilliseconds { get; private set; }
        public int? MetricsRollingPercentileWindowBuckets { get; private set; }
        public int? MetricsRollingStatisticalWindowInMilliseconds { get; private set; }
        public int? MetricsRollingStatisticalWindowBuckets { get; private set; }
        public bool? RequestCacheEnabled { get; private set; }
        public bool? RequestLogEnabled { get; private set; }

        public HystrixCommandPropertiesSetter WithCircuitBreakerEnabled(bool value)
        {
            CircuitBreakerEnabled = value;
            return this;
        }
        public HystrixCommandPropertiesSetter WithCircuitBreakerErrorThresholdPercentage(int value)
        {
            CircuitBreakerErrorThresholdPercentage = value;
            return this;
        }
        public HystrixCommandPropertiesSetter WithCircuitBreakerForceClosed(bool value)
        {
            CircuitBreakerForceClosed = value;
            return this;
        }
        public HystrixCommandPropertiesSetter WithCircuitBreakerForceOpen(bool value)
        {
            CircuitBreakerForceOpen = value;
            return this;
        }
        public HystrixCommandPropertiesSetter WithCircuitBreakerRequestVolumeThreshold(int value)
        {
            CircuitBreakerRequestVolumeThreshold = value;
            return this;
        }
        public HystrixCommandPropertiesSetter WithCircuitBreakerSleepWindowInMilliseconds(int value)
        {
            CircuitBreakerSleepWindow = TimeSpan.FromMilliseconds(value);
            return this;
        }
        public HystrixCommandPropertiesSetter WithCircuitBreakerSleepWindow(TimeSpan value)
        {
            CircuitBreakerSleepWindow = value;
            return this;
        }
        public HystrixCommandPropertiesSetter WithExecutionIsolationSemaphoreMaxConcurrentRequests(int value)
        {
            ExecutionIsolationSemaphoreMaxConcurrentRequests = value;
            return this;
        }
        public HystrixCommandPropertiesSetter WithExecutionIsolationStrategy(ExecutionIsolationStrategy value)
        {
            ExecutionIsolationStrategy = value;
            return this;
        }
        public HystrixCommandPropertiesSetter WithExecutionIsolationThreadInterruptOnTimeout(bool value)
        {
            ExecutionIsolationThreadInterruptOnTimeout = value;
            return this;
        }
        public HystrixCommandPropertiesSetter WithExecutionIsolationThreadTimeoutInMilliseconds(int milliseconds)
        {
            ExecutionIsolationThreadTimeout = TimeSpan.FromMilliseconds(milliseconds);
            return this;
        }
        public HystrixCommandPropertiesSetter WithExecutionIsolationThreadTimeout(TimeSpan value)
        {
            ExecutionIsolationThreadTimeout = value;
            return this;
        }
        public HystrixCommandPropertiesSetter WithFallbackIsolationSemaphoreMaxConcurrentRequests(int value)
        {
            FallbackIsolationSemaphoreMaxConcurrentRequests = value;
            return this;
        }
        public HystrixCommandPropertiesSetter WithFallbackEnabled(bool value)
        {
            FallbackEnabled = value;
            return this;
        }
        public HystrixCommandPropertiesSetter WithMetricsHealthSnapshotInterval(TimeSpan value)
        {
            MetricsHealthSnapshotInterval = value;
            return this;
        }
        public HystrixCommandPropertiesSetter WithMetricsRollingPercentileBucketSize(int value)
        {
            MetricsRollingPercentileBucketSize = value;
            return this;
        }
        public HystrixCommandPropertiesSetter WithMetricsRollingPercentileEnabled(bool value)
        {
            MetricsRollingPercentileEnabled = value;
            return this;
        }
        public HystrixCommandPropertiesSetter WithMetricsRollingPercentileWindow(int value)
        {
            MetricsRollingPercentileWindowInMilliseconds = value;
            return this;
        }
        public HystrixCommandPropertiesSetter WithMetricsRollingPercentileWindowBuckets(int value)
        {
            MetricsRollingPercentileWindowBuckets = value;
            return this;
        }
        public HystrixCommandPropertiesSetter WithMetricsRollingStatisticalWindowInMilliseconds(int value)
        {
            MetricsRollingStatisticalWindowInMilliseconds = value;
            return this;
        }
        public HystrixCommandPropertiesSetter WithMetricsRollingStatisticalWindow(TimeSpan value)
        {
            MetricsRollingStatisticalWindowInMilliseconds = (int)value.TotalMilliseconds;
            return this;
        }
        public HystrixCommandPropertiesSetter WithMetricsRollingStatisticalWindowBuckets(int value)
        {
            MetricsRollingStatisticalWindowBuckets = value;
            return this;
        }
        public HystrixCommandPropertiesSetter WithRequestCacheEnabled(bool value)
        {
            RequestCacheEnabled = value;
            return this;
        }
        public HystrixCommandPropertiesSetter WithRequestLogEnabled(bool value)
        {
            RequestLogEnabled = value;
            return this;
        }
    }
}
