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
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Provides a method to get the current time. It uses <see cref="Stopwatch"/>.
    /// </summary>
    internal class ActualTime : ITime
    {
        /// <summary>
        /// The singleton instance of the <see cref="ActualTime"/> class.
        /// </summary>
        public static readonly ActualTime Instance = new ActualTime();

        /// <summary>
        /// The instance which provides the accurate time measurement.
        /// </summary>
        private Stopwatch stopwatch;

        /// <summary>
        /// Prevents a default instance of the <see cref="ActualTime"/> class from being created.
        /// </summary>
        private ActualTime()
        {
            this.stopwatch = new Stopwatch();
            this.stopwatch.Start();
        }

        /// <summary>
        /// Gets the current time in milliseconds.
        /// </summary>
        public static long CurrentTimeInMillis
        {
            get
            {
                return Instance.GetCurrentTimeInMillis();
            }
        }

        /// <summary>
        /// Gets the current time in milliseconds.
        /// </summary>
        /// <returns>The current time in milliseconds.</returns>
        public long GetCurrentTimeInMillis()
        {
            return this.stopwatch.ElapsedMilliseconds;
        }
    }
}
