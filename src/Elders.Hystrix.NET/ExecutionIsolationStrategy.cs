// Copyright 2012 Netflix, Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Elders.Hystrix.NET
{
    /// <summary>
    /// Describes the way <see cref="HystrixCommand"/> should execute <see cref="HystrixCommand.Run"/>.
    /// </summary>
    public enum ExecutionIsolationStrategy
    {
        /// <summary>
        /// <see cref="HystrixCommand"/> instances will put the work in their thread pools either
        /// you use <see cref="HystrixCommand.Queue"/> or <see cref="HystrixCommand.Execute"/>.
        /// The maximum number of concurrent commands of the same type executing their work is limited
        /// by <see cref="IHystrixThreadPoolProperties.CoreSize"/>. Commands beyond this limit will be queued,
        /// but only if the command's <see cref="IHystrixThreadPool.IsQueueSpaceAvailable"/> is true, otherwise
        /// they will be rejected, causing them to fall-back.
        /// </summary>
        Thread,

        /// <summary>
        /// <see cref="HystrixCommand"/> instances will execute the work synchronously either you
        /// use <see cref="HystrixCommand.Queue"/> or <see cref="HystrixCommand.Execute"/>.
        /// The maximum number of concurrent commands of the same type executing their work is limited
        /// by <see cref="IHystrixCommandProperties.ExecutionIsolationSemaphoreMaxConcurrentRequests"/>.
        /// Commands beyond this limit will be rejected, causing them to fall-back.
        /// </summary>
        Semaphore,
    }
}
