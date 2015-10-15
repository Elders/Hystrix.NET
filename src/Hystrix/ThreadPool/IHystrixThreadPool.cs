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
    using Java.Util.Concurrent;

    /// <summary>
    /// ThreadPool used to execute <see cref="HystrixCommand.Run"/> on separate threads when configured to do so with <see cref="HystrixCommandProperties.ExecutionIsolationStrategy"/>.
    /// Typically each <see cref="HystrixCommandGroupKey"/> has its own thread-pool so that any one group of commands can not starve others from being able to run.
    /// A <see cref="HystrixCommand"/> can be configured with a thread-pool explicitly by injecting a <see cref="HystrixThreadPoolKey"/> or via the
    /// <see cref="HystrixCommandProperties.ExecutionIsolationThreadPoolKeyOverride"/> otherwise it
    /// will derive a <see cref="HystrixThreadPoolKey"/> from the injected <see cref="HystrixCommandGroupKey"/>.
    /// The pool should be sized large enough to handle normal healthy traffic but small enough that it will constrain concurrent execution if backend calls become latent.
    /// For more information see the Github Wiki: https://github.com/Netflix/Hystrix/wiki/Configuration#wiki-ThreadPool and https://github.com/Netflix/Hystrix/wiki/How-it-Works#wiki-Isolation
    /// </summary>
    public interface IHystrixThreadPool
    {
        /// <summary>
        /// Gets the implementation of <see cref="ThreadPoolExecutor"/>.
        /// </summary>
        ThreadPoolExecutor Executor { get; }

        /// <summary>
        /// Gets a value indicating whether the queue will allow adding an item to it.
        /// This allows dynamic control of the max queue size versus whatever the actual max queue size
        /// is so that dynamic changes can be done via property changes rather than needing an app
        /// restart to adjust when commands should be rejected from queuing up.
        /// </summary>
        bool IsQueueSpaceAvailable { get; }

        /// <summary>
        /// Marks the beginning of a command execution.
        /// </summary>
        void MarkThreadExecution();

        /// <summary>
        /// Marks the completion of a command execution.
        /// </summary>
        void MarkThreadCompletion();
    }
}
