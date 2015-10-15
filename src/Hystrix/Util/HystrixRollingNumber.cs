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
    using System.Threading;

    /// <summary>
    /// <para>
    /// A number which can be used to track counters (increment) or set values over time.
    /// </para>
    /// <para>
    /// It is "rolling" in the sense that a 'timeInMilliseconds' is given that you want to track (such as 10 seconds) and then that is broken into buckets (defaults to 10) so that the 10 second window
    /// doesn't empty out and restart every 10 seconds, but instead every 1 second you have a new bucket added and one dropped so that 9 of the buckets remain and only the newest starts from scratch.
    /// </para>
    /// <para>
    /// This is done so that the statistics are gathered over a rolling 10 second window with data being added/dropped in 1 second intervals (or whatever granularity is defined by the arguments) rather
    /// than each 10 second window starting at 0 again.
    /// </para>
    /// <para>
    /// Performance-wise this class is optimized for writes, not reads. This is done because it expects far higher write volume (thousands/second) than reads (a few per second).
    /// </para>
    /// <para>
    /// For example, on each read to getSum/getCount it will iterate buckets to sum the data so that on writes we don't need to maintain the overall sum and pay the synchronization cost at each write to
    /// ensure the sum is up-to-date when the read can easily iterate each bucket to get the sum when it needs it.
    /// </para>
    /// </summary>
    public class HystrixRollingNumber
    {
        /// <summary>
        /// The object used to synchronize the <see cref="GetCurrentBucket"/> method.
        /// </summary>
        private readonly object newBucketLock = new object();

        /// <summary>
        /// The <see cref="ITime"/> instance to measure time.
        /// </summary>
        private readonly ITime time;

        /// <summary>
        /// The total time window to track.
        /// </summary>
        private readonly IHystrixProperty<int> timeInMilliseconds;

        /// <summary>
        /// The number of parts to break the time window.
        /// </summary>
        private readonly IHystrixProperty<int> numberOfBuckets;

        /// <summary>
        /// The cumulative sum of events of all time.
        /// </summary>
        private readonly CumulativeSum cumulativeSum = new CumulativeSum();

        /// <summary>
        /// The array to store the event counters of the specified time window.
        /// </summary>
        private readonly BucketCircularArray<Bucket> buckets;

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixRollingNumber"/> class.
        /// </summary>
        /// <param name="timeInMilliseconds">The total time window to track.</param>
        /// <param name="numberOfBuckets">The number of parts to break the time window.</param>
        public HystrixRollingNumber(IHystrixProperty<int> timeInMilliseconds, IHystrixProperty<int> numberOfBuckets)
            : this(ActualTime.Instance, timeInMilliseconds, numberOfBuckets)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixRollingNumber"/> class. This constructor is used for unit testing
        /// to be able to inject mocked time measurement.
        /// </summary>
        /// <param name="time">The <see cref="ITime"/> instance to measure time.</param>
        /// <param name="timeInMilliseconds">The total time window to track.</param>
        /// <param name="numberOfBuckets">The number of parts to break the time window.</param>
        internal HystrixRollingNumber(ITime time, int timeInMilliseconds, int numberOfBuckets)
            : this(time, HystrixPropertyFactory.AsProperty(timeInMilliseconds), HystrixPropertyFactory.AsProperty(numberOfBuckets))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixRollingNumber"/> class.
        /// </summary>
        /// <param name="time">The <see cref="ITime"/> instance to measure time.</param>
        /// <param name="timeInMilliseconds">The total time window to track.</param>
        /// <param name="numberOfBuckets">The number of parts to break the time window.</param>
        private HystrixRollingNumber(ITime time, IHystrixProperty<int> timeInMilliseconds, IHystrixProperty<int> numberOfBuckets)
        {
            this.time = time;
            this.timeInMilliseconds = timeInMilliseconds;
            this.numberOfBuckets = numberOfBuckets;

            if (timeInMilliseconds.Get() % numberOfBuckets.Get() != 0)
            {
                throw new ArgumentException("The timeInMilliseconds must divide equally into numberOfBuckets. For example 1000/10 is ok, 1000/11 is not.");
            }

            this.buckets = new BucketCircularArray<Bucket>(numberOfBuckets.Get());
        }

        /// <summary>
        /// Gets the total window size in milliseconds.
        /// </summary>
        public int TimeInMilliseconds
        {
            get { return this.timeInMilliseconds.Get(); }
        }

        /// <summary>
        /// Gets the number of parts to break the time window.
        /// </summary>
        public int NumberOfBuckets
        {
            get { return this.numberOfBuckets.Get(); }
        }

        /// <summary>
        /// Gets the bucket size in milliseconds. (TimeInMilliseconds / NumberOfBuckets)
        /// </summary>
        public int BucketSizeInMilliseconds
        {
            get { return this.timeInMilliseconds.Get() / this.numberOfBuckets.Get(); }
        }

        /// <summary>
        /// Gets the internal circular array to store the buckets. This property is intended to use only in unit tests.
        /// </summary>
        internal BucketCircularArray<Bucket> Buckets
        {
            get { return this.buckets; }
        }

        /// <summary>
        /// Increment the counter in the current bucket by one for the given <see cref="HystrixRollingNumberEvent"/> type.
        /// </summary>
        /// <param name="type">Defining which counter to increment, must be a "Counter" type (HystrixRollingNumberEvent.IsCounter() == true).</param>
        public void Increment(HystrixRollingNumberEvent type)
        {
            this.GetCurrentBucket().GetAdder(type).Increment();
        }

        /// <summary>
        /// Add to the counter in the current bucket for the given <see cref="HystrixRollingNumberEvent"/> type.
        /// </summary>
        /// <param name="type">Defining which counter to add to, must be a "Counter" type (HystrixRollingNumberEvent.IsCounter() == true).</param>
        /// <param name="value">Value to be added to the current bucket.</param>
        public void Add(HystrixRollingNumberEvent type, long value)
        {
            this.GetCurrentBucket().GetAdder(type).Add(value);
        }

        /// <summary>
        /// Update a value and retain the max value.
        /// </summary>
        /// <param name="type">Defining which counter to update, must be a "MaxUpdater" type (HystrixRollingNumberEvent.IsMaxUpdater() == true).</param>
        /// <param name="value">Value to be updated to the current bucket</param>
        public void UpdateRollingMax(HystrixRollingNumberEvent type, long value)
        {
            this.GetCurrentBucket().GetMaxUpdater(type).Update(value);
        }

        /// <summary>
        /// Force a reset of all rolling counters (clear all buckets) so that statistics start being gathered from scratch.
        /// This does NOT reset the CumulativeSum values.
        /// </summary>
        public void Reset()
        {
            // if we are resetting, that means the lastBucket won't have a chance to be captured in CumulativeSum, so let's do it here
            Bucket lastBucket = this.buckets.PeekLast();
            if (lastBucket != null)
            {
                this.cumulativeSum.AddBucket(lastBucket);
            }

            // clear buckets so we start over again
            this.buckets.Clear();
        }

        /// <summary>
        /// Get the cumulative sum of all buckets ever since the application started without rolling for the given
        /// <see cref="HystrixRollingNumberEvent"/> type. See <see cref="GetRollingSum(HystrixRollingNumberEvent)"/> for the rolling sum.
        /// </summary>
        /// <param name="type">Must be a "Counter" type (HystrixRollingNumberEvent.IsCounter() == true).</param>
        /// <returns>Cumulative sum of all increments and adds for the given <see cref="HystrixRollingNumberEvent"/> counter type.</returns>
        public long GetCumulativeSum(HystrixRollingNumberEvent type)
        {
            // This isn't 100% atomic since multiple threads can be affecting latestBucket & cumulativeSum independently
            // but that's okay since the count is always a moving target and we're accepting a "point in time" best attempt
            // we are however putting 'GetValueOfLatestBucket' first since it can have side-affects on cumulativeSum whereas the inverse is not true
            return this.GetValueOfLatestBucket(type) + this.cumulativeSum.Get(type);
        }

        /// <summary>
        /// Get the sum of all buckets in the rolling counter for the given <see cref="HystrixRollingNumberEvent"/> type.
        /// The <see cref="HystrixRollingNumberEvent"/> must be a "Counter" type (HystrixRollingNumberEvent.IsCounter() == true).
        /// </summary>
        /// <param name="type">defining which counter to retrieve values from</param>
        /// <returns>Value from the given <see cref="HystrixRollingNumberEvent"/> counter type.</returns>
        public long GetRollingSum(HystrixRollingNumberEvent type)
        {
            if (this.GetCurrentBucket() == null)
            {
                return 0;
            }

            long sum = 0;
            foreach (Bucket b in this.buckets)
            {
                sum += b.GetAdder(type).Sum();
            }

            return sum;
        }

        /// <summary>
        /// Get the value of the latest (current) bucket in the rolling counter for the given <see cref="HystrixRollingNumberEvent"/> type.
        /// The <see cref="HystrixRollingNumberEvent"/> must be a "Counter" type (HystrixRollingNumberEvent.IsCounter() == true).
        /// </summary>
        /// <param name="type">HystrixRollingNumberEvent defining which counter to retrieve value from</param>
        /// <returns>value from latest bucket for given <see cref="HystrixRollingNumberEvent"/> counter type</returns>
        public long GetValueOfLatestBucket(HystrixRollingNumberEvent type)
        {
            Bucket lastBucket = this.GetCurrentBucket();
            if (lastBucket == null)
            {
                return 0;
            }

            // we have bucket data so we'll return the lastBucket
            return lastBucket.Get(type);
        }

        /// <summary>
        /// Get an array of values for all buckets in the rolling counter for the given <see cref="HystrixRollingNumberEvent"/> type.
        /// Index 0 is the oldest bucket.
        /// The <see cref="HystrixRollingNumberEvent"/> must be a "Counter" type (HystrixRollingNumberEvent.IsCounter() == true).
        /// </summary>
        /// <param name="type">HystrixRollingNumberEvent defining which counter to retrieve values from</param>
        /// <returns>Array of values from each of the rolling buckets for given <see cref="HystrixRollingNumberEvent"/> counter type</returns>
        public long[] GetValues(HystrixRollingNumberEvent type)
        {
            if (this.GetCurrentBucket() == null)
            {
                return new long[0];
            }

            // get buckets as an array (which is a copy of the current state at this point in time)
            Bucket[] bucketArray = this.buckets.GetArray();

            // we have bucket data so we'll return an array of values for all buckets
            long[] values = new long[bucketArray.Length];
            int i = 0;
            foreach (Bucket bucket in bucketArray)
            {
                if (type.IsCounter())
                {
                    values[i++] = bucket.GetAdder(type).Sum();
                }
                else if (type.IsMaxUpdater())
                {
                    values[i++] = bucket.GetMaxUpdater(type).Max();
                }
            }

            return values;
        }

        /// <summary>
        /// Get the max value of values in all buckets for the given <see cref="HystrixRollingNumberEvent"/> type.
        /// The <see cref="HystrixRollingNumberEvent"/> must be a "MaxUpdater" type (HystrixRollingNumberEvent.IsMaxUpdater() == true).
        /// </summary>
        /// <param name="type">HystrixRollingNumberEvent defining which "MaxUpdater" to retrieve values from</param>
        /// <returns>Max value for given <see cref="HystrixRollingNumberEvent"/> type during rolling window</returns>
        public long GetRollingMaxValue(HystrixRollingNumberEvent type)
        {
            long[] values = this.GetValues(type);
            if (values.Length == 0)
            {
                return 0;
            }
            else
            {
                Array.Sort(values);
                return values[values.Length - 1];
            }
        }

        /// <summary>
        /// Gets the current bucket. If the time is after the window of the current bucket, a new one will be created.
        /// Internal because it's used in unit tests.
        /// </summary>
        /// <returns>The current bucket.</returns>
        internal Bucket GetCurrentBucket()
        {
            long currentTime = this.time.GetCurrentTimeInMillis();

            // A shortcut to try and get the most common result of immediately finding the current bucket.
            // Retrieve the latest bucket if the given time is BEFORE the end of the bucket window, otherwise it returns NULL.
            Bucket currentBucket = this.buckets.PeekLast();
            if (currentBucket != null && currentTime < currentBucket.WindowStart + this.BucketSizeInMilliseconds)
            {
                // If we're within the bucket 'window of time' return the current one
                // NOTE: We do not worry if we are BEFORE the window in a weird case of where thread scheduling causes that to occur,
                // we'll just use the latest as long as we're not AFTER the window
                return currentBucket;
            }

            // If we didn't find the current bucket above, then we have to create one:
            //
            // The following needs to be synchronized/locked even with a synchronized/thread-safe data structure such as LinkedBlockingDeque because
            // the logic involves multiple steps to check existence, create an object then insert the object. The 'check' or 'insertion' themselves
            // are thread-safe by themselves but not the aggregate algorithm, thus we put this entire block of logic inside synchronized.
            // 
            // I am using a tryLock if/then (http://download.oracle.com/javase/6/docs/api/java/util/concurrent/locks/Lock.html#tryLock())
            // so that a single thread will get the lock and as soon as one thread gets the lock all others will go the 'else' block
            // and just return the currentBucket until the newBucket is created. This should allow the throughput to be far higher
            // and only slow down 1 thread instead of blocking all of them in each cycle of creating a new bucket based on some testing
            // (and it makes sense that it should as well).
            // 
            // This means the timing won't be exact to the millisecond as to what data ends up in a bucket, but that's acceptable.
            // It's not critical to have exact precision to the millisecond, as long as it's rolling, if we can instead reduce the impact synchronization.
            // 
            // More importantly though it means that the 'if' block within the lock needs to be careful about what it changes that can still
            // be accessed concurrently in the 'else' block since we're not completely synchronizing access.
            // 
            // For example, we can't have a multi-step process to add a bucket, remove a bucket, then update the sum since the 'else' block of code
            // can retrieve the sum while this is all happening. The trade-off is that we don't maintain the rolling sum and let readers just iterate
            // bucket to calculate the sum themselves. This is an example of favoring write-performance instead of read-performance and how the tryLock
            // versus a synchronized block needs to be accommodated.
            if (Monitor.TryEnter(this.newBucketLock))
            {
                try
                {
                    if (this.buckets.PeekLast() == null)
                    {
                        // the list is empty so create the first bucket
                        Bucket newBucket = new Bucket(currentTime);
                        this.buckets.AddLast(newBucket);
                        return newBucket;
                    }
                    else
                    {
                        // We go into a loop so that it will create as many buckets as needed to catch up to the current time
                        // as we want the buckets complete even if we don't have transactions during a period of time.
                        for (int i = 0; i < this.NumberOfBuckets; i++)
                        {
                            // We have at least 1 bucket so retrieve it
                            Bucket lastBucket = this.buckets.PeekLast();
                            if (currentTime < lastBucket.WindowStart + this.BucketSizeInMilliseconds)
                            {
                                // If we're within the bucket 'window of time' return the current one
                                // NOTE: We do not worry if we are BEFORE the window in a weird case of where thread scheduling causes that to occur,
                                // we'll just use the latest as long as we're not AFTER the window
                                return lastBucket;
                            }
                            else if (currentTime - (lastBucket.WindowStart + this.BucketSizeInMilliseconds) > this.TimeInMilliseconds)
                            {
                                // The time passed is greater than the entire rolling counter so we want to clear it all and start from scratch
                                this.Reset();

                                // Recursively call GetCurrentBucket which will create a new bucket and return it.
                                return this.GetCurrentBucket();
                            }
                            else
                            {
                                // We're past the window so we need to create a new bucket.
                                // Create a new bucket and add it as the new 'last'
                                this.buckets.AddLast(new Bucket(lastBucket.WindowStart + this.BucketSizeInMilliseconds));

                                // Add the lastBucket values to the cumulativeSum
                                this.cumulativeSum.AddBucket(lastBucket);
                            }
                        }

                        // We have finished the for-loop and created all of the buckets, so return the lastBucket now.
                        return this.buckets.PeekLast();
                    }
                }
                finally
                {
                    Monitor.Exit(this.newBucketLock);
                }
            }
            else
            {
                currentBucket = this.buckets.PeekLast();
                if (currentBucket != null)
                {
                    // we didn't get the lock so just return the latest bucket while another thread creates the next one
                    return currentBucket;
                }
                else
                {
                    // The rare scenario where multiple threads raced to create the very first bucket.
                    // Wait slightly and then use recursion while the other thread finishes creating a bucket.
                    Thread.Sleep(5);
                    return this.GetCurrentBucket();
                }
            }
        }

        /// <summary>
        /// Counters for a given 'bucket' of time.
        /// </summary>
        internal class Bucket
        {
            /// <summary>
            /// Stores the time of the start of this bucket.
            /// </summary>
            private readonly long windowStart;

            /// <summary>
            /// <see cref="LongAdder"/> instances for each <see cref="HystrixRollingNumberEvent"/>.
            /// </summary>
            private readonly LongAdder[] adderForCounterType;

            /// <summary>
            /// <see cref="LongMaxUpdater"/> instances for each <see cref="HystrixRollingNumberEvent"/>.
            /// </summary>
            private readonly LongMaxUpdater[] updaterForCounterType;

            /// <summary>
            /// Initializes a new instance of the <see cref="Bucket"/> class.
            /// </summary>
            /// <param name="startTime">The time of start of this bucket.</param>
            public Bucket(long startTime)
            {
                this.windowStart = startTime;

                // We support both LongAdder and LongMaxUpdater in a bucket but don't want the memory allocation
                // of all types for each so we only allocate the objects if the HystrixRollingNumberEvent matches
                // the correct type - though we still have the allocation of empty arrays to the given length
                // as we want to keep using the (int)type value for fast random access.
                this.adderForCounterType = new LongAdder[HystrixRollingNumberEventExtensions.Values.Count];
                foreach (HystrixRollingNumberEvent type in HystrixRollingNumberEventExtensions.Values)
                {
                    if (type.IsCounter())
                    {
                        this.adderForCounterType[(int)type] = new LongAdder();
                    }
                }

                this.updaterForCounterType = new LongMaxUpdater[HystrixRollingNumberEventExtensions.Values.Count];
                foreach (HystrixRollingNumberEvent type in HystrixRollingNumberEventExtensions.Values)
                {
                    if (type.IsMaxUpdater())
                    {
                        this.updaterForCounterType[(int)type] = new LongMaxUpdater();

                        // initialize to 0 otherwise it is Long.MIN_VALUE
                        this.updaterForCounterType[(int)type].Update(0);
                    }
                }
            }

            /// <summary>
            /// Gets the time of start of this bucket.
            /// </summary>
            public long WindowStart
            {
                get { return this.windowStart; }
            }

            /// <summary>
            /// Gets the value for the specified <see cref="HystrixRollingNumberEvent"/> in this bucket.
            /// (Returns <see cref="LongAdder.Sum()"/> for Counter types and <see cref="LongMaxUpdater.Max()"/> for MaxUpdater types.)
            /// </summary>
            /// <param name="type">The specified event.</param>
            /// <returns>The value for the specified event in this bucket.</returns>
            public long Get(HystrixRollingNumberEvent type)
            {
                if (type.IsCounter())
                {
                    return this.adderForCounterType[(int)type].Sum();
                }

                if (type.IsMaxUpdater())
                {
                    return this.updaterForCounterType[(int)type].Max();
                }

                throw new ArgumentException(string.Format("Unknown type of event: {0}", type), "type");
            }

            /// <summary>
            /// Gets the <see cref="LongAdder"/> instance for the specified event.
            /// </summary>
            /// <param name="type">The specified event.</param>
            /// <returns>The <see cref="LongAdder"/> instance for the specified event.</returns>
            public LongAdder GetAdder(HystrixRollingNumberEvent type)
            {
                if (!type.IsCounter())
                {
                    throw new ArgumentException(string.Format("Type is not a Counter: {0}", type), "type");
                }

                return this.adderForCounterType[(int)type];
            }

            /// <summary>
            /// Gets the <see cref="LongMaxUpdater"/> instance for the specified event.
            /// </summary>
            /// <param name="type">The specified event.</param>
            /// <returns>The <see cref="LongMaxUpdater"/> instance for the specified event.</returns>
            public LongMaxUpdater GetMaxUpdater(HystrixRollingNumberEvent type)
            {
                if (!type.IsMaxUpdater())
                {
                    throw new ArgumentException(string.Format("Type is not a MaxUpdater: {0}", type), "type");
                }

                return this.updaterForCounterType[(int)type];
            }
        }

        /// <summary>
        /// Cumulative counters (all time) for each <see cref="HystrixRollingNumberEvent"/> used in <see cref="HystrixRollingNumber"/>.
        /// </summary>
        private class CumulativeSum
        {
            /// <summary>
            /// <see cref="LongAdder"/> instances for each <see cref="HystrixRollingNumberEvent"/>.
            /// </summary>
            private readonly LongAdder[] adderForCounterType;

            /// <summary>
            /// <see cref="LongMaxUpdater"/> instances for each <see cref="HystrixRollingNumberEvent"/>.
            /// </summary>
            private readonly LongMaxUpdater[] updaterForCounterType;

            /// <summary>
            /// Initializes a new instance of the <see cref="CumulativeSum"/> class.
            /// </summary>
            public CumulativeSum()
            {
                // We support both LongAdder and LongMaxUpdater in a bucket but don't want the memory allocation
                // of all types for each so we only allocate the objects if the HystrixRollingNumberEvent matches
                // the correct type - though we still have the allocation of empty arrays to the given length
                // as we want to keep using the (int)type value for fast random access.
                this.adderForCounterType = new LongAdder[HystrixRollingNumberEventExtensions.Values.Count];
                foreach (HystrixRollingNumberEvent type in HystrixRollingNumberEventExtensions.Values)
                {
                    if (type.IsCounter())
                    {
                        this.adderForCounterType[(int)type] = new LongAdder();
                    }
                }

                this.updaterForCounterType = new LongMaxUpdater[HystrixRollingNumberEventExtensions.Values.Count];
                foreach (HystrixRollingNumberEvent type in HystrixRollingNumberEventExtensions.Values)
                {
                    if (type.IsMaxUpdater())
                    {
                        this.updaterForCounterType[(int)type] = new LongMaxUpdater();

                        // initialize to 0 otherwise it is long.MinValue
                        this.updaterForCounterType[(int)type].Update(0);
                    }
                }
            }

            /// <summary>
            /// Updates the cumulative values by the values of a <see cref="Bucket"/>.
            /// </summary>
            /// <param name="lastBucket">The bucket to update with.</param>
            public void AddBucket(Bucket lastBucket)
            {
                foreach (HystrixRollingNumberEvent type in HystrixRollingNumberEventExtensions.Values)
                {
                    if (type.IsCounter())
                    {
                        this.GetAdder(type).Add(lastBucket.GetAdder(type).Sum());
                    }

                    if (type.IsMaxUpdater())
                    {
                        this.GetMaxUpdater(type).Update(lastBucket.GetMaxUpdater(type).Max());
                    }
                }
            }

            /// <summary>
            /// Gets the cumulative value for the specified <see cref="HystrixRollingNumberEvent"/>.
            /// (Returns <see cref="LongAdder.Sum()"/> for Counter types and <see cref="LongMaxUpdater.Max()"/> for MaxUpdater types.)
            /// </summary>
            /// <param name="type">The specified event.</param>
            /// <returns>The cumulative value for the specified event.</returns>
            public long Get(HystrixRollingNumberEvent type)
            {
                if (type.IsCounter())
                {
                    return this.adderForCounterType[(int)type].Sum();
                }
                else if (type.IsMaxUpdater())
                {
                    return this.updaterForCounterType[(int)type].Max();
                }
                else
                {
                    throw new ArgumentException(string.Format("Unknown type of event: {0}", type), "type");
                }
            }

            /// <summary>
            /// Gets the <see cref="LongAdder"/> instance for the specified event.
            /// </summary>
            /// <param name="type">The specified event.</param>
            /// <returns>The <see cref="LongAdder"/> instance for the specified event.</returns>
            public LongAdder GetAdder(HystrixRollingNumberEvent type)
            {
                if (!type.IsCounter())
                {
                    throw new ArgumentException(string.Format("Type is not a Counter: {0}", type), "type");
                }

                return this.adderForCounterType[(int)type];
            }

            /// <summary>
            /// Gets the <see cref="LongMaxUpdater"/> instance for the specified event.
            /// </summary>
            /// <param name="type">The specified event.</param>
            /// <returns>The <see cref="LongMaxUpdater"/> instance for the specified event.</returns>
            public LongMaxUpdater GetMaxUpdater(HystrixRollingNumberEvent type)
            {
                if (!type.IsMaxUpdater())
                {
                    throw new ArgumentException(string.Format("Type is not a MaxUpdater: {0}", type), "type");
                }

                return this.updaterForCounterType[(int)type];
            }
        }
    }
}
