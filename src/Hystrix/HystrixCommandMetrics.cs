namespace Netflix.Hystrix
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using Netflix.Hystrix.Strategy;
    using Netflix.Hystrix.Strategy.EventNotifier;
    using Netflix.Hystrix.Util;
    using slf4net;

    public class HystrixCommandMetrics
    {
        private static readonly ILogger logger = LoggerFactory.GetLogger(typeof(HystrixCommandMetrics));
        private static readonly ConcurrentDictionary<string, HystrixCommandMetrics> metrics = new ConcurrentDictionary<string, HystrixCommandMetrics>();

        public static HystrixCommandMetrics GetInstance(HystrixCommandKey key, HystrixCommandGroupKey commandGroup, IHystrixCommandProperties properties)
        {
            return metrics.GetOrAdd(key.Name, w => new HystrixCommandMetrics(key, commandGroup, properties, HystrixPlugins.Instance.EventNotifier));
        }
        public static HystrixCommandMetrics GetInstance(HystrixCommandKey key)
        {
            return metrics[key.Name];
        }
        public static IEnumerable<HystrixCommandMetrics> Instances { get { return metrics.Values; } }
        internal static void Reset()
        {
            metrics.Clear();
        }

        private readonly IHystrixCommandProperties properties;
        private readonly HystrixRollingNumber counter;
        private readonly HystrixRollingPercentile percentileExecution;
        private readonly HystrixRollingPercentile percentileTotal;
        private readonly HystrixCommandKey key;
        private readonly HystrixCommandGroupKey group;
        private readonly IHystrixEventNotifier eventNotifier;
        private int concurrentExecutionCount;

        public HystrixCommandKey CommandKey { get { return this.key; } }
        public HystrixCommandGroupKey CommandGroup { get { return this.group; } }
        public IHystrixCommandProperties Properties { get { return this.properties; } }
        public int CurrentConcurrentExecutionCount { get { return this.concurrentExecutionCount; } }

        internal HystrixCommandMetrics(HystrixCommandKey key, HystrixCommandGroupKey commandGroup, IHystrixCommandProperties properties, IHystrixEventNotifier eventNotifier)
        {
            this.key = key;
            this.group = commandGroup;
            this.properties = properties;
            this.counter = new HystrixRollingNumber(properties.MetricsRollingStatisticalWindowInMilliseconds, properties.MetricsRollingStatisticalWindowBuckets);
            this.percentileExecution = new HystrixRollingPercentile(properties.MetricsRollingPercentileWindowInMilliseconds, properties.MetricsRollingPercentileWindowBuckets, properties.MetricsRollingPercentileBucketSize, properties.MetricsRollingPercentileEnabled);
            this.percentileTotal = new HystrixRollingPercentile(properties.MetricsRollingPercentileWindowInMilliseconds, properties.MetricsRollingPercentileWindowBuckets, properties.MetricsRollingPercentileBucketSize, properties.MetricsRollingPercentileEnabled);
            this.eventNotifier = eventNotifier;
        }

        public long GetCumulativeCount(HystrixRollingNumberEvent ev)
        {
            return this.counter.GetCumulativeSum(ev);
        }
        public long GetRollingCount(HystrixRollingNumberEvent ev)
        {
            return this.counter.GetRollingSum(ev);
        }
        public int GetExecutionTimePercentile(double percentile)
        {
            return this.percentileExecution.GetPercentile(percentile);
        }
        public int GetExecutionTimeMean()
        {
            return this.percentileExecution.GetMean();
        }
        public int GetTotalTimePercentile(double percentile)
        {
            return this.percentileTotal.GetPercentile(percentile);
        }
        public int GetTotalTimeMean()
        {
            return this.percentileTotal.GetMean();
        }

        internal void ResetCounter()
        {
            this.counter.Reset();
        }

        internal void MarkSuccess(long duration)
        {
            this.eventNotifier.MarkEvent(HystrixEventType.Success, this.key);
            this.counter.Increment(HystrixRollingNumberEvent.Success);
        }
        internal void MarkFailure(long duration)
        {
            this.eventNotifier.MarkEvent(HystrixEventType.Failure, this.key);
            this.counter.Increment(HystrixRollingNumberEvent.Failure);
        }
        internal void MarkTimeout(long duration)
        {
            this.eventNotifier.MarkEvent(HystrixEventType.Timeout, this.key);
            this.counter.Increment(HystrixRollingNumberEvent.Timeout);
        }
        internal void MarkShortCircuited()
        {
            this.eventNotifier.MarkEvent(HystrixEventType.ShortCircuited, this.key);
            this.counter.Increment(HystrixRollingNumberEvent.ShortCircuited);
        }
        internal void MarkThreadPoolRejection()
        {
            this.eventNotifier.MarkEvent(HystrixEventType.ThreadPoolRejected, this.key);
            this.counter.Increment(HystrixRollingNumberEvent.ThreadPoolRejected);
        }
        internal void MarkSemaphoreRejection()
        {
            this.eventNotifier.MarkEvent(HystrixEventType.SemaphoreRejected, this.key);
            this.counter.Increment(HystrixRollingNumberEvent.SemaphoreRejected);
        }
        internal void MarkFallbackSuccess()
        {
            this.eventNotifier.MarkEvent(HystrixEventType.FallbackSuccess, key);
            this.counter.Increment(HystrixRollingNumberEvent.FallbackSuccess);
        }
        internal void MarkFallbackFailure()
        {
            this.eventNotifier.MarkEvent(HystrixEventType.FallbackFailure, key);
            this.counter.Increment(HystrixRollingNumberEvent.FallbackFailure);
        }
        internal void MarkFallbackRejection()
        {
            this.eventNotifier.MarkEvent(HystrixEventType.FallbackRejection, key);
            this.counter.Increment(HystrixRollingNumberEvent.FallbackRejection);
        }
        internal void MarkExceptionThrown()
        {
            this.eventNotifier.MarkEvent(HystrixEventType.ExceptionThrown, key);
            this.counter.Increment(HystrixRollingNumberEvent.ExceptionThrown);
        }
        internal void MarkCollapsed(int numRequestsCollapsedToBatch)
        {
            this.eventNotifier.MarkEvent(HystrixEventType.Collapsed, key);
            this.counter.Add(HystrixRollingNumberEvent.Collapsed, numRequestsCollapsedToBatch);
        }
        internal void MarkResponseFromCache()
        {
            this.eventNotifier.MarkEvent(HystrixEventType.ResponseFromCache, key);
            this.counter.Increment(HystrixRollingNumberEvent.ResponseFromCache);
        }

        internal void IncrementConcurrentExecutionCount()
        {
            Interlocked.Increment(ref this.concurrentExecutionCount);
        }
        internal void DecrementConcurrentExecutionCount()
        {
            Interlocked.Decrement(ref this.concurrentExecutionCount);
        }

        internal void AddCommandExecutionTime(long duration)
        {
            this.percentileExecution.AddValue((int)duration);
        }
        internal void AddUserThreadExecutionTime(long duration)
        {
            this.percentileTotal.AddValue((int)duration);
        }

        private volatile HealthCounts healthCountsSnapshot = new HealthCounts(0, 0);
        private long lastHealthCountsSnapshot = ActualTime.CurrentTimeInMillis;

        public HealthCounts GetHealthCounts()
        {
            // we put an interval between snapshots so high-volume commands don't 
            // spend too much unnecessary time calculating metrics in very small time periods
            long lastTime = this.lastHealthCountsSnapshot;
            long currentTime = ActualTime.CurrentTimeInMillis;
            if (currentTime - lastTime >= this.properties.MetricsHealthSnapshotInterval.Get().TotalMilliseconds || this.healthCountsSnapshot == null)
            {
                if (Interlocked.CompareExchange(ref this.lastHealthCountsSnapshot, currentTime, lastTime) == lastTime)
                {
                    // our thread won setting the snapshot time so we will proceed with generating a new snapshot
                    // losing threads will continue using the old snapshot
                    long success = counter.GetRollingSum(HystrixRollingNumberEvent.Success);
                    long failure = counter.GetRollingSum(HystrixRollingNumberEvent.Failure); // fallbacks occur on this
                    long timeout = counter.GetRollingSum(HystrixRollingNumberEvent.Timeout); // fallbacks occur on this
                    long threadPoolRejected = counter.GetRollingSum(HystrixRollingNumberEvent.ThreadPoolRejected); // fallbacks occur on this
                    long semaphoreRejected = counter.GetRollingSum(HystrixRollingNumberEvent.SemaphoreRejected); // fallbacks occur on this
                    long shortCircuited = counter.GetRollingSum(HystrixRollingNumberEvent.ShortCircuited); // fallbacks occur on this

                    long totalCount = failure + success + timeout + threadPoolRejected + shortCircuited + semaphoreRejected;
                    long errorCount = failure + timeout + threadPoolRejected + shortCircuited + semaphoreRejected;

                    healthCountsSnapshot = new HealthCounts(totalCount, errorCount);
                }
            }
            return healthCountsSnapshot;
        }
    }
}
