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
    /// A key to represent a <see cref="HystrixThreadPool"/> for monitoring, metrics publishing, caching and other such uses.
    /// </summary>
    public class HystrixThreadPoolKey : HystrixKey
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixThreadPoolKey"/> class.
        /// </summary>
        /// <param name="name">The name of the thread pool key.</param>
        public HystrixThreadPoolKey(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Converts a string to a <see cref="HystrixThreadPoolKey"/> object.
        /// </summary>
        /// <param name="name">The name of the thread pool key.</param>
        /// <returns>A <see cref="HystrixThreadPoolKey"/> object constructed from the specified name.</returns>
        public static implicit operator HystrixThreadPoolKey(string name)
        {
            return new HystrixThreadPoolKey(name);
        }

        /// <summary>
        /// Converts a string to a <see cref="HystrixThreadPoolKey"/> object.
        /// </summary>
        /// <param name="name">The name of the thread pool key.</param>
        /// <returns>A <see cref="HystrixThreadPoolKey"/> object constructed from the specified name.</returns>
        public static HystrixThreadPoolKey FromString(string name)
        {
            return new HystrixThreadPoolKey(name);
        }
    }
}
