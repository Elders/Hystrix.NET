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

namespace Netflix.Hystrix.ThreadPool
{
    using System;

    /// <summary>
    /// Default implementation of <see cref="IHystrixThreadPoolProperties"/> using
    /// default <see cref="IHystrixProperty"/> implementation initialized from the values
    /// of a <see cref="HystrixThreadPoolPropertiesSetter"/>.
    /// </summary>
    public class HystrixThreadPoolPropertiesDefault : IHystrixThreadPoolProperties
    {
        /// <summary>
        /// Size of thread pool.
        /// </summary>
        private const int DefaultCoreSize = 10;

        /// <summary>
        /// Size of queue (this can't be dynamically changed so we use 'QueueSizeRejectionThreshold' to artificially limit and reject) -1 turns if off and makes us use <see cref="SynchronousQueue"/>.
        /// </summary>
        private const int DefaultMaxQueueSize = -1;

        /// <summary>
        /// Number of items in queue.
        /// </summary>
        private const int DefaultQueueSizeRejectionThreshold = 5;

        /// <summary>
        /// Milliseconds for rolling number.
        /// </summary>
        private const int DefaultMetricsRollingStatisticalWindowInMilliseconds = 10000;

        /// <summary>
        /// Number of buckets in rolling number (10 1-second buckets)
        /// </summary>
        private const int DefaultThreadPoolRollingNumberStatisticalWindowBuckets = 10;

        /// <summary>
        /// Time to keep a thread alive (though in practice this doesn't get used as by default we set a fixed size).
        /// </summary>
        private static readonly TimeSpan DefaultKeepAliveTime = TimeSpan.FromMinutes(1.0);

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixThreadPoolPropertiesDefault"/> class.
        /// </summary>
        /// <param name="setter">The property value provider.</param>
        public HystrixThreadPoolPropertiesDefault(HystrixThreadPoolPropertiesSetter setter)
        {
            if (setter == null)
            {
                throw new ArgumentNullException("setter");
            }

            this.CoreSize = HystrixPropertyFactory.AsProperty(setter.CoreSize, DefaultCoreSize);
            this.KeepAliveTime = HystrixPropertyFactory.AsProperty(setter.KeepAliveTime, DefaultKeepAliveTime);
            this.MaxQueueSize = HystrixPropertyFactory.AsProperty(setter.MaxQueueSize, DefaultMaxQueueSize);
            this.QueueSizeRejectionThreshold = HystrixPropertyFactory.AsProperty(setter.QueueSizeRejectionThreshold, DefaultQueueSizeRejectionThreshold);
            this.MetricsRollingStatisticalWindowInMilliseconds = HystrixPropertyFactory.AsProperty(setter.MetricsRollingStatisticalWindowInMilliseconds, DefaultMetricsRollingStatisticalWindowInMilliseconds);
            this.MetricsRollingStatisticalWindowBuckets = HystrixPropertyFactory.AsProperty(setter.MetricsRollingStatisticalWindowBuckets, DefaultThreadPoolRollingNumberStatisticalWindowBuckets);
        }

        /// <inheritdoc />
        public IHystrixProperty<int> CoreSize { get; private set; }

        /// <inheritdoc />
        public IHystrixProperty<TimeSpan> KeepAliveTime { get; private set; }

        /// <inheritdoc />
        public IHystrixProperty<int> MetricsRollingStatisticalWindowBuckets { get; private set; }

        /// <inheritdoc />
        public IHystrixProperty<int> MetricsRollingStatisticalWindowInMilliseconds { get; private set; }

        /// <inheritdoc />
        public IHystrixProperty<int> MaxQueueSize { get; private set; }

        /// <inheritdoc />
        public IHystrixProperty<int> QueueSizeRejectionThreshold { get; private set; }
    }
}
