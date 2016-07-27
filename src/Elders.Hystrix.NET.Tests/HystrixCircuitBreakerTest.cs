namespace Elders.Hystrix.NET.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Elders.Hystrix.NET;
    using Elders.Hystrix.NET.CircuitBreaker;
    using Elders.Hystrix.NET.Strategy.EventNotifier;

    [TestClass]
    public class HystrixCircuitBreakerTest
    {
        private HystrixCommandKey key = CommandKeyForUnitTest.KeyOne;


        /**
         * Test that if all 'marks' are successes during the test window that it does NOT trip the circuit.
         * Test that if all 'marks' are failures during the test window that it trips the circuit.
         */
        [TestMethod]
        public void CircuitBreaker_TripCircuit()
        {
            try
            {
                HystrixCommandPropertiesSetter properties = UnitTestSetterFactory.GetCommandPropertiesSetter();
                HystrixCommandMetrics metrics = GetMetrics(properties);
                IHystrixCircuitBreaker cb = GetCircuitBreaker(key, CommandGroupForUnitTest.OwnerTwo, metrics, properties);

                metrics.MarkSuccess(1000);
                metrics.MarkSuccess(1000);
                metrics.MarkSuccess(1000);
                metrics.MarkSuccess(1000);

                // this should still allow requests as everything has been successful
                Assert.IsTrue(cb.AllowRequest());
                Assert.IsFalse(cb.IsOpen());

                // fail
                metrics.MarkFailure(1000);
                metrics.MarkFailure(1000);
                metrics.MarkFailure(1000);
                metrics.MarkFailure(1000);

                // everything has failed in the test window so we should return false now
                Assert.IsFalse(cb.AllowRequest());
                Assert.IsTrue(cb.IsOpen());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Assert.Fail("Error occurred: " + e.Message);
            }
        }

        /**
         * Test that if the % of failures is higher than the threshold that the circuit trips.
         */
        [TestMethod]
        public void CircuitBreaker_TripCircuitOnFailuresAboveThreshold()
        {
            try
            {
                HystrixCommandPropertiesSetter properties = UnitTestSetterFactory.GetCommandPropertiesSetter();
                HystrixCommandMetrics metrics = GetMetrics(properties);
                IHystrixCircuitBreaker cb = GetCircuitBreaker(key, CommandGroupForUnitTest.OwnerTwo, metrics, properties);

                // this should start as allowing requests
                Assert.IsTrue(cb.AllowRequest());
                Assert.IsFalse(cb.IsOpen());

                // success with high latency
                metrics.MarkSuccess(400);
                metrics.MarkSuccess(400);
                metrics.MarkFailure(10);
                metrics.MarkSuccess(400);
                metrics.MarkFailure(10);
                metrics.MarkFailure(10);
                metrics.MarkSuccess(400);
                metrics.MarkFailure(10);
                metrics.MarkFailure(10);

                // this should trip the circuit as the error percentage is above the threshold
                Assert.IsFalse(cb.AllowRequest());
                Assert.IsTrue(cb.IsOpen());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Assert.Fail("Error occurred: " + e.Message);
            }
        }

        /**
         * Test that if the % of failures is higher than the threshold that the circuit trips.
         */
        [TestMethod]
        public void CircuitBreaker_CircuitDoesNotTripOnFailuresBelowThreshold()
        {
            try
            {
                HystrixCommandPropertiesSetter properties = UnitTestSetterFactory.GetCommandPropertiesSetter();
                HystrixCommandMetrics metrics = GetMetrics(properties);
                IHystrixCircuitBreaker cb = GetCircuitBreaker(key, CommandGroupForUnitTest.OwnerTwo, metrics, properties);

                // this should start as allowing requests
                Assert.IsTrue(cb.AllowRequest());
                Assert.IsFalse(cb.IsOpen());

                // success with high latency
                metrics.MarkSuccess(400);
                metrics.MarkSuccess(400);
                metrics.MarkFailure(10);
                metrics.MarkSuccess(400);
                metrics.MarkSuccess(40);
                metrics.MarkSuccess(400);
                metrics.MarkFailure(10);
                metrics.MarkFailure(10);

                // this should remain open as the failure threshold is below the percentage limit
                Assert.IsTrue(cb.AllowRequest());
                Assert.IsFalse(cb.IsOpen());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Assert.Fail("Error occurred: " + e.Message);
            }
        }

        /**
         * Test that if all 'marks' are timeouts that it will trip the circuit.
         */
        [TestMethod]
        public void CircuitBreaker_TripCircuitOnTimeouts()
        {
            try
            {
                HystrixCommandPropertiesSetter properties = UnitTestSetterFactory.GetCommandPropertiesSetter();
                HystrixCommandMetrics metrics = GetMetrics(properties);
                IHystrixCircuitBreaker cb = GetCircuitBreaker(key, CommandGroupForUnitTest.OwnerTwo, metrics, properties);

                // this should start as allowing requests
                Assert.IsTrue(cb.AllowRequest());
                Assert.IsFalse(cb.IsOpen());

                // timeouts
                metrics.MarkTimeout(2000);
                metrics.MarkTimeout(2000);
                metrics.MarkTimeout(2000);
                metrics.MarkTimeout(2000);

                // everything has been a timeout so we should not allow any requests
                Assert.IsFalse(cb.AllowRequest());
                Assert.IsTrue(cb.IsOpen());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Assert.Fail("Error occurred: " + e.Message);
            }
        }

        /**
         * Test that if the % of timeouts is higher than the threshold that the circuit trips.
         */
        [TestMethod]
        public void CircuitBreaker_TripCircuitOnTimeoutsAboveThreshold()
        {
            try
            {
                HystrixCommandPropertiesSetter properties = UnitTestSetterFactory.GetCommandPropertiesSetter();
                HystrixCommandMetrics metrics = GetMetrics(properties);
                IHystrixCircuitBreaker cb = GetCircuitBreaker(key, CommandGroupForUnitTest.OwnerTwo, metrics, properties);

                // this should start as allowing requests
                Assert.IsTrue(cb.AllowRequest());
                Assert.IsFalse(cb.IsOpen());

                // success with high latency
                metrics.MarkSuccess(400);
                metrics.MarkSuccess(400);
                metrics.MarkTimeout(10);
                metrics.MarkSuccess(400);
                metrics.MarkTimeout(10);
                metrics.MarkTimeout(10);
                metrics.MarkSuccess(400);
                metrics.MarkTimeout(10);
                metrics.MarkTimeout(10);

                // this should trip the circuit as the error percentage is above the threshold
                Assert.IsFalse(cb.AllowRequest());
                Assert.IsTrue(cb.IsOpen());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Assert.Fail("Error occurred: " + e.Message);
            }
        }

        /**
         * Test that on an open circuit that a single attempt will be allowed after a window of time to see if issues are resolved.
         */
        [TestMethod]
        public void CircuitBreaker_SingleTestOnOpenCircuitAfterTimeWindow()
        {
            try
            {
                int sleepWindow = 200;
                HystrixCommandPropertiesSetter properties = UnitTestSetterFactory.GetCommandPropertiesSetter().WithCircuitBreakerSleepWindowInMilliseconds(sleepWindow);
                HystrixCommandMetrics metrics = GetMetrics(properties);
                IHystrixCircuitBreaker cb = GetCircuitBreaker(key, CommandGroupForUnitTest.OwnerTwo, metrics, properties);

                // fail
                metrics.MarkFailure(1000);
                metrics.MarkFailure(1000);
                metrics.MarkFailure(1000);
                metrics.MarkFailure(1000);

                // everything has failed in the test window so we should return false now
                Assert.IsFalse(cb.AllowRequest());
                Assert.IsTrue(cb.IsOpen());

                // wait for sleepWindow to pass
                Thread.Sleep(sleepWindow + 50);

                // we should now allow 1 request
                Assert.IsTrue(cb.AllowRequest());
                // but the circuit should still be open
                Assert.IsTrue(cb.IsOpen());
                // and further requests are still blocked
                Assert.IsFalse(cb.AllowRequest());

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Assert.Fail("Error occurred: " + e.Message);
            }
        }

        /**
         * Test that an open circuit is closed after 1 success.
         */
        [TestMethod]
        public void CircuitBreaker_CircuitClosedAfterSuccess()
        {
            try
            {
                int sleepWindow = 200;
                HystrixCommandPropertiesSetter properties = UnitTestSetterFactory.GetCommandPropertiesSetter().WithCircuitBreakerSleepWindowInMilliseconds(sleepWindow);
                HystrixCommandMetrics metrics = GetMetrics(properties);
                IHystrixCircuitBreaker cb = GetCircuitBreaker(key, CommandGroupForUnitTest.OwnerTwo, metrics, properties);

                // fail
                metrics.MarkFailure(1000);
                metrics.MarkFailure(1000);
                metrics.MarkFailure(1000);
                metrics.MarkTimeout(1000);

                // everything has failed in the test window so we should return false now
                Assert.IsFalse(cb.AllowRequest());
                Assert.IsTrue(cb.IsOpen());

                // wait for sleepWindow to pass
                Thread.Sleep(sleepWindow + 50);

                // we should now allow 1 request
                Assert.IsTrue(cb.AllowRequest());
                // but the circuit should still be open
                Assert.IsTrue(cb.IsOpen());
                // and further requests are still blocked
                Assert.IsFalse(cb.AllowRequest());

                // the 'singleTest' succeeds so should cause the circuit to be closed
                metrics.MarkSuccess(500);
                cb.MarkSuccess();

                // all requests should be open again
                Assert.IsTrue(cb.AllowRequest());
                Assert.IsTrue(cb.AllowRequest());
                Assert.IsTrue(cb.AllowRequest());
                // and the circuit should be closed again
                Assert.IsFalse(cb.IsOpen());

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Assert.Fail("Error occurred: " + e.Message);
            }
        }

        /**
         * Test that an open circuit is closed after 1 success... when the sleepWindow is smaller than the statisticalWindow and 'failure' stats are still sticking around.
         * <p>
         * This means that the statistical window needs to be cleared otherwise it will still calculate the failure percentage below the threshold and immediately open the circuit again.
         */
        [TestMethod]
        public void CircuitBreaker_CircuitClosedAfterSuccessAndClearsStatisticalWindow()
        {
            try
            {
                int statisticalWindow = 200;
                int sleepWindow = 10; // this is set very low so that returning from a retry still ends up having data in the buckets for the statisticalWindow
                HystrixCommandPropertiesSetter properties = UnitTestSetterFactory.GetCommandPropertiesSetter().WithCircuitBreakerSleepWindowInMilliseconds(sleepWindow).WithMetricsRollingStatisticalWindowInMilliseconds(statisticalWindow);
                HystrixCommandMetrics metrics = GetMetrics(properties);
                IHystrixCircuitBreaker cb = GetCircuitBreaker(key, CommandGroupForUnitTest.OwnerTwo, metrics, properties);

                // fail
                metrics.MarkFailure(1000);
                metrics.MarkFailure(1000);
                metrics.MarkFailure(1000);
                metrics.MarkFailure(1000);

                // everything has failed in the test window so we should return false now
                Assert.IsFalse(cb.AllowRequest());
                Assert.IsTrue(cb.IsOpen());

                // wait for sleepWindow to pass
                Thread.Sleep(sleepWindow + 50);

                // we should now allow 1 request
                Assert.IsTrue(cb.AllowRequest());
                // but the circuit should still be open
                Assert.IsTrue(cb.IsOpen());
                // and further requests are still blocked
                Assert.IsFalse(cb.AllowRequest());

                // the 'singleTest' succeeds so should cause the circuit to be closed
                metrics.MarkSuccess(500);
                cb.MarkSuccess();

                // all requests should be open again
                Assert.IsTrue(cb.AllowRequest());
                Assert.IsTrue(cb.AllowRequest());
                Assert.IsTrue(cb.AllowRequest());
                // and the circuit should be closed again
                Assert.IsFalse(cb.IsOpen());

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Assert.Fail("Error occurred: " + e.Message);
            }
        }

        /**
         * Over a period of several 'windows' a single attempt will be made and fail and then finally succeed and close the circuit.
         * <p>
         * Ensure the circuit is kept open through the entire testing period and that only the single attempt in each window is made.
         */
        [TestMethod]
        public void CircuitBreaker_MultipleTimeWindowRetriesBeforeClosingCircuit()
        {
            try
            {
                int sleepWindow = 200;
                HystrixCommandPropertiesSetter properties = UnitTestSetterFactory.GetCommandPropertiesSetter().WithCircuitBreakerSleepWindowInMilliseconds(sleepWindow);
                HystrixCommandMetrics metrics = GetMetrics(properties);
                IHystrixCircuitBreaker cb = GetCircuitBreaker(key, CommandGroupForUnitTest.OwnerTwo, metrics, properties);

                // fail
                metrics.MarkFailure(1000);
                metrics.MarkFailure(1000);
                metrics.MarkFailure(1000);
                metrics.MarkFailure(1000);

                // everything has failed in the test window so we should return false now
                Assert.IsFalse(cb.AllowRequest());
                Assert.IsTrue(cb.IsOpen());

                // wait for sleepWindow to pass
                Thread.Sleep(sleepWindow + 50);

                // we should now allow 1 request
                Assert.IsTrue(cb.AllowRequest());
                // but the circuit should still be open
                Assert.IsTrue(cb.IsOpen());
                // and further requests are still blocked
                Assert.IsFalse(cb.AllowRequest());

                // the 'singleTest' fails so it should go back to sleep and not allow any requests again until another 'singleTest' after the sleep
                metrics.MarkFailure(1000);

                Assert.IsFalse(cb.AllowRequest());
                Assert.IsFalse(cb.AllowRequest());
                Assert.IsFalse(cb.AllowRequest());

                // wait for sleepWindow to pass
                Thread.Sleep(sleepWindow + 50);

                // we should now allow 1 request
                Assert.IsTrue(cb.AllowRequest());
                // but the circuit should still be open
                Assert.IsTrue(cb.IsOpen());
                // and further requests are still blocked
                Assert.IsFalse(cb.AllowRequest());

                // the 'singleTest' fails again so it should go back to sleep and not allow any requests again until another 'singleTest' after the sleep
                metrics.MarkFailure(1000);

                Assert.IsFalse(cb.AllowRequest());
                Assert.IsFalse(cb.AllowRequest());
                Assert.IsFalse(cb.AllowRequest());

                // wait for sleepWindow to pass
                Thread.Sleep(sleepWindow + 50);

                // we should now allow 1 request
                Assert.IsTrue(cb.AllowRequest());
                // but the circuit should still be open
                Assert.IsTrue(cb.IsOpen());
                // and further requests are still blocked
                Assert.IsFalse(cb.AllowRequest());

                // now it finally succeeds
                metrics.MarkSuccess(200);
                cb.MarkSuccess();

                // all requests should be open again
                Assert.IsTrue(cb.AllowRequest());
                Assert.IsTrue(cb.AllowRequest());
                Assert.IsTrue(cb.AllowRequest());
                // and the circuit should be closed again
                Assert.IsFalse(cb.IsOpen());

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Assert.Fail("Error occurred: " + e.Message);
            }
        }

        /**
         * When volume of reporting during a statistical window is lower than a defined threshold the circuit
         * will not trip regardless of whatever statistics are calculated.
         */
        [TestMethod]
        public void CircuitBreaker_LowVolumeDoesNotTripCircuit()
        {
            try
            {
                int sleepWindow = 200;
                int lowVolume = 5;

                HystrixCommandPropertiesSetter properties = UnitTestSetterFactory.GetCommandPropertiesSetter().WithCircuitBreakerSleepWindowInMilliseconds(sleepWindow).WithCircuitBreakerRequestVolumeThreshold(lowVolume);
                HystrixCommandMetrics metrics = GetMetrics(properties);
                IHystrixCircuitBreaker cb = GetCircuitBreaker(key, CommandGroupForUnitTest.OwnerTwo, metrics, properties);

                // fail
                metrics.MarkFailure(1000);
                metrics.MarkFailure(1000);
                metrics.MarkFailure(1000);
                metrics.MarkFailure(1000);

                // even though it has all failed we won't trip the circuit because the volume is low
                Assert.IsTrue(cb.AllowRequest());
                Assert.IsFalse(cb.IsOpen());

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Assert.Fail("Error occurred: " + e.Message);
            }
        }










        internal static HystrixCommandMetrics GetMetrics(HystrixCommandPropertiesSetter properties)
        {
            return new HystrixCommandMetrics(CommandKeyForUnitTest.KeyOne, CommandGroupForUnitTest.OwnerOne, new MockingHystrixCommandProperties(properties), HystrixEventNotifierDefault.Instance);
        }

        private static IHystrixCircuitBreaker GetCircuitBreaker(HystrixCommandKey key, HystrixCommandGroupKey commandGroup, HystrixCommandMetrics metrics, HystrixCommandPropertiesSetter properties)
        {
            return new HystrixCircuitBreakerImpl(new MockingHystrixCommandProperties(properties), metrics);
        }
    }
}
