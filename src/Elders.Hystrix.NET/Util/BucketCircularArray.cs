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
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Java.Util.Concurrent.Atomic;

    /// <summary>
    /// This is a circular array acting as a FIFO queue.
    /// </summary>
    /// <remarks>
    /// <para>
    /// It purposefully does NOT implement Queue or some other Collection interface as it only implements functionality necessary for this RollingNumber use case.
    /// </para>
    /// <para>
    /// Important Thread-Safety Note: This is ONLY thread-safe within the context of RollingNumber and the protection it gives in the <see cref="GetCurrentBucket"/> method. It uses AtomicReference
    /// objects to ensure anything done outside of <see cref="GetCurrentBucket"/> is thread-safe, and to ensure visibility of changes across threads (volatility) but the <see cref="AddLast"/> and <see cref="RemoveFirst"/>
    /// methods are NOT thread-safe for external access they depend upon the lock.tryLock() protection in <see cref="GetCurrentBucket"/> which ensures only a single thread will access them at at time.
    /// </para>
    /// <para>
    /// This implementation was chosen based on performance testing Ben J. Christensen did and documented at: http://benjchristensen.com/2011/10/08/atomiccirculararray/
    /// </para>
    /// </remarks>
    /// <typeparam name="T">The type of the items to store in this bucket.</typeparam>
    internal class BucketCircularArray<T> : IEnumerable<T> where T : class
    {
        /// <summary>
        /// An atomic reference of the actual list state.
        /// </summary>
        private readonly AtomicReference<BucketCircularArrayListState> state;

        /// <summary>
        /// Initializes a new instance of the <see cref="BucketCircularArray{T}"/> class.
        /// </summary>
        /// <param name="size">The maximum number of buckets to store.</param>
        public BucketCircularArray(int size)
        {
            AtomicReferenceArray<T> buckets = new AtomicReferenceArray<T>(size + 1); // + 1 as extra room for the add/remove
            BucketCircularArrayListState listState = new BucketCircularArrayListState(buckets, 0, 0);
            this.state = new AtomicReference<BucketCircularArrayListState>(listState);
        }

        /// <summary>
        /// Gets the number of buckets currently stored.
        /// </summary>
        public int Size
        {
            get
            {
                // the size can also be worked out each time as:
                // return (tail + data.length() - head) % data.length();
                return this.state.Value.Size;
            }
        }

        /// <summary>
        /// Clears the array.
        /// </summary>
        public void Clear()
        {
            while (true)
            {
                // it should be very hard to not succeed the first pass thru since this is typically is only called from
                // a single thread protected by a tryLock, but there is at least 1 other place (at time of writing this comment)
                // where reset can be called from (CircuitBreaker.markSuccess after circuit was tripped) so it can
                // in an edge-case conflict.
                // 
                // Instead of trying to determine if someone already successfully called clear() and we should skip
                // we will have both calls reset the circuit, even if that means losing data added in between the two
                // depending on thread scheduling.
                // 
                // The rare scenario in which that would occur, we'll accept the possible data loss while clearing it
                // since the code has stated its desire to clear() anyways.
                BucketCircularArrayListState current = this.state.Value;
                BucketCircularArrayListState newState = current.Clear();
                if (this.state.CompareAndSet(current, newState))
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Gets the enumerator for the buckets.
        /// </summary>
        /// <returns>The enumerator for the buckets.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return this.GetArray().ToList().GetEnumerator();
        }

        /// <summary>
        /// Gets the enumerator for the buckets.
        /// </summary>
        /// <returns>The enumerator for the buckets.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Appends an item to the end of the circular array.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void AddLast(T item)
        {
            BucketCircularArrayListState currentState = this.state.Value;
            BucketCircularArrayListState newState = currentState.AddBucket(item);

            // use CompareAndSet to set in case multiple threads are attempting (which shouldn't be the case
            // because since AddLast will ONLY be called by a single thread at a time due to protection
            // provided in GetCurrentBucket)
            if (this.state.CompareAndSet(currentState, newState))
            {
                // we succeeded
                return;
            }
            else
            {
                // we failed, someone else was adding or removing
                // instead of trying again and risking multiple AddLast concurrently (which shouldn't be the case)
                // we'll just return and let the other thread 'win' and if the timing is off the next call to GetCurrentBucket will fix things
                return;
            }
        }

        /// <summary>
        /// Gets the tail item of this circular array.
        /// </summary>
        /// <returns>The last item.</returns>
        public T GetLast()
        {
            return this.PeekLast();
        }

        /// <summary>
        /// Gets the tail item of this circular array.
        /// </summary>
        /// <returns>The last item.</returns>
        public T PeekLast()
        {
            return this.state.Value.Tail;
        }

        /// <summary>
        /// Creates a copy of the internal array.
        /// </summary>
        /// <returns>The array of items.</returns>
        public T[] GetArray()
        {
            return this.state.Value.GetArray();
        }

        /// <summary>
        /// Immutable object that is atomically set every time the state of the BucketCircularArray changes.
        /// This handles the compound operations.
        /// </summary>
        private class BucketCircularArrayListState
        {
            /*
            * this is an AtomicReferenceArray and not a normal Array because we're copying the reference
            * between ListState objects and multiple threads could maintain references across these
            * compound operations so I want the visibility/concurrency guarantees
            */

            /// <summary>
            /// The array which contains the items of the circular array.
            /// </summary>
            private readonly AtomicReferenceArray<T> data;

            /// <summary>
            /// The currently stored number of items of the array.
            /// </summary>
            private readonly int size;

            /// <summary>
            /// The array index of the tail.
            /// </summary>
            private readonly int tail;

            /// <summary>
            /// The array index of the head.
            /// </summary>
            private readonly int head;

            /// <summary>
            /// The actual length if the data array, which is the number of buckets to store + 1.
            /// </summary>
            private readonly int dataLength;

            /// <summary>
            /// The maximum number of buckets to store.
            /// </summary>
            private readonly int numBuckets;

            /// <summary>
            /// Initializes a new instance of the <see cref="BucketCircularArrayListState"/> class.
            /// </summary>
            /// <param name="data">The internal array storing the items.</param>
            /// <param name="head">The index of the head.</param>
            /// <param name="tail">The index of the tail.</param>
            public BucketCircularArrayListState(AtomicReferenceArray<T> data, int head, int tail)
            {
                this.head = head;
                this.tail = tail;
                this.dataLength = data.Length;
                this.numBuckets = data.Length - 1;
                if (head == 0 && tail == 0)
                {
                    this.size = 0;
                }
                else
                {
                    this.size = (tail + this.dataLength - head) % this.dataLength;
                }

                this.data = data;
            }

            /// <summary>
            /// Gets the number of buckets currently stored.
            /// </summary>
            public int Size
            {
                get { return this.size; }
            }

            /// <summary>
            /// Gets the tail item of the circular item.
            /// </summary>
            public T Tail
            {
                get
                {
                    if (this.size == 0)
                    {
                        return null;
                    }
                    else
                    {
                        // we want to get the last item, so size() - 1
                        return this.data[this.Convert(this.size - 1)];
                    }
                }
            }

            /// <summary>
            /// Creates a copy of the internal array.
            /// </summary>
            /// <returns>The array of items.</returns>
            public T[] GetArray()
            {
                // this isn't technically thread-safe since it requires multiple reads on something that can change
                // but since we never clear the data directly, only increment/decrement head/tail we would never get a NULL
                // just potentially return stale data which we are okay with doing
                List<T> array = new List<T>();
                for (int i = 0; i < this.size; i++)
                {
                    array.Add(this.data[this.Convert(i)]);
                }

                return array.ToArray();
            }

            /// <summary>
            /// Creates an empty list state.
            /// </summary>
            /// <returns>The clear list state.</returns>
            public BucketCircularArrayListState Clear()
            {
                return new BucketCircularArrayListState(new AtomicReferenceArray<T>(this.dataLength), 0, 0);
            }

            /// <summary>
            /// Adds a new item to the circular array and increments the tail.
            /// </summary>
            /// <param name="item">The item to add.</param>
            /// <returns>The new circular array state with the added bucket.</returns>
            public BucketCircularArrayListState AddBucket(T item)
            {
                // We could in theory have 2 threads AddBucket concurrently and this compound operation would interleave.
                // This should NOT happen since GetCurrentBucket is supposed to be executed by a single thread.
                // If it does happen, it's not a huge deal as IncrementTail() will be protected by CompareAndSet and one of the two AddBucket calls will succeed with one of the Buckets.
                // In either case, a single Bucket will be returned as "last" and data loss should not occur and everything keeps in sync for head/tail.
                // Also, it's fine to set it before IncrementTail because nothing else should be referencing that index position until IncrementTail occurs.
                this.data[this.tail] = item;
                return this.IncrementTail();
            }

            /// <summary>
            /// Increments the tail and if necessary the head too.
            /// </summary>
            /// <returns>The new list state with incremented tail.</returns>
            private BucketCircularArrayListState IncrementTail()
            {
                // if incrementing results in growing larger than 'length' which is the max we should be at,
                // then also increment head (equivalent of removeFirst but done atomically)
                if (this.size == this.numBuckets)
                {
                    // increment tail and head
                    return new BucketCircularArrayListState(this.data, (this.head + 1) % this.dataLength, (this.tail + 1) % this.dataLength);
                }
                else
                {
                    // increment only tail
                    return new BucketCircularArrayListState(this.data, this.head, (this.tail + 1) % this.dataLength);
                }
            }

            /// <summary>
            /// Converts a logical index to the physical index of the array.
            /// </summary>
            /// <param name="index">The logical index.</param>
            /// <returns>The physical index.</returns>
            private int Convert(int index)
            {
                return (index + this.head) % this.dataLength;
            }
        }
    }
}
