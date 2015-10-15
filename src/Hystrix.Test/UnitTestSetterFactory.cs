namespace Hystrix.Test
{
    using System;
    using Netflix.Hystrix;
    using Netflix.Hystrix.ThreadPool;

    internal static class UnitTestSetterFactory
    {
        public static HystrixThreadPoolPropertiesSetter GetThreadPoolPropertiesSetter()
        {
            return new HystrixThreadPoolPropertiesSetter()
                .WithCoreSize(10)                                                   // size of thread pool
                .WithKeepAliveTime(TimeSpan.FromMinutes(1.0))                       // minutes to keep a thread alive (though in practice this doesn't get used as by default we set a fixed size)
                .WithMaxQueueSize(100)                                              // size of queue (but we never allow it to grow this big ... this can't be dynamically changed so we use 'queueSizeRejectionThreshold' to artificially limit and reject)
                .WithQueueSizeRejectionThreshold(10)                                // number of items in queue at which point we reject (this can be dyamically changed)
                .WithMetricsRollingStatisticalWindow(10000)    // milliseconds for rolling number
                .WithMetricsRollingStatisticalWindowBuckets(10);                    // number of buckets in rolling number (10 1-second buckets)
        }

        public static HystrixCommandPropertiesSetter GetCommandPropertiesSetter()
        {
            return new HystrixCommandPropertiesSetter()
                .WithExecutionIsolationThreadTimeout(TimeSpan.FromSeconds(1.0))     // when an execution will be timed out
                .WithExecutionIsolationStrategy(ExecutionIsolationStrategy.Thread)  // we want thread execution by default in tests
                .WithExecutionIsolationThreadInterruptOnTimeout(true)
                .WithCircuitBreakerForceOpen(false)                                 // we don't want short-circuiting by default
                .WithCircuitBreakerErrorThresholdPercentage(40)                     // % of 'marks' that must be failed to trip the circuit
                .WithMetricsRollingStatisticalWindowInMilliseconds(5000)            // milliseconds back that will be tracked
                .WithMetricsRollingStatisticalWindowBuckets(5)                      // buckets
                .WithCircuitBreakerRequestVolumeThreshold(0)                        // in testing we will not have a threshold unless we're specifically testing that feature
                .WithCircuitBreakerSleepWindow(TimeSpan.FromSeconds(5000.0))        // milliseconds after tripping circuit before allowing retry (by default set VERY long as we want it to effectively never allow a singleTest for most unit tests)
                .WithCircuitBreakerEnabled(true)
                .WithRequestLogEnabled(true)
                .WithExecutionIsolationSemaphoreMaxConcurrentRequests(20)
                .WithFallbackIsolationSemaphoreMaxConcurrentRequests(10)
                .WithFallbackEnabled(true)
                .WithCircuitBreakerForceClosed(false)
                .WithMetricsRollingPercentileEnabled(true)
                .WithRequestCacheEnabled(true)
                .WithMetricsRollingPercentileWindow(60000)
                .WithMetricsRollingPercentileWindowBuckets(12)
                .WithMetricsRollingPercentileBucketSize(1000)
                .WithMetricsHealthSnapshotInterval(TimeSpan.Zero);
        }
    }
}
