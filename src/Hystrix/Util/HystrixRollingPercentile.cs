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
    using System.Diagnostics;
    using System.Threading;
    using Java.Util.Concurrent.Atomic;
    using slf4net;

    /// <summary>
    /// <para>
    /// Add values to a rolling window and retrieve percentile calculations such as median, 90th, 99th, etc.
    /// </para>
    /// <para>
    /// The underlying data structure contains a circular array of buckets that "roll" over time.
    /// </para>
    /// <para>
    /// For example, if the time window is configured to 60 seconds with 12 buckets of 5 seconds each, values will be captured in each 5 second bucket and rotate each 5 seconds.
    /// </para>
    /// <para>
    /// This means that percentile calculations are for the "rolling window" of 55-60 seconds up to 5 seconds ago.
    /// </para>
    /// <para>
    /// Each bucket will contain a circular array of long values and if more than the configured amount (1000 values for example) it will wrap around and overwrite values until time passes and a new bucket
    /// is allocated. This sampling approach for high volume metrics is done to conserve memory and reduce sorting time when calculating percentiles.
    /// </para>
    /// </summary>
    public class HystrixRollingPercentile
    {
        /// <summary>
        /// The logger instance to log the events of this object.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(HystrixRollingPercentile));

        /// <summary>
        /// The object used to synchronize the <see cref="GetCurrentBucket"/> method.
        /// </summary>
        private readonly object newBucketLock = new object();

        /// <summary>
        /// The <see cref="ITime"/> instance to measure time.
        /// </summary>
        private readonly ITime time;

        /// <summary>
        /// Number of milliseconds of data that should be tracked.
        /// </summary>
        private readonly IHystrixProperty<int> timeInMilliseconds;

        /// <summary>
        /// Number of buckets that the time window should be divided into.
        /// </summary>
        private readonly IHystrixProperty<int> numberOfBuckets;

        /// <summary>
        /// Number of values stored in each bucket.
        /// </summary>
        private readonly IHystrixProperty<int> bucketDataLength;

        /// <summary>
        /// Sets whether data should be tracked and percentiles be calculated.
        /// </summary>
        private readonly IHystrixProperty<bool> enabled;

        /// <summary>
        /// The array to store the values of the specified time window.
        /// </summary>
        private readonly BucketCircularArray<Bucket> buckets;

        /// <summary>
        /// The current snapshot of the percentile data.
        /// </summary>
        private volatile PercentileSnapshot currentPercentileSnapshot = new PercentileSnapshot(0);

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixRollingPercentile"/> class.
        /// </summary>
        /// <param name="timeInMilliseconds">Number of milliseconds of data that should be tracked.</param>
        /// <param name="numberOfBuckets">Number of buckets that the time window should be divided into.</param>
        /// <param name="bucketDataLength">Number of values stored in each bucket.</param>
        /// <param name="enabled">Sets whether data should be tracked and percentiles be calculated.</param>
        public HystrixRollingPercentile(IHystrixProperty<int> timeInMilliseconds, IHystrixProperty<int> numberOfBuckets, IHystrixProperty<int> bucketDataLength, IHystrixProperty<bool> enabled)
            : this(ActualTime.Instance, timeInMilliseconds, numberOfBuckets, bucketDataLength, enabled)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixRollingPercentile"/> class.
        /// </summary>
        /// <param name="time">The <see cref="ITime"/> instance to measure time.</param>
        /// <param name="timeInMilliseconds">Number of milliseconds of data that should be tracked.</param>
        /// <param name="numberOfBuckets">Number of buckets that the time window should be divided into.</param>
        /// <param name="bucketDataLength">Number of values stored in each bucket.</param>
        /// <param name="enabled">Sets whether data should be tracked and percentiles be calculated.</param>
        internal HystrixRollingPercentile(ITime time, IHystrixProperty<int> timeInMilliseconds, IHystrixProperty<int> numberOfBuckets, IHystrixProperty<int> bucketDataLength, IHystrixProperty<bool> enabled)
        {
            if (timeInMilliseconds.Get() % numberOfBuckets.Get() != 0)
            {
                throw new ArgumentException("The timeInMilliseconds must divide equally into numberOfBuckets. For example 1000/10 is ok, 1000/11 is not.");
            }

            this.time = time;
            this.timeInMilliseconds = timeInMilliseconds;
            this.numberOfBuckets = numberOfBuckets;
            this.bucketDataLength = bucketDataLength;
            this.enabled = enabled;
            this.buckets = new BucketCircularArray<Bucket>(this.numberOfBuckets.Get());
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
        /// Gets the current snapshot.
        /// It will NOT include data from the current bucket, but all previous buckets.
        /// It remains cached until the next bucket rotates at which point a new one will be created.
        /// </summary>
        internal PercentileSnapshot CurrentPercentileSnapshot
        {
            get { return this.currentPercentileSnapshot; }
        }

        /// <summary>
        /// Gets the internal circular array to store the buckets. This property is intended to use only in unit tests.
        /// </summary>
        internal BucketCircularArray<Bucket> Buckets
        {
            get { return this.buckets; }
        }

        /// <summary>
        /// Add value (or values) to current bucket.
        /// </summary>
        /// <param name="value">Value to be stored in current bucket, such as execution latency.</param>
        public void AddValue(params int[] value)
        {
            // No-op if disabled
            if (!this.enabled.Get())
            {
                return;
            }

            foreach (int v in value)
            {
                try
                {
                    this.GetCurrentBucket().Data.AddValue(v);
                }
                catch (Exception e)
                {
                    Logger.Error("Failed to add value: " + v, e);
                }
            }
        }

        /// <summary>
        /// <para>
        /// Compute a percentile from the underlying rolling buckets of values.
        /// </para>
        /// <para>
        /// For performance reasons it maintains a single snapshot of the sorted values from all buckets that is re-generated each time the bucket rotates.
        /// </para>
        /// <para>
        /// This means that if a bucket is 5000 milliseconds, then this method will re-compute a percentile at most once every 5000 milliseconds.
        /// </para>
        /// </summary>
        /// <param name="percentile">Value such as 99 (99th percentile), 99.5 (99.5th percentile), 50 (median, 50th percentile) to compute and retrieve percentile from rolling buckets.</param>
        /// <returns>Percentile value</returns>
        public int GetPercentile(double percentile)
        {
            // No-op if disabled
            if (!this.enabled.Get())
            {
                return -1;
            }

            // Force logic to move buckets forward in case other requests aren't making it happen.
            this.GetCurrentBucket();

            // Fetch the current snapshot
            return this.CurrentPercentileSnapshot.GetPercentile(percentile);
        }

        /// <summary>
        /// Gets the mean (average) of all values in the current snapshot. This is not a percentile but often desired so captured and exposed here.
        /// </summary>
        /// <returns>Mean of all values.</returns>
        public int GetMean()
        {
            // No-op if disabled
            if (!this.enabled.Get())
            {
                return -1;
            }

            // Force logic to move buckets forward in case other requests aren't making it happen
            this.GetCurrentBucket();

            // Fetch the current snapshot
            return this.CurrentPercentileSnapshot.Mean;
        }

        /// <summary>
        /// Force a reset so that percentiles start being gathered from scratch.
        /// </summary>
        public void Reset()
        {
            // No-op if disabled
            if (!this.enabled.Get())
            {
                return;
            }

            // Clear buckets so we start over again
            this.buckets.Clear();
        }

        /// <summary>
        /// Gets the current bucket. If the time is after the window of the current bucket, a new one will be created.
        /// Internal because it's used in unit tests.
        /// </summary>
        /// <returns>The current bucket.</returns>
        private Bucket GetCurrentBucket()
        {
            long currentTime = this.time.GetCurrentTimeInMillis();

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
                        Bucket newBucket = new Bucket(currentTime, this.bucketDataLength.Get());
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
                                Bucket[] allBuckets = this.buckets.GetArray();

                                // Create a new bucket and add it as the new 'last' (once this is done other threads will start using it on subsequent retrievals)
                                this.buckets.AddLast(new Bucket(lastBucket.WindowStart + this.BucketSizeInMilliseconds, this.bucketDataLength.Get()));

                                // Add the lastBucket values to the cumulativeSum
                                this.currentPercentileSnapshot = new PercentileSnapshot(allBuckets);
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
        /// Stores all the values of the given buckets and provides methods to calculate percentiles.
        /// </summary>
        internal class PercentileSnapshot
        {
            /// <summary>
            /// The stored values.
            /// </summary>
            private readonly int[] data;

            /// <summary>
            /// The number of values stored.
            /// </summary>
            private readonly int length;

            /// <summary>
            /// Initializes a new instance of the <see cref="PercentileSnapshot"/> class from the given buckets.
            /// </summary>
            /// <param name="buckets">The buckets to initialize the snapshot.</param>
            public PercentileSnapshot(Bucket[] buckets)
            {
                int lengthFromBuckets = 0;

                // We need to calculate it dynamically as it could have been changed by properties (rare, but possible)
                // Also this way we capture the actual index size rather than the max, so size the int[] to only what we need
                foreach (Bucket bd in buckets)
                {
                    lengthFromBuckets += bd.Data.Length;
                }

                this.data = new int[lengthFromBuckets];
                int index = 0;
                int sum = 0;
                foreach (Bucket bd in buckets)
                {
                    PercentileBucketData pbd = bd.Data;
                    int length = pbd.Length;
                    for (int i = 0; i < length; i++)
                    {
                        int v = pbd.List[i];
                        this.data[index++] = v;
                        sum += v;
                    }
                }

                this.length = index;
                if (this.length == 0)
                {
                    this.Mean = 0;
                }
                else
                {
                    this.Mean = sum / this.length;
                }

                Array.Sort(this.data, 0, this.length);
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="PercentileSnapshot"/> class using specified values.
            /// </summary>
            /// <param name="data">The values to store.</param>
            public PercentileSnapshot(params int[] data)
            {
                this.data = data;
                this.length = data.Length;

                int sum = 0;
                foreach (int v in data)
                {
                    sum += v;
                }

                this.Mean = sum / this.length;
                Array.Sort(this.data, 0, this.length);
            }

            /// <summary>
            /// Gets the mean of the stored values.
            /// </summary>
            public int Mean { get; private set; }

            /// <summary>
            /// Calculates percentile.
            /// </summary>
            /// <param name="percentile">The percentile to calculate.</param>
            /// <returns>The computed percentile.</returns>
            public int GetPercentile(double percentile)
            {
                if (this.length == 0)
                {
                    return 0;
                }

                return this.ComputePercentile(percentile);
            }

            /// <summary>
            /// Do the actual calculation.
            /// </summary>
            /// <remarks>
            /// http://en.wikipedia.org/wiki/Percentile
            /// http://cnx.org/content/m10805/latest/
            /// </remarks>
            /// <param name="percent">The percentile to calculate.</param>
            /// <returns>The calculated percentile.</returns>
            private int ComputePercentile(double percent)
            {
                // Some just-in-case edge cases
                if (this.length <= 0)
                {
                    return 0;
                }
                else if (percent <= 0.0)
                {
                    return this.data[0];
                }
                else if (percent >= 100.0)
                {
                    return this.data[this.length - 1];
                }

                // ranking (http://en.wikipedia.org/wiki/Percentile#Alternative_methods)
                double rank = (percent / 100.0) * this.length;

                // linear interpolation between closest ranks
                int lowIndex = (int)Math.Floor(rank);
                int highIndex = (int)Math.Ceiling(rank);
                Debug.Assert(0 <= lowIndex && lowIndex <= rank && rank <= highIndex && highIndex <= this.length, "Invalid indexes for the closest ranks.");
                Debug.Assert((highIndex - lowIndex) <= 1, "Invalid indexes for the closest ranks.");

                if (highIndex >= this.length)
                {
                    // Another edge case
                    return this.data[this.length - 1];
                }
                else if (lowIndex == highIndex)
                {
                    return this.data[lowIndex];
                }
                else
                {
                    // Interpolate between the two bounding values
                    return (int)(this.data[lowIndex] + ((rank - lowIndex) * (this.data[highIndex] - this.data[lowIndex])));
                }
            }
        }

        /// <summary>
        /// Stores the values of a bucket in <see cref="HystrixRollingPercentile"/>. It's behavior is similar to a circular array,
        /// the new items after the specified count will overwrite the oldest items.
        /// </summary>
        internal class PercentileBucketData
        {
            /// <summary>
            /// The maximum number of values to store.
            /// </summary>
            private readonly int length;

            /// <summary>
            /// The array of stored values.
            /// </summary>
            private readonly AtomicIntegerArray list;

            /// <summary>
            /// The index of the next item.
            /// </summary>
            private readonly AtomicInteger index = new AtomicInteger();

            /// <summary>
            /// Initializes a new instance of the <see cref="PercentileBucketData"/> class.
            /// </summary>
            /// <param name="dataLength">The maximum number of values to store.</param>
            public PercentileBucketData(int dataLength)
            {
                this.length = dataLength;
                this.list = new AtomicIntegerArray(dataLength);
            }

            /// <summary>
            /// Gets the internal array of stored items.
            /// </summary>
            public AtomicIntegerArray List
            {
                get { return this.list; }
            }

            /// <summary>
            /// Gets the number of stored items.
            /// </summary>
            public int Length
            {
                get { return Math.Min(this.index.Value, this.list.Length); }
            }

            /// <summary>
            /// Adds values to this bucket.
            /// </summary>
            /// <param name="latency">The values to add.</param>
            public void AddValue(params int[] latency)
            {
                foreach (int l in latency)
                {
                    // We just wrap around the beginning and over-write if we go past 'dataLength' as that will effectively cause us to "sample" the most recent data.
                    this.list[(this.index.IncrementAndGet() - 1) % this.length] = l;

                    // TODO Alternative to AtomicInteger? The getAndIncrement may be a source of contention on high throughput circuits on large multi-core systems.
                    // LongAdder isn't suited to this as it is not consistent. Perhaps a different data structure that doesn't need indexed adds?
                    // A threadlocal data storage that only aggregates when fetched would be ideal. Similar to LongAdder except for accumulating lists of data.
                }
            }
        }

        /// <summary>
        /// Counters for a given 'bucket' of time.
        /// </summary>
        internal class Bucket
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Bucket"/> class.
            /// </summary>
            /// <param name="startTime">The start time of this bucket.</param>
            /// <param name="bucketDataLength">The maximum number of values to store in this bucket.</param>
            public Bucket(long startTime, int bucketDataLength)
            {
                this.WindowStart = startTime;
                this.Data = new PercentileBucketData(bucketDataLength);
            }

            /// <summary>
            /// Gets the start time of this bucket.
            /// </summary>
            public long WindowStart { get; private set; }

            /// <summary>
            /// Gets the object storing the data of the bucket.
            /// </summary>
            public PercentileBucketData Data { get; private set; }
        }
    }
}
