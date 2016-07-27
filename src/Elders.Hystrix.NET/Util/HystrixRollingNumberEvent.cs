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

namespace Elders.Hystrix.NET.Util
{
    /// <summary>
    /// <para>
    /// Various states/events that can be captured in the <see cref="HystrixRollingNumber"/>.
    /// </para>
    /// <para>
    /// Events can be type of Counter or MaxUpdater, which can be determined using the
    /// <see cref="HystrixRollingNumberEventExtensions.IsCounter()"/> or
    /// <see cref="HystrixRollingNumberEventExtensions.IsMaxUpdater()"/> extension methods.
    /// </para>
    /// <para>
    /// The Counter type events can be used with <see cref="HystrixRollingNumber.Increment()"/>, <see cref="HystrixRollingNumber.Add()"/>,
    /// <see cref="HystrixRollingNumber.GetRollingSum()"/> methods.
    /// </para>
    /// <para>
    /// The MaxUpdater type events can be used with <see cref="HystrixRollingNumber.UpdateRollingMax()"/> and <see cref="HystrixRollingNumber.GetRollingMax()"/> methods.
    /// </para>
    /// </summary>
    public enum HystrixRollingNumberEvent
    {
        /// <summary>
        /// When a <see cref="HystrixCommand" /> successfully completes.
        /// </summary>
        Success,

        /// <summary>
        /// When a <see cref="HystrixCommand" /> fails to complete.
        /// </summary>
        Failure,

        /// <summary>
        /// When a <see cref="HystrixCommand" /> times out (fails to complete).
        /// </summary>
        Timeout,

        /// <summary>
        /// When a <see cref="HystrixCommand" /> performs a short-circuited fallback.
        /// </summary>
        ShortCircuited,

        /// <summary>
        /// When a <see cref="HystrixCommand" /> is unable to queue up (thread pool rejection).
        /// </summary>
        ThreadPoolRejected,

        /// <summary>
        /// When a <see cref="HystrixCommand" /> is unable to execute due to reaching the semaphore limit.
        /// </summary>
        SemaphoreRejected,

        /// <summary>
        /// When a <see cref="HystrixCommand" /> returns a Fallback successfully.
        /// </summary>
        FallbackSuccess,

        /// <summary>
        /// When a <see cref="HystrixCommand" /> attempts to retrieve a fallback but fails.
        /// </summary>
        FallbackFailure,

        /// <summary>
        /// When a <see cref="HystrixCommand" /> attempts to retrieve a fallback but it is rejected due to too many concurrent executing fallback requests.
        /// </summary>
        FallbackRejection,

        /// <summary>
        /// When a <see cref="HystrixCommand" /> throws an exception.
        /// </summary>
        ExceptionThrown,

        /// <summary>
        /// When a thread is executed.
        /// </summary>
        ThreadExecution,

        /// <summary>
        /// A MaxUpdater event which is used to determine the maximum number of concurrent threads.
        /// </summary>
        ThreadMaxActive,

        /// <summary>
        /// When a command is fronted by an <see cref="HystrixCollapser" /> then this marks how many requests are collapsed into the single command execution.
        /// </summary>
        Collapsed,

        /// <summary>
        /// When a response is coming from a cache. The cache-hit ratio can be determined by dividing this number by the total calls.
        /// </summary>
        ResponseFromCache,
    }
}
