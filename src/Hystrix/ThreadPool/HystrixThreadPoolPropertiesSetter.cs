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
    /// Fluent interface that allows chained setting of properties that can be passed into a <see cref="IHystrixThreadPool"/>
    /// via a <see cref="HystrixCommand"/> constructor to inject instance specific property.
    /// </summary>
    public class HystrixThreadPoolPropertiesSetter
    {
        /// <summary>
        /// Gets the number of threads in the thread-pool.
        /// </summary>
        public int? CoreSize { get; private set; }

        /// <summary>
        /// Gets the time that threads exceeding the core size remain idle before being terminated.
        /// </summary>
        public TimeSpan? KeepAliveTime { get; private set; }

        /// <summary>
        /// Gets the max queue size that gets passed to <see cref="IBlockingQueue"/> by the current <see cref="IHystrixConcurrencyStrategy"/>.
        /// </summary>
        public int? MaxQueueSize { get; private set; }

        /// <summary>
        /// Gets the number of buckets the rolling statistical window is broken into. This is passed into <see cref="HystrixRollingNumber"/>
        /// inside each <see cref="HystrixThreadPoolMetrics"/> instance.
        /// </summary>
        public int? MetricsRollingStatisticalWindowBuckets { get; private set; }

        /// <summary>
        /// Gets the duration of statistical rolling window in milliseconds. This is passed into <see cref="HystrixRollingNumber"/>
        /// inside each <see cref="HystrixThreadPoolMetrics"/> instance.
        /// </summary>
        public int? MetricsRollingStatisticalWindowInMilliseconds { get; private set; }

        /// <summary>
        /// Gets the queue size rejection threshold. It's an artificial "max" size at which rejections will occur even if
        /// <see cref="MaxQueueSize"/> has not been reached. This is done because the <see cref="MaxQueueSize"/> of a
        /// <see cref="IBlockingQueue"/> can not be dynamically changed and we want to support dynamically changing the
        /// queue size that affects rejections.
        /// </summary>
        public int? QueueSizeRejectionThreshold { get; private set; }

        /// <summary>
        /// Sets the core size.
        /// </summary>
        /// <param name="value">The core size.</param>
        /// <returns>This setter instance.</returns>
        public HystrixThreadPoolPropertiesSetter WithCoreSize(int value)
        {
            this.CoreSize = value;
            return this;
        }

        /// <summary>
        /// Sets the keep alive time.
        /// </summary>
        /// <param name="value">The keep alive time.</param>
        /// <returns>This setter instance.</returns>
        public HystrixThreadPoolPropertiesSetter WithKeepAliveTime(TimeSpan value)
        {
            this.KeepAliveTime = value;
            return this;
        }

        /// <summary>
        /// Sets the max queue size.
        /// </summary>
        /// <param name="value">The max queue size.</param>
        /// <returns>This setter instance.</returns>
        public HystrixThreadPoolPropertiesSetter WithMaxQueueSize(int value)
        {
            this.MaxQueueSize = value;
            return this;
        }

        /// <summary>
        /// Sets the queue size rejection threshold.
        /// </summary>
        /// <param name="value">The queue size rejection threshold.</param>
        /// <returns>This setter instance.</returns>
        public HystrixThreadPoolPropertiesSetter WithQueueSizeRejectionThreshold(int value)
        {
            this.QueueSizeRejectionThreshold = value;
            return this;
        }

        /// <summary>
        /// Sets the rolling statistical window.
        /// </summary>
        /// <param name="value">The rolling statistical window.</param>
        /// <returns>This setter instance.</returns>
        public HystrixThreadPoolPropertiesSetter WithMetricsRollingStatisticalWindow(int value)
        {
            this.MetricsRollingStatisticalWindowInMilliseconds = value;
            return this;
        }

        /// <summary>
        /// Sets the rolling statistical window buckets.
        /// </summary>
        /// <param name="value">The rolling statistical window buckets.</param>
        /// <returns>This setter instance.</returns>
        public HystrixThreadPoolPropertiesSetter WithMetricsRollingStatisticalWindowBuckets(int value)
        {
            this.MetricsRollingStatisticalWindowBuckets = value;
            return this;
        }
    }
}
