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

namespace Netflix.Hystrix.Util
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Provides helper methods for the <see cref="HystrixRollingNumberEvent"/> enumeration.
    /// </summary>
    public static class HystrixRollingNumberEventExtensions
    {
        /// <summary>
        /// All possible values of the <see cref="HystrixRollingNumberEvent"/> enumeration.
        /// </summary>
        public static readonly IReadOnlyCollection<HystrixRollingNumberEvent> Values = Enum.GetValues(typeof(HystrixRollingNumberEvent)).Cast<HystrixRollingNumberEvent>().ToList();

        /// <summary>
        /// Gets whether the specified event is a Counter type or not.
        /// </summary>
        /// <param name="rollingNumberEvent">The specified event.</param>
        /// <returns>True if it's a Counter type, otherwise false.</returns>
        public static bool IsCounter(this HystrixRollingNumberEvent rollingNumberEvent)
        {
            return !rollingNumberEvent.IsMaxUpdater();
        }

        /// <summary>
        /// Gets whether the specified event is a MaxUpdater type or not.
        /// </summary>
        /// <param name="rollingNumberEvent">The specified event.</param>
        /// <returns>True if it's a MaxUpdater type, otherwise false.</returns>
        public static bool IsMaxUpdater(this HystrixRollingNumberEvent rollingNumberEvent)
        {
            return rollingNumberEvent == HystrixRollingNumberEvent.ThreadMaxActive;
        }
    }
}
