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

namespace Elders.Hystrix.NET.Exceptions
{
    /// <summary>
    /// Describes what kind of failure happened during the execution of a <see cref="HystrixCommand"/>.
    /// </summary>
    public enum FailureType
    {

        CommandException,
        Timeout,
        Shortcircuit,
        RejectedThreadExecution,
        RejectedSemaphoreExecution,

        /// <summary>
        /// The maximum number of concurrent requests permitted to <see cref="HystrixCommand.GetFallback"/> is limited by
        /// <see cref="IHystrixCommandProperties.FallbackIsolationSemaphoreMaxConcurrentRequests"/>. This value indicates
        /// that this command was beyond the concurrent limit, so it's got rejected.
        /// </summary>
        RejectedSemaphoreFallback,
    }
}
