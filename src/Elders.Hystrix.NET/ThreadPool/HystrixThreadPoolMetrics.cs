// Copyright 2012 Netflix, Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Elders.Hystrix.NET.ThreadPool
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Java.Util.Concurrent;
    using Elders.Hystrix.NET.Util;

    /// <summary>
    /// Used by <see cref="IHystrixThreadPool"/> to record metrics.
    /// Thread-safe and ensures only 1 <see cref="HystrixThreadPoolMetrics"/> per <see cref="HystrixThreadPoolKey"/>.
    /// </summary>
    public class HystrixThreadPoolMetrics
    {
        /// <summary>
        /// Stores instances of <see cref="HystrixThreadPoolMetrics"/>.
        /// </summary>
        private static readonly ConcurrentDictionary<HystrixThreadPoolKey, HystrixThreadPoolMetrics> Metrics = new ConcurrentDictionary<HystrixThreadPoolKey, HystrixThreadPoolMetrics>();

        /// <summary>
        /// The key of the parent <see cref="IHystrixThreadPool"/>.
        /// </summary>
        private readonly HystrixThreadPoolKey threadPoolKey;

        /// <summary>
        /// The counter used to track thread pool metrics.
        /// </summary>
        private readonly HystrixRollingNumber counter;

        /// <summary>
        /// The <see cref="ThreadPoolExecutor"/> used to execute <see cref="HystrixCommand"/> instances.
        /// </summary>
        private readonly ThreadPoolExecutor threadPool;

        /// <summary>
        /// The properties of the parent <see cref="IHystrixThreadPool"/>.
        /// </summary>
        private readonly IHystrixThreadPoolProperties properties;

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixThreadPoolMetrics"/> class.
        /// </summary>
        /// <param name="threadPoolKey">The key of the parent thread pool.</param>
        /// <param name="threadPool">The <see cref="ThreadPoolExecutor"/> of the parent thread pool.</param>
        /// <param name="properties">The properties of the parent thread pool.</param>
        private HystrixThreadPoolMetrics(HystrixThreadPoolKey threadPoolKey, ThreadPoolExecutor threadPool, IHystrixThreadPoolProperties properties)
        {
            this.threadPoolKey = threadPoolKey;
            this.threadPool = threadPool;
            this.properties = properties;
            this.counter = new HystrixRollingNumber(properties.MetricsRollingStatisticalWindowInMilliseconds, properties.MetricsRollingStatisticalWindowBuckets);
        }

        /// <summary>
        /// Gets the created and stored <see cref="HystrixThreadPoolMetrics"/> instances.
        /// </summary>
        public static IEnumerable<HystrixThreadPoolMetrics> Instances
        {
            get
            {
                return Metrics.Values;
            }
        }

        /// <summary>
        /// Gets the key of the tracked <see cref="IHystrixThreadPool"/>.
        /// </summary>
        public HystrixThreadPoolKey ThreadPoolKey
        {
            get
            {
                return this.threadPoolKey;
            }
        }

        /// <summary>
        /// Gets the properties of the tracked <see cref="IHystrixThreadPool"/>.
        /// </summary>
        public IHystrixThreadPoolProperties Properties
        {
            get
            {
                return this.properties;
            }
        }

        /// <summary>
        /// Gets the number of threads currently running.
        /// </summary>
        public int CurrentActiveCount
        {
            get
            {
                return this.threadPool.ActiveCount;
            }
        }

        /// <summary>
        /// Gets the total number of tasks executed by the tracked thread pool.
        /// </summary>
        public long CurrentCompletedTaskCount
        {
            get
            {
                return this.threadPool.CompletedTaskCount;
            }
        }

        /// <summary>
        /// Gets the core thread pool size.
        /// </summary>
        public int CurrentCorePoolSize
        {
            get
            {
                return this.threadPool.CorePoolSize;
            }
        }

        /// <summary>
        /// Gets the maximum number of threads that have ever worked simultaneously.
        /// </summary>
        public int CurrentLargestPoolSize
        {
            get
            {
                return this.threadPool.LargestPoolSize;
            }
        }

        /// <summary>
        /// Gets the maximum allowed number of threads.
        /// </summary>
        public int CurrentMaximumPoolSize
        {
            get
            {
                return this.threadPool.MaximumPoolSize;
            }
        }

        /// <summary>
        /// Gets the current number of existing threads in the tracked thread pool.
        /// </summary>
        public int CurrentPoolSize
        {
            get
            {
                return this.threadPool.PoolSize;
            }
        }

        /// <summary>
        /// Gets the total number of tasks that have been scheduled by the tracked thread pool.
        /// </summary>
        public long CurrentTaskCount
        {
            get
            {
                return this.threadPool.TaskCount;
            }
        }

        /// <summary>
        /// Gets the number of tasks waiting in the queue.
        /// </summary>
        public int CurrentQueueSize
        {
            get
            {
                return this.threadPool.Queue.Count;
            }
        }

        /// <summary>
        /// Gets the rolling count of number of threads executed during rolling statistical window.
        /// The rolling window is defined by <see cref="HystrixThreadPoolProperties.MetricsRollingStatisticalWindowInMilliseconds"/>.
        /// </summary>
        public long RollingCountThreadsExecuted
        {
            get
            {
                return this.counter.GetRollingSum(HystrixRollingNumberEvent.ThreadExecution);
            }
        }

        /// <summary>
        /// Gets the cumulative count of number of threads executed since the start of the application.
        /// </summary>
        public long CumulativeCountThreadsExecuted
        {
            get
            {
                return this.counter.GetCumulativeSum(HystrixRollingNumberEvent.ThreadExecution);
            }
        }

        /// <summary>
        /// Gets the rolling max number of active threads during rolling statistical window.
        /// The rolling window is defined by <see cref="HystrixThreadPoolProperties.MetricsRollingStatisticalWindowInMilliseconds"/>.
        /// </summary>
        public long RollingMaxActiveThreads
        {
            get
            {
                return this.counter.GetRollingMaxValue(HystrixRollingNumberEvent.ThreadMaxActive);
            }
        }

        /// <summary>
        /// Gets the <see cref="HystrixThreadPoolMetrics"/> instance for a given <see cref="HystrixThreadPoolKey"/> or null if none exists.
        /// </summary>
        /// <param name="key">Key of the tracked thread pool.</param>
        /// <returns>A thread pool metrics instance of the specified thread pool.</returns>
        public static HystrixThreadPoolMetrics GetInstance(HystrixThreadPoolKey key)
        {
            HystrixThreadPoolMetrics threadPoolMetrics = null;
            Metrics.TryGetValue(key, out threadPoolMetrics);
            return threadPoolMetrics;
        }

        /// <summary>
        /// Gets the <see cref="HystrixThreadPoolMetrics"/> instance for a given <see cref="HystrixThreadPoolKey"/>.
        /// If no metrics exists for the specified key, a new one will be created from the specified threadPool and setter.
        /// </summary>
        /// <param name="key">Key of the tracked thread pool.</param>
        /// <param name="threadPool">The thread pool executor of the tracked pool.</param>
        /// <param name="properties">The properties of the tracked pool.</param>
        /// <returns>A new or an existing thread pool metrics instance of the specified key.</returns>
        public static HystrixThreadPoolMetrics GetInstance(HystrixThreadPoolKey key, ThreadPoolExecutor threadPool, IHystrixThreadPoolProperties properties)
        {
            return Metrics.GetOrAdd(key, w => new HystrixThreadPoolMetrics(key, threadPool, properties));
        }

        /// <summary>
        /// Marks the beginning of a command execution.
        /// </summary>
        public void MarkThreadExecution()
        {
            this.counter.Increment(HystrixRollingNumberEvent.ThreadExecution);
            this.SetMaxActiveThreads();
        }

        /// <summary>
        /// Marks the completion of a command execution.
        /// </summary>
        public void MarkThreadCompletion()
        {
            this.SetMaxActiveThreads();
        }

        /// <summary>
        /// Records the current number of active threads in the rolling number.
        /// </summary>
        private void SetMaxActiveThreads()
        {
            this.counter.UpdateRollingMax(HystrixRollingNumberEvent.ThreadMaxActive, this.threadPool.ActiveCount);
        }
    }
}
