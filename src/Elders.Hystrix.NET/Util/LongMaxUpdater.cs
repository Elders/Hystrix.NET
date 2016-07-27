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
    using Java.Util.Concurrent.Atomic;

    /// <summary>
    /// This class is currently uses <see cref="AtomicLong"/> to calculate the maximum of a series of number.
    /// The original implementation derive from <see cref="Striped64"/> to implement a counter with better throughput under high contention.
    /// </summary>
    public class LongMaxUpdater
    {
        /// <summary>
        /// The <see cref="AtomicLong"/> instance storing the maximum.
        /// </summary>
        private readonly AtomicLong value = new AtomicLong(long.MinValue);

        /// <summary>
        /// Updates the maximum if the specified number is greater than the maximum.
        /// </summary>
        /// <param name="x">The number to update with.</param>
        public void Update(long x)
        {
            while (true)
            {
                long current = this.value.Value;
                if (current >= x)
                {
                    return;
                }

                if (this.value.CompareAndSet(current, x))
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Gets the maximum.
        /// </summary>
        /// <returns>The maximum.</returns>
        public long Max()
        {
            return this.value.Value;
        }

        /// <summary>
        /// Sets the maximum to 0.
        /// </summary>
        public void Reset()
        {
            this.value.GetAndSet(0);
        }

        /// <summary>
        /// Gets the maximum and sets it to 0.
        /// </summary>
        /// <returns>The maximum.</returns>
        public long MaxThenReset()
        {
            return this.value.GetAndSet(0);
        }

        /// <summary>
        /// Gets the string representation of the maximum.
        /// </summary>
        /// <returns>The string representation of the maximum.</returns>
        public override string ToString()
        {
            return this.value.ToString();
        }
    }
}
