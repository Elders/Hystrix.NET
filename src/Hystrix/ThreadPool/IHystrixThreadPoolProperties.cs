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
    using Netflix.Hystrix.Strategy.Properties;

    /// <summary>
    /// Properties for instances of <see cref="IHystrixThreadPool"/>. The actual implementation of <see cref="IHystrixThreadPoolProperties"/>
    /// will be initialized by the current <see cref="IHystrixPropertiesStrategy"/> registered in <see cref="HystrixPlugins"/>.
    /// </summary>
    /// <seealso cref="HystrixPlugins"/>
    /// <seealso cref="IHystrixPropertiesStrategy"/>
    /// <seealso cref="HystrixCommandPropertiesSetter"/>
    public interface IHystrixThreadPoolProperties
    {
        /// <summary>
        /// Gets the number of threads in the thread-pool.
        /// </summary>
        IHystrixProperty<int> CoreSize { get; }

        /// <summary>
        /// Gets the time that threads exceeding the core size remain idle before being terminated.
        /// </summary>
        IHystrixProperty<TimeSpan> KeepAliveTime { get; }

        /// <summary>
        /// Gets the max queue size that gets passed to <see cref="IBlockingQueue"/> by the current <see cref="IHystrixConcurrencyStrategy"/>.
        /// </summary>
        IHystrixProperty<int> MaxQueueSize { get; }

        /// <summary>
        /// Gets the number of buckets the rolling statistical window is broken into. This is passed into <see cref="HystrixRollingNumber"/>
        /// inside each <see cref="HystrixThreadPoolMetrics"/> instance.
        /// </summary>
        IHystrixProperty<int> MetricsRollingStatisticalWindowBuckets { get; }

        /// <summary>
        /// Gets the duration of statistical rolling window in milliseconds. This is passed into <see cref="HystrixRollingNumber"/>
        /// inside each <see cref="HystrixThreadPoolMetrics"/> instance.
        /// </summary>
        IHystrixProperty<int> MetricsRollingStatisticalWindowInMilliseconds { get; }

        /// <summary>
        /// Gets the queue size rejection threshold. It's an artificial "max" size at which rejections will occur even if
        /// <see cref="MaxQueueSize"/> has not been reached. This is done because the <see cref="MaxQueueSize"/> of a
        /// <see cref="IBlockingQueue"/> can not be dynamically changed and we want to support dynamically changing the
        /// queue size that affects rejections.
        /// </summary>
        IHystrixProperty<int> QueueSizeRejectionThreshold { get; }
    }
}
