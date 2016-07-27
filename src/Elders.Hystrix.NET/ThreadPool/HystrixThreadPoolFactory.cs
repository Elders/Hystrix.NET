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
    using System;
    using System.Collections.Concurrent;
    using System.Runtime.CompilerServices;
    using System.Threading;

    /// <summary>
    /// Factory of <see cref="IHystrixThreadPool"/> instances.
    /// Thread safe and ensures only 1 <see cref="IHystrixThreadPool"/> per <see cref="HystrixThreadPoolKey"/>.
    /// </summary>
    internal static class HystrixThreadPoolFactory
    {
        /// <summary>
        /// Stores instances of <see cref="IHystrixThreadPool"/>.
        /// </summary>
        private static readonly ConcurrentDictionary<HystrixThreadPoolKey, IHystrixThreadPool> ThreadPools = new ConcurrentDictionary<HystrixThreadPoolKey, IHystrixThreadPool>();

        /// <summary>
        /// Gets the number of created and stored thread pools.
        /// </summary>
        internal static int ThreadPoolCount
        {
            get
            {
                return ThreadPools.Count;
            }
        }
        
        /// <summary>
        /// Gets the <see cref="IHystrixThreadPool"/> instance for a given <see cref="HystrixThreadPoolKey"/>.
        /// If no thread pool exists for the specified key, a new one will be created from the specified setter.
        /// This is thread-safe and ensures only 1 <see cref="IHystrixThreadPool"/> per <see cref="HystrixThreadPoolKey"/>.
        /// </summary>
        /// <param name="threadPoolKey">Key of the requested thread pool.</param>
        /// <param name="setter">The setter to construct the new thread pool.</param>
        /// <returns>A new or an existing thread pool instance.</returns>
        internal static IHystrixThreadPool GetInstance(HystrixThreadPoolKey threadPoolKey, HystrixThreadPoolPropertiesSetter setter)
        {
            return ThreadPools.GetOrAdd(threadPoolKey, w => new HystrixThreadPoolDefault(threadPoolKey, setter));
        }

        /// <summary>
        /// Initiate the shutdown of all <see cref="IHystrixThreadPool"/> instances.
        /// NOTE: This is NOT thread-safe if HystrixCommands are concurrently being executed
        /// and causing thread-pools to initialize while also trying to shutdown.
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        internal static void Shutdown()
        {
            foreach (IHystrixThreadPool pool in ThreadPools.Values)
            {
                pool.Executor.Shutdown();
            }

            ThreadPools.Clear();
        }

        /// <summary>
        /// Initiate the shutdown of all <see cref="IHystrixThreadPool"/> instances and wait up to the given time on each pool to complete.
        /// NOTE: This is NOT thread-safe if HystrixCommands are concurrently being executed
        /// and causing thread-pools to initialize while also trying to shutdown.
        /// </summary>
        /// <param name="timeout">The time to wait for the thread pools to shutdown.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        internal static void Shutdown(TimeSpan timeout)
        {
            foreach (IHystrixThreadPool pool in ThreadPools.Values)
            {
                pool.Executor.Shutdown();
            }

            foreach (IHystrixThreadPool pool in ThreadPools.Values)
            {
                try
                {
                    pool.Executor.AwaitTermination(timeout);
                }
                catch (ThreadInterruptedException e)
                {
                    throw new ThreadInterruptedException("Interrupted while waiting for thread-pools to terminate. Pools may not be correctly shutdown or cleared.", e);
                }
            }

            ThreadPools.Clear();
        }
    }
}
