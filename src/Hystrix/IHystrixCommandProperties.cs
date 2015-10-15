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

namespace Netflix.Hystrix
{
    using System;
    using Netflix.Hystrix.Strategy;
    using Netflix.Hystrix.Strategy.Properties;

    /// <summary>
    /// Provides properties for <see cref="HystrixCommand"/> instances. The instances of <see cref="IHystrixCommandProperties"/>
    /// will be created by the current <see cref="IHystrixPropertiesStrategy"/> registered in <see cref="HystrixPlugins"/>.
    /// We can create only a <see cref="HystrixCommandPropertiesSetter"/> for a command, which is only used to get the
    /// default values for the current <see cref="IHystrixCommandProperties"/> implementation.
    /// </summary>
    /// <seealso cref="HystrixPlugins"/>
    /// <seealso cref="IHystrixPropertiesStrategy"/>
    /// <seealso cref="HystrixCommandPropertiesSetter"/>
    public interface IHystrixCommandProperties
    {
        IHystrixProperty<bool> CircuitBreakerEnabled { get; }
        IHystrixProperty<int> CircuitBreakerErrorThresholdPercentage { get; }
        IHystrixProperty<bool> CircuitBreakerForceClosed { get; }
        IHystrixProperty<bool> CircuitBreakerForceOpen { get; }
        IHystrixProperty<int> CircuitBreakerRequestVolumeThreshold { get; }
        IHystrixProperty<TimeSpan> CircuitBreakerSleepWindow { get; }

        /// <summary>
        /// Gets which <see cref="ExecutionIsolationStrategy"/> will be used to execute <see cref="HystrixCommand.Run"/>.
        /// </summary>
        /// <seealso cref="ExecutionIsolationStrategy"/>
        IHystrixProperty<ExecutionIsolationStrategy> ExecutionIsolationStrategy { get; }

        /// <summary>
        /// Gets whether the command should interrupt the thread isolated hystrix execution on timeout or not.
        /// </summary>
        IHystrixProperty<bool> ExecutionIsolationThreadInterruptOnTimeout { get; }

        IHystrixProperty<int> ExecutionIsolationSemaphoreMaxConcurrentRequests { get; }

        IHystrixProperty<string> ExecutionIsolationThreadPoolKeyOverride { get; }

        IHystrixProperty<TimeSpan> ExecutionIsolationThreadTimeout { get; }

        IHystrixProperty<bool> FallbackEnabled { get; }

        /// <summary>
        /// Gets the maximum number of concurrent requests permitted to <see cref="HystrixCommand.GetFallback"/> in this command.
        /// Requests beyond the concurrent limit will fail-fast and not attempt retrieving a fallback.
        /// </summary>
        IHystrixProperty<int> FallbackIsolationSemaphoreMaxConcurrentRequests { get; }

        IHystrixProperty<TimeSpan> MetricsHealthSnapshotInterval { get; }
        IHystrixProperty<int> MetricsRollingPercentileBucketSize { get; }
        IHystrixProperty<bool> MetricsRollingPercentileEnabled { get; }
        IHystrixProperty<int> MetricsRollingPercentileWindowInMilliseconds { get; }
        IHystrixProperty<int> MetricsRollingPercentileWindowBuckets { get; }
        IHystrixProperty<int> MetricsRollingStatisticalWindowInMilliseconds { get; }
        IHystrixProperty<int> MetricsRollingStatisticalWindowBuckets { get; }
        IHystrixProperty<bool> RequestCacheEnabled { get; }
        IHystrixProperty<bool> RequestLogEnabled { get; }
    }
}
