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
    using Java.Util.Concurrent;
    using Elders.Hystrix.NET.Strategy;
    using Elders.Hystrix.NET.Strategy.Concurrency;

    /// <summary>
    /// The default production implementation of <see cref="IHystrixThreadPool"/>.
    /// </summary>
    internal class HystrixThreadPoolDefault : IHystrixThreadPool
    {
        /// <summary>
        /// The properties of this thread pool.
        /// </summary>
        private readonly IHystrixThreadPoolProperties properties;

        /// <summary>
        /// The blocking queue instance of the <see cref="ThreadPoolExecutor"/>.
        /// </summary>
        private readonly IBlockingQueue<IRunnable> queue;

        /// <summary>
        /// The <see cref="ThreadPoolExecutor"/> to execute commands.
        /// </summary>
        private readonly ThreadPoolExecutor threadPool;

        /// <summary>
        /// The metrics object of this thread pool.
        /// </summary>
        private readonly HystrixThreadPoolMetrics metrics;

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixThreadPoolDefault"/> class.
        /// </summary>
        /// <param name="threadPoolKey">The key of this thread pool.</param>
        /// <param name="setter">The default properties of this thread pool.</param>
        public HystrixThreadPoolDefault(HystrixThreadPoolKey threadPoolKey, HystrixThreadPoolPropertiesSetter setter)
        {
            this.properties = HystrixPropertiesFactory.GetThreadPoolProperties(threadPoolKey, setter);
            this.queue = HystrixPlugins.Instance.ConcurrencyStrategy.GetBlockingQueue(this.properties.MaxQueueSize.Get());
            this.threadPool = HystrixPlugins.Instance.ConcurrencyStrategy.GetThreadPool(threadPoolKey, this.properties.CoreSize, this.properties.CoreSize, this.properties.KeepAliveTime, this.queue);
            this.metrics = HystrixThreadPoolMetrics.GetInstance(threadPoolKey, this.threadPool, this.properties);

            HystrixMetricsPublisherFactory.CreateOrRetrievePublisherForThreadPool(threadPoolKey, this.metrics, this.properties);
        }

        /// <inheritdoc />
        public ThreadPoolExecutor Executor
        {
            get
            {
                // allow us to change things via fast-properties by setting it each time
                this.threadPool.CorePoolSize = this.properties.CoreSize.Get();
                this.threadPool.MaximumPoolSize = this.properties.CoreSize.Get(); // we always want maxSize the same as coreSize, we are not using a dynamically resizing pool
                this.threadPool.KeepAliveTime = this.properties.KeepAliveTime.Get(); // this doesn't really matter since we're not resizing

                return this.threadPool;
            }
        }

        /// <inheritdoc />
        public bool IsQueueSpaceAvailable
        {
            get
            {
                if (this.properties.MaxQueueSize.Get() < 0)
                {
                    return true;
                }
                else
                {
                    return this.threadPool.Queue.Count < this.properties.QueueSizeRejectionThreshold.Get();
                }
            }
        }

        /// <inheritdoc />
        public void MarkThreadExecution()
        {
            this.metrics.MarkThreadExecution();
        }

        /// <inheritdoc />
        public void MarkThreadCompletion()
        {
            this.metrics.MarkThreadCompletion();
        }
    }
}
