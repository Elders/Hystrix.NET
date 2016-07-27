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
    /// This class is currently equivalent to <see cref="AtomicLong"/>. The original implementation derive from <see cref="Striped64"/>
    /// to implement a counter with better throughput under high contention.
    /// </summary>
    public class LongAdder
    {
        /// <summary>
        /// The <see cref="AtomicLong"/> instance storing the sum.
        /// </summary>
        private readonly AtomicLong value = new AtomicLong(0);

        /// <summary>
        /// Increases the sum by the specified amount.
        /// </summary>
        /// <param name="x">The number to add.</param>
        public void Add(long x)
        {
            this.value.AddAndGet(x);
        }

        /// <summary>
        /// Increments the sum by 1.
        /// </summary>
        public void Increment()
        {
            this.Add(1);
        }

        /// <summary>
        /// Decrements the sum by 1.
        /// </summary>
        public void Decrement()
        {
            this.Add(-1);
        }

        /// <summary>
        /// Gets the sum.
        /// </summary>
        /// <returns>The sum.</returns>
        public long Sum()
        {
            return this.value.Value;
        }

        /// <summary>
        /// Sets the sum to 0.
        /// </summary>
        public void Reset()
        {
            this.value.GetAndSet(0);
        }

        /// <summary>
        /// Gets the current sum and sets it to 0.
        /// </summary>
        /// <returns>The current sum.</returns>
        public long SumThenReset()
        {
            return this.value.GetAndSet(0);
        }

        /// <summary>
        /// Gets the string representation of the sum.
        /// </summary>
        /// <returns>The string representation of the sum.</returns>
        public override string ToString()
        {
            return this.value.ToString();
        }
    }
}
