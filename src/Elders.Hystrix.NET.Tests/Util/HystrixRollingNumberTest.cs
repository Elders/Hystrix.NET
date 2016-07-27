namespace Elders.Hystrix.NET.Test.Util
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Elders.Hystrix.NET.Util;

    [TestClass]
    public class HystrixRollingNumberTest
    {
        private TestContext testContextInstance;
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }


        [TestMethod]
        public void RollingNumber_CreatesBuckets()
        {
            MockedTime time = new MockedTime();
            try
            {
                HystrixRollingNumber counter = new HystrixRollingNumber(time, 200, 10);
                // confirm the initial settings
                Assert.AreEqual(200, counter.TimeInMilliseconds);
                Assert.AreEqual(10, counter.NumberOfBuckets);
                Assert.AreEqual(20, counter.BucketSizeInMilliseconds);

                // we start out with 0 buckets in the queue
                Assert.AreEqual(0, counter.Buckets.Size);

                // add a success in each interval which should result in all 10 buckets being created with 1 success in each
                for (int i = 0; i < counter.NumberOfBuckets; i++)
                {
                    counter.Increment(HystrixRollingNumberEvent.Success);
                    time.Increment(counter.BucketSizeInMilliseconds);
                }

                // confirm we have all 10 buckets
                Assert.AreEqual(10, counter.Buckets.Size);

                // add 1 more and we should still only have 10 buckets since that's the max
                counter.Increment(HystrixRollingNumberEvent.Success);
                Assert.AreEqual(10, counter.Buckets.Size);

            }
            catch (Exception e)
            {
                TestContext.WriteLine(e.ToString());
                Assert.Fail("Exception: " + e.Message);
            }
        }

        [TestMethod]
        public void RollingNumber_ResetBuckets()
        {
            MockedTime time = new MockedTime();
            try
            {
                HystrixRollingNumber counter = new HystrixRollingNumber(time, 200, 10);

                // we start out with 0 buckets in the queue
                Assert.AreEqual(0, counter.Buckets.Size);

                // add 1
                counter.Increment(HystrixRollingNumberEvent.Success);

                // confirm we have 1 bucket
                Assert.AreEqual(1, counter.Buckets.Size);

                // confirm we still have 1 bucket
                Assert.AreEqual(1, counter.Buckets.Size);

                // add 1
                counter.Increment(HystrixRollingNumberEvent.Success);

                // we should now have a single bucket with no values in it instead of 2 or more buckets
                Assert.AreEqual(1, counter.Buckets.Size);

            }
            catch (Exception e)
            {
                TestContext.WriteLine(e.ToString());
                Assert.Fail("Exception: " + e.Message);
            }
        }

        [TestMethod]
        public void RollingNumber_EmptyBucketsFillIn()
        {
            MockedTime time = new MockedTime();
            try
            {
                HystrixRollingNumber counter = new HystrixRollingNumber(time, 200, 10);

                // add 1
                counter.Increment(HystrixRollingNumberEvent.Success);

                // we should have 1 bucket
                Assert.AreEqual(1, counter.Buckets.Size);

                // wait past 3 bucket time periods (the 1st bucket then 2 empty ones)
                time.Increment(counter.BucketSizeInMilliseconds * 3);

                // add another
                counter.Increment(HystrixRollingNumberEvent.Success);

                // we should have 4 (1 + 2 empty + 1 new one) buckets
                Assert.AreEqual(4, counter.Buckets.Size);

            }
            catch (Exception e)
            {
                TestContext.WriteLine(e.ToString());
                Assert.Fail("Exception: " + e.Message);
            }
        }

        [TestMethod]
        public void RollingNumber_IncrementInSingleBucket()
        {
            MockedTime time = new MockedTime();
            try
            {
                HystrixRollingNumber counter = new HystrixRollingNumber(time, 200, 10);

                // increment
                counter.Increment(HystrixRollingNumberEvent.Success);
                counter.Increment(HystrixRollingNumberEvent.Success);
                counter.Increment(HystrixRollingNumberEvent.Success);
                counter.Increment(HystrixRollingNumberEvent.Success);
                counter.Increment(HystrixRollingNumberEvent.Failure);
                counter.Increment(HystrixRollingNumberEvent.Failure);
                counter.Increment(HystrixRollingNumberEvent.Timeout);

                // we should have 1 bucket
                Assert.AreEqual(1, counter.Buckets.Size);

                // the count should be 4
                Assert.AreEqual(4, counter.Buckets.GetLast().GetAdder(HystrixRollingNumberEvent.Success).Sum());
                Assert.AreEqual(2, counter.Buckets.GetLast().GetAdder(HystrixRollingNumberEvent.Failure).Sum());
                Assert.AreEqual(1, counter.Buckets.GetLast().GetAdder(HystrixRollingNumberEvent.Timeout).Sum());

            }
            catch (Exception e)
            {
                TestContext.WriteLine(e.ToString());
                Assert.Fail("Exception: " + e.Message);
            }
        }

        [TestMethod]
        public void RollingNumber_Timeout()
        {
            MockedTime time = new MockedTime();
            try
            {
                HystrixRollingNumber counter = new HystrixRollingNumber(time, 200, 10);

                // increment
                counter.Increment(HystrixRollingNumberEvent.Timeout);

                // we should have 1 bucket
                Assert.AreEqual(1, counter.Buckets.Size);

                // the count should be 1
                Assert.AreEqual(1, counter.Buckets.GetLast().GetAdder(HystrixRollingNumberEvent.Timeout).Sum());
                Assert.AreEqual(1, counter.GetRollingSum(HystrixRollingNumberEvent.Timeout));

                // sleep to get to a new bucket
                time.Increment(counter.BucketSizeInMilliseconds * 3);

                // incremenet again in latest bucket
                counter.Increment(HystrixRollingNumberEvent.Timeout);

                // we should have 4 buckets
                Assert.AreEqual(4, counter.Buckets.Size);

                // the counts of the last bucket
                Assert.AreEqual(1, counter.Buckets.GetLast().GetAdder(HystrixRollingNumberEvent.Timeout).Sum());

                // the total counts
                Assert.AreEqual(2, counter.GetRollingSum(HystrixRollingNumberEvent.Timeout));

            }
            catch (Exception e)
            {
                TestContext.WriteLine(e.ToString());
                Assert.Fail("Exception: " + e.Message);
            }
        }

        [TestMethod]
        public void RollingNumber_ShortCircuited()
        {
            MockedTime time = new MockedTime();
            try
            {
                HystrixRollingNumber counter = new HystrixRollingNumber(time, 200, 10);

                // increment
                counter.Increment(HystrixRollingNumberEvent.ShortCircuited);

                // we should have 1 bucket
                Assert.AreEqual(1, counter.Buckets.Size);

                // the count should be 1
                Assert.AreEqual(1, counter.Buckets.GetLast().GetAdder(HystrixRollingNumberEvent.ShortCircuited).Sum());
                Assert.AreEqual(1, counter.GetRollingSum(HystrixRollingNumberEvent.ShortCircuited));

                // sleep to get to a new bucket
                time.Increment(counter.BucketSizeInMilliseconds * 3);

                // incremenet again in latest bucket
                counter.Increment(HystrixRollingNumberEvent.ShortCircuited);

                // we should have 4 buckets
                Assert.AreEqual(4, counter.Buckets.Size);

                // the counts of the last bucket
                Assert.AreEqual(1, counter.Buckets.GetLast().GetAdder(HystrixRollingNumberEvent.ShortCircuited).Sum());

                // the total counts
                Assert.AreEqual(2, counter.GetRollingSum(HystrixRollingNumberEvent.ShortCircuited));

            }
            catch (Exception e)
            {
                TestContext.WriteLine(e.ToString());
                Assert.Fail("Exception: " + e.Message);
            }
        }

        [TestMethod]
        public void RollingNumber_ThreadPoolRejection()
        {
            TestCounterType(HystrixRollingNumberEvent.ThreadPoolRejected);
        }

        [TestMethod]
        public void RollingNumber_FallbackSuccess()
        {
            TestCounterType(HystrixRollingNumberEvent.FallbackSuccess);
        }

        [TestMethod]
        public void RollingNumber_FallbackFailure()
        {
            TestCounterType(HystrixRollingNumberEvent.FallbackFailure);
        }

        [TestMethod]
        public void RollingNumber_ExceptionThrow()
        {
            TestCounterType(HystrixRollingNumberEvent.ExceptionThrown);
        }

        private void TestCounterType(HystrixRollingNumberEvent type)
        {
            MockedTime time = new MockedTime();
            try
            {
                HystrixRollingNumber counter = new HystrixRollingNumber(time, 200, 10);

                // increment
                counter.Increment(type);

                // we should have 1 bucket
                Assert.AreEqual(1, counter.Buckets.Size);

                // the count should be 1
                Assert.AreEqual(1, counter.Buckets.GetLast().GetAdder(type).Sum());
                Assert.AreEqual(1, counter.GetRollingSum(type));

                // sleep to get to a new bucket
                time.Increment(counter.BucketSizeInMilliseconds * 3);

                // increment again in latest bucket
                counter.Increment(type);

                // we should have 4 buckets
                Assert.AreEqual(4, counter.Buckets.Size);

                // the counts of the last bucket
                Assert.AreEqual(1, counter.Buckets.GetLast().GetAdder(type).Sum());

                // the total counts
                Assert.AreEqual(2, counter.GetRollingSum(type));

            }
            catch (Exception e)
            {
                TestContext.WriteLine(e.ToString());
                Assert.Fail("Exception: " + e.Message);
            }
        }

        [TestMethod]
        public void RollingNumber_IncrementInMultipleBuckets()
        {
            MockedTime time = new MockedTime();
            try
            {
                HystrixRollingNumber counter = new HystrixRollingNumber(time, 200, 10);

                // increment
                counter.Increment(HystrixRollingNumberEvent.Success);
                counter.Increment(HystrixRollingNumberEvent.Success);
                counter.Increment(HystrixRollingNumberEvent.Success);
                counter.Increment(HystrixRollingNumberEvent.Success);
                counter.Increment(HystrixRollingNumberEvent.Failure);
                counter.Increment(HystrixRollingNumberEvent.Failure);
                counter.Increment(HystrixRollingNumberEvent.Timeout);
                counter.Increment(HystrixRollingNumberEvent.Timeout);
                counter.Increment(HystrixRollingNumberEvent.ShortCircuited);

                // sleep to get to a new bucket
                time.Increment(counter.BucketSizeInMilliseconds * 3);

                // increment
                counter.Increment(HystrixRollingNumberEvent.Success);
                counter.Increment(HystrixRollingNumberEvent.Success);
                counter.Increment(HystrixRollingNumberEvent.Failure);
                counter.Increment(HystrixRollingNumberEvent.Failure);
                counter.Increment(HystrixRollingNumberEvent.Failure);
                counter.Increment(HystrixRollingNumberEvent.Timeout);
                counter.Increment(HystrixRollingNumberEvent.ShortCircuited);

                // we should have 4 buckets
                Assert.AreEqual(4, counter.Buckets.Size);

                // the counts of the last bucket
                Assert.AreEqual(2, counter.Buckets.GetLast().GetAdder(HystrixRollingNumberEvent.Success).Sum());
                Assert.AreEqual(3, counter.Buckets.GetLast().GetAdder(HystrixRollingNumberEvent.Failure).Sum());
                Assert.AreEqual(1, counter.Buckets.GetLast().GetAdder(HystrixRollingNumberEvent.Timeout).Sum());
                Assert.AreEqual(1, counter.Buckets.GetLast().GetAdder(HystrixRollingNumberEvent.ShortCircuited).Sum());

                // the total counts
                Assert.AreEqual(6, counter.GetRollingSum(HystrixRollingNumberEvent.Success));
                Assert.AreEqual(5, counter.GetRollingSum(HystrixRollingNumberEvent.Failure));
                Assert.AreEqual(3, counter.GetRollingSum(HystrixRollingNumberEvent.Timeout));
                Assert.AreEqual(2, counter.GetRollingSum(HystrixRollingNumberEvent.ShortCircuited));

                // wait until window passes
                time.Increment(counter.TimeInMilliseconds);

                // increment
                counter.Increment(HystrixRollingNumberEvent.Success);

                // the total counts should now include only the last bucket after a reset since the window passed
                Assert.AreEqual(1, counter.GetRollingSum(HystrixRollingNumberEvent.Success));
                Assert.AreEqual(0, counter.GetRollingSum(HystrixRollingNumberEvent.Failure));
                Assert.AreEqual(0, counter.GetRollingSum(HystrixRollingNumberEvent.Timeout));

            }
            catch (Exception e)
            {
                TestContext.WriteLine(e.ToString());
                Assert.Fail("Exception: " + e.Message);
            }
        }

        [TestMethod]
        public void RollingNumber_CounterRetrievalRefreshesBuckets()
        {
            MockedTime time = new MockedTime();
            try
            {
                HystrixRollingNumber counter = new HystrixRollingNumber(time, 200, 10);

                // increment
                counter.Increment(HystrixRollingNumberEvent.Success);
                counter.Increment(HystrixRollingNumberEvent.Success);
                counter.Increment(HystrixRollingNumberEvent.Success);
                counter.Increment(HystrixRollingNumberEvent.Success);
                counter.Increment(HystrixRollingNumberEvent.Failure);
                counter.Increment(HystrixRollingNumberEvent.Failure);

                // sleep to get to a new bucket
                time.Increment(counter.BucketSizeInMilliseconds * 3);

                // we should have 1 bucket since nothing has triggered the update of buckets in the elapsed time
                Assert.AreEqual(1, counter.Buckets.Size);

                // the total counts
                Assert.AreEqual(4, counter.GetRollingSum(HystrixRollingNumberEvent.Success));
                Assert.AreEqual(2, counter.GetRollingSum(HystrixRollingNumberEvent.Failure));

                // we should have 4 buckets as the counter 'gets' should have triggered the buckets being created to fill in time
                Assert.AreEqual(4, counter.Buckets.Size);

                // wait until window passes
                time.Increment(counter.TimeInMilliseconds);

                // the total counts should all be 0 (and the buckets cleared by the get, not only increment)
                Assert.AreEqual(0, counter.GetRollingSum(HystrixRollingNumberEvent.Success));
                Assert.AreEqual(0, counter.GetRollingSum(HystrixRollingNumberEvent.Failure));

                // increment
                counter.Increment(HystrixRollingNumberEvent.Success);

                // the total counts should now include only the last bucket after a reset since the window passed
                Assert.AreEqual(1, counter.GetRollingSum(HystrixRollingNumberEvent.Success));
                Assert.AreEqual(0, counter.GetRollingSum(HystrixRollingNumberEvent.Failure));

            }
            catch (Exception e)
            {
                TestContext.WriteLine(e.ToString());
                Assert.Fail("Exception: " + e.Message);
            }
        }

        [TestMethod]
        public void RollingNumber_UpdateMax1() {
            MockedTime time = new MockedTime();
            try {
                HystrixRollingNumber counter = new HystrixRollingNumber(time, 200, 10);

                // increment
                counter.UpdateRollingMax(HystrixRollingNumberEvent.ThreadMaxActive, 10);

                // we should have 1 bucket
                Assert.AreEqual(1, counter.Buckets.Size);

                // the count should be 10
                Assert.AreEqual(10, counter.Buckets.GetLast().GetMaxUpdater(HystrixRollingNumberEvent.ThreadMaxActive).Max());
                Assert.AreEqual(10, counter.GetRollingMaxValue(HystrixRollingNumberEvent.ThreadMaxActive));

                // sleep to get to a new bucket
                time.Increment(counter.BucketSizeInMilliseconds * 3);

                // increment again in latest bucket
                counter.UpdateRollingMax(HystrixRollingNumberEvent.ThreadMaxActive, 20);

                // we should have 4 buckets
                Assert.AreEqual(4, counter.Buckets.Size);

                // the max
                Assert.AreEqual(20, counter.Buckets.GetLast().GetMaxUpdater(HystrixRollingNumberEvent.ThreadMaxActive).Max());

                // counts per bucket
                long[] values = counter.GetValues(HystrixRollingNumberEvent.ThreadMaxActive);
                Assert.AreEqual(10, values[0]); // oldest bucket
                Assert.AreEqual(0, values[1]);
                Assert.AreEqual(0, values[2]);
                Assert.AreEqual(20, values[3]); // latest bucket

            } catch (Exception e) {
                TestContext.WriteLine(e.ToString());
                Assert.Fail("Exception: " + e.Message);
            }
        }

        [TestMethod]
        public void RollingNumber_UpdateMax2()
        {
            MockedTime time = new MockedTime();
            try
            {
                HystrixRollingNumber counter = new HystrixRollingNumber(time, 200, 10);

                // increment
                counter.UpdateRollingMax(HystrixRollingNumberEvent.ThreadMaxActive, 10);
                counter.UpdateRollingMax(HystrixRollingNumberEvent.ThreadMaxActive, 30);
                counter.UpdateRollingMax(HystrixRollingNumberEvent.ThreadMaxActive, 20);

                // we should have 1 bucket
                Assert.AreEqual(1, counter.Buckets.Size);

                // the count should be 30
                Assert.AreEqual(30, counter.Buckets.GetLast().GetMaxUpdater(HystrixRollingNumberEvent.ThreadMaxActive).Max());
                Assert.AreEqual(30, counter.GetRollingMaxValue(HystrixRollingNumberEvent.ThreadMaxActive));

                // sleep to get to a new bucket
                time.Increment(counter.BucketSizeInMilliseconds * 3);

                counter.UpdateRollingMax(HystrixRollingNumberEvent.ThreadMaxActive, 30);
                counter.UpdateRollingMax(HystrixRollingNumberEvent.ThreadMaxActive, 30);
                counter.UpdateRollingMax(HystrixRollingNumberEvent.ThreadMaxActive, 50);

                // we should have 4 buckets
                Assert.AreEqual(4, counter.Buckets.Size);

                // the count
                Assert.AreEqual(50, counter.Buckets.GetLast().GetMaxUpdater(HystrixRollingNumberEvent.ThreadMaxActive).Max());
                Assert.AreEqual(50, counter.GetValueOfLatestBucket(HystrixRollingNumberEvent.ThreadMaxActive));

                // values per bucket
                long[] values = counter.GetValues(HystrixRollingNumberEvent.ThreadMaxActive);
                Assert.AreEqual(30, values[0]); // oldest bucket
                Assert.AreEqual(0, values[1]);
                Assert.AreEqual(0, values[2]);
                Assert.AreEqual(50, values[3]); // latest bucket

            }
            catch (Exception e)
            {
                TestContext.WriteLine(e.ToString());
                Assert.Fail("Exception: " + e.Message);
            }
        }

        [TestMethod]
        public void RollingNumber_MaxValue()
        {
            MockedTime time = new MockedTime();
            try
            {
                HystrixRollingNumberEvent type = HystrixRollingNumberEvent.ThreadMaxActive;

                HystrixRollingNumber counter = new HystrixRollingNumber(time, 200, 10);

                counter.UpdateRollingMax(type, 10);

                // sleep to get to a new bucket
                time.Increment(counter.BucketSizeInMilliseconds);

                counter.UpdateRollingMax(type, 30);

                // sleep to get to a new bucket
                time.Increment(counter.BucketSizeInMilliseconds);

                counter.UpdateRollingMax(type, 40);

                // sleep to get to a new bucket
                time.Increment(counter.BucketSizeInMilliseconds);

                counter.UpdateRollingMax(type, 15);

                Assert.AreEqual(40, counter.GetRollingMaxValue(type));

            }
            catch (Exception e)
            {
                TestContext.WriteLine(e.ToString());
                Assert.Fail("Exception: " + e.Message);
            }
        }

        [TestMethod]
        public void RollingNumber_EmptySum()
        {
            MockedTime time = new MockedTime();
            HystrixRollingNumberEvent type = HystrixRollingNumberEvent.Collapsed;
            HystrixRollingNumber counter = new HystrixRollingNumber(time, 200, 10);
            Assert.AreEqual(0, counter.GetRollingSum(type));
        }

        [TestMethod]
        public void RollingNumber_EmptyMax()
        {
            MockedTime time = new MockedTime();
            HystrixRollingNumberEvent type = HystrixRollingNumberEvent.ThreadMaxActive;
            HystrixRollingNumber counter = new HystrixRollingNumber(time, 200, 10);
            Assert.AreEqual(0, counter.GetRollingMaxValue(type));
        }

        [TestMethod]
        public void RollingNumber_EmptyLatestValue()
        {
            MockedTime time = new MockedTime();
            HystrixRollingNumberEvent type = HystrixRollingNumberEvent.ThreadMaxActive;
            HystrixRollingNumber counter = new HystrixRollingNumber(time, 200, 10);
            Assert.AreEqual(0, counter.GetValueOfLatestBucket(type));
        }

        [TestMethod]
        public void RollingNumber_Rolling()
        {
            MockedTime time = new MockedTime();
            HystrixRollingNumberEvent type = HystrixRollingNumberEvent.ThreadMaxActive;
            HystrixRollingNumber counter = new HystrixRollingNumber(time, 20, 2);
            // iterate over 20 buckets on a queue sized for 2
            for (int i = 0; i < 20; i++)
            {
                // first bucket
                counter.GetCurrentBucket();
                try
                {
                    time.Increment(counter.BucketSizeInMilliseconds);
                }
                catch (Exception)
                {
                    // ignore
                }

                Assert.AreEqual(2, counter.GetValues(type).Length);

                counter.GetValueOfLatestBucket(type);

                // System.out.println("Head: " + counter.Buckets.state.Get().head);
                // System.out.println("Tail: " + counter.Buckets.state.Get().tail);
            }
        }

        [TestMethod]
        public void RollingNumber_CumulativeCounterAfterRolling()
        {
            MockedTime time = new MockedTime();
            HystrixRollingNumberEvent type = HystrixRollingNumberEvent.Success;
            HystrixRollingNumber counter = new HystrixRollingNumber(time, 20, 2);

            Assert.AreEqual(0, counter.GetCumulativeSum(type));

            // iterate over 20 buckets on a queue sized for 2
            for (int i = 0; i < 20; i++)
            {
                // first bucket
                counter.Increment(type);
                try
                {
                    time.Increment(counter.BucketSizeInMilliseconds);
                }
                catch (Exception)
                {
                    // ignore
                }

                Assert.AreEqual(2, counter.GetValues(type).Length);

                counter.GetValueOfLatestBucket(type);

            }

            // cumulative count should be 20 (for the number of loops above) regardless of buckets rolling
            Assert.AreEqual(20, counter.GetCumulativeSum(type));
        }

        [TestMethod]
        public void RollingNumber_CumulativeCounterAfterRollingAndReset()
        {
            MockedTime time = new MockedTime();
            HystrixRollingNumberEvent type = HystrixRollingNumberEvent.Success;
            HystrixRollingNumber counter = new HystrixRollingNumber(time, 20, 2);

            Assert.AreEqual(0, counter.GetCumulativeSum(type));

            // iterate over 20 buckets on a queue sized for 2
            for (int i = 0; i < 20; i++)
            {
                // first bucket
                counter.Increment(type);
                try
                {
                    time.Increment(counter.BucketSizeInMilliseconds);
                }
                catch (Exception)
                {
                    // ignore
                }

                Assert.AreEqual(2, counter.GetValues(type).Length);

                counter.GetValueOfLatestBucket(type);

                if (i == 5 || i == 15)
                {
                    // simulate a reset occurring every once in a while
                    // so we ensure the absolute sum is handling it okay
                    counter.Reset();
                }
            }

            // cumulative count should be 20 (for the number of loops above) regardless of buckets rolling
            Assert.AreEqual(20, counter.GetCumulativeSum(type));
        }

        [TestMethod]
        public void RollingNumber_CumulativeCounterAfterRollingAndReset2()
        {
            MockedTime time = new MockedTime();
            HystrixRollingNumberEvent type = HystrixRollingNumberEvent.Success;
            HystrixRollingNumber counter = new HystrixRollingNumber(time, 20, 2);

            Assert.AreEqual(0, counter.GetCumulativeSum(type));

            counter.Increment(type);
            counter.Increment(type);
            counter.Increment(type);

            // iterate over 20 buckets on a queue sized for 2
            for (int i = 0; i < 20; i++)
            {
                try
                {
                    time.Increment(counter.BucketSizeInMilliseconds);
                }
                catch (Exception)
                {
                    // ignore
                }

                if (i == 5 || i == 15)
                {
                    // simulate a reset occurring every once in a while
                    // so we ensure the absolute sum is handling it okay
                    counter.Reset();
                }
            }

            // no increments during the loop, just some before and after
            counter.Increment(type);
            counter.Increment(type);

            // cumulative count should be 5 regardless of buckets rolling
            Assert.AreEqual(5, counter.GetCumulativeSum(type));
        }

        [TestMethod]
        public void RollingNumber_CumulativeCounterAfterRollingAndReset3()
        {
            MockedTime time = new MockedTime();
            HystrixRollingNumberEvent type = HystrixRollingNumberEvent.Success;
            HystrixRollingNumber counter = new HystrixRollingNumber(time, 20, 2);

            Assert.AreEqual(0, counter.GetCumulativeSum(type));

            counter.Increment(type);
            counter.Increment(type);
            counter.Increment(type);

            // iterate over 20 buckets on a queue sized for 2
            for (int i = 0; i < 20; i++)
            {
                try
                {
                    time.Increment(counter.BucketSizeInMilliseconds);
                }
                catch (Exception)
                {
                    // ignore
                }
            }

            // since we are rolling over the buckets it should reset naturally

            // no increments during the loop, just some before and after
            counter.Increment(type);
            counter.Increment(type);

            // cumulative count should be 5 regardless of buckets rolling
            Assert.AreEqual(5, counter.GetCumulativeSum(type));
        }
    }
}
