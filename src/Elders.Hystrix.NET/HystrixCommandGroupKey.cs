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

namespace Elders.Hystrix.NET
{
    using System;

    /// <summary>
    /// A group name for a <see cref="HystrixCommand"/>. This is used for grouping together commands such as for reporting, alerting, dashboards or team/library ownership.
    /// By default this will be used to define the <see cref="HystrixThreadPoolKey"/> unless a separate one is defined.
    /// </summary>
    public class HystrixCommandGroupKey : HystrixKey
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixCommandGroupKey"/> class.
        /// </summary>
        /// <param name="name">The name of the command group key.</param>
        public HystrixCommandGroupKey(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Converts a string to a <see cref="HystrixCommandGroupKey"/> object.
        /// </summary>
        /// <param name="name">The name of the command group key.</param>
        /// <returns>A <see cref="HystrixCommandGroupKey"/> object constructed from the specified name.</returns>
        public static implicit operator HystrixCommandGroupKey(string name)
        {
            return new HystrixCommandGroupKey(name);
        }
    }
}
