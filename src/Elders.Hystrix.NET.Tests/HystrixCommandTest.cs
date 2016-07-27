namespace Elders.Hystrix.NET.Test
{
    using System;
    using System.Linq;
    using System.Threading;
    using Elders.Hystrix.NET.Test.CircuitBreakerTestImplementations;
    using Elders.Hystrix.NET.Test.HystrixCommandTestImplementations;
    using Java.Util.Concurrent;
    using Java.Util.Concurrent.Atomic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Elders.Hystrix.NET;
    using Elders.Hystrix.NET.Exceptions;
    using Elders.Hystrix.NET.Strategy.Concurrency;
    using Elders.Hystrix.NET.Util;

    [TestClass]
    public class HystrixCommandTest
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


        [TestInitialize]
        public void PrepareForTest()
        {
            /* we must call this to simulate a new request lifecycle running and clearing caches */
            HystrixRequestContext.InitializeContext();
        }

        [TestCleanup]
        public void Cleanup()
        {
            // instead of storing the reference from initialize we'll just get the current state and shutdown
            if (HystrixRequestContext.ContextForCurrentThread != null)
            {
                // it could have been set NULL by the test
            }
        }

        /**
         * Test a successful command execution.
         */
        [TestMethod]
        [TestCategory("TestCategory")]
        public void Command_ExecutionSuccess()
        {
            TestHystrixCommand<bool> command = new SuccessfulTestCommand();
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(true, command.Execute());
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));

            Assert.AreEqual(null, command.FailedExecutionException);


            Assert.IsTrue(command.ExecutionTimeInMilliseconds > -1);
            Assert.IsTrue(command.IsSuccessfulExecution);

            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(0, command.Builder.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(1, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());

            Hystrix.Reset();
        }


        /**
         * Test that a command can not be executed multiple times.
         */
        [TestMethod]
        public void Command_ExecutionMultipleTimes()
        {
            SuccessfulTestCommand command = new SuccessfulTestCommand();
            Assert.IsFalse(command.IsExecutionComplete);
            // first should succeed
            Assert.AreEqual(true, command.Execute());
            Assert.IsTrue(command.IsExecutionComplete);
            Assert.IsTrue(command.IsExecutedInThread);
            Assert.IsTrue(command.ExecutionTimeInMilliseconds > -1);
            Assert.IsTrue(command.IsSuccessfulExecution);
            try
            {
                // second should fail
                command.Execute();
                Assert.Fail("we should not allow this ... it breaks the state of request logs");
            }
            catch (Exception)
            {

                // we want to get here
            }

            try
            {
                // queue should also fail
                command.Queue();
                Assert.Fail("we should not allow this ... it breaks the state of request logs");
            }
            catch (Exception)
            {

                // we want to get here
            }

            Hystrix.Reset();
        }

        /**
         * Test a command execution that throws an HystrixException and didn't implement getFallback.
         */
        [TestMethod]
        public void Command_ExecutionKnownFailureWithNoFallback()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
            TestHystrixCommand<bool> command = new KnownFailureTestCommandWithoutFallback(circuitBreaker);
            try
            {
                command.Execute();
                Assert.Fail("we shouldn't get here");
            }
            catch (HystrixRuntimeException e)
            {
                Assert.IsNotNull(e.FallbackException);
                Assert.IsNotNull(e.CommandType);
                Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
                Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
                Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
                Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            }
            catch (Exception)
            {

                Assert.Fail("We should always get an HystrixRuntimeException when an error occurs.");
            }
            Assert.IsTrue(command.ExecutionTimeInMilliseconds > -1);
            Assert.IsTrue(command.IsFailedExecution);

            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(100, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(1, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());

            Hystrix.Reset();
        }

        /**
         * Test a command execution that throws an unknown exception (not HystrixException) and didn't implement getFallback.
         */
        [TestMethod]
        public void Command_ExecutionUnknownFailureWithNoFallback()
        {
            TestHystrixCommand<bool> command = new UnknownFailureTestCommandWithoutFallback();
            try
            {
                command.Execute();
                Assert.Fail("we shouldn't get here");
            }
            catch (HystrixRuntimeException e)
            {

                Assert.IsNotNull(e.FallbackException);
                Assert.IsNotNull(e.CommandType);

            }
            catch (Exception)
            {

                Assert.Fail("We should always get an HystrixRuntimeException when an error occurs.");
            }

            Assert.IsTrue(command.ExecutionTimeInMilliseconds > -1);
            Assert.IsTrue(command.IsFailedExecution);

            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(100, command.Builder.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(1, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());

            Hystrix.Reset();
        }

        /**
         * Test a command execution that fails but has a fallback.
         */
        [TestMethod]
        public void Command_ExecutionFailureWithFallback()
        {
            TestHystrixCommand<bool> command = new KnownFailureTestCommandWithFallback(new TestCircuitBreaker());
            try
            {
                Assert.AreEqual(false, command.Execute());
            }
            catch (Exception)
            {

                Assert.Fail("We should have received a response from the fallback.");
            }

            Assert.AreEqual("we failed with a simulated issue", command.FailedExecutionException.Message);

            Assert.IsTrue(command.ExecutionTimeInMilliseconds > -1);
            Assert.IsTrue(command.IsFailedExecution);

            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(100, command.Builder.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(1, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());

            Hystrix.Reset();
        }

        /**
         * Test a command execution that fails, has getFallback implemented but that fails as well.
         */
        [TestMethod]
        public void Command_ExecutionFailureWithFallbackFailure()
        {
            TestHystrixCommand<bool> command = new KnownFailureTestCommandWithFallbackFailure();
            try
            {
                command.Execute();
                Assert.Fail("we shouldn't get here");
            }
            catch (HystrixRuntimeException e)
            {
                TestContext.WriteLine("------------------------------------------------");
                TestContext.WriteLine(e.ToString());
                TestContext.WriteLine("------------------------------------------------");
                Assert.IsNotNull(e.FallbackException);
            }

            Assert.IsTrue(command.ExecutionTimeInMilliseconds > -1);
            Assert.IsTrue(command.IsFailedExecution);

            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(100, command.Builder.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(1, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());

            Hystrix.Reset();
        }

        /**
         * Test a successful command execution (asynchronously).
         */
        [TestMethod]
        public void Command_QueueSuccess()
        {
            TestHystrixCommand<bool> command = new SuccessfulTestCommand();
            try
            {
                IFuture<bool> future = command.Queue();
                Assert.AreEqual(true, future.Get());
            }
            catch (Exception)
            {

                Assert.Fail("We received an exception.");
            }

            Assert.IsTrue(command.ExecutionTimeInMilliseconds > -1);
            Assert.IsTrue(command.IsSuccessfulExecution);

            Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(0, command.Builder.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(1, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());

            Hystrix.Reset();
        }

        /**
         * Test a command execution (asynchronously) that throws an HystrixException and didn't implement getFallback.
         */
        [TestMethod]
        public void Command_QueueKnownFailureWithNoFallback()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
            TestHystrixCommand<bool> command = new KnownFailureTestCommandWithoutFallback(circuitBreaker);
            try
            {
                IFuture<bool> future = command.Queue();
                future.Get();
                Assert.Fail("we shouldn't get here");
            }
            catch (Exception e)
            {

                if (e.InnerException is HystrixRuntimeException)
                {
                    HystrixRuntimeException de = (HystrixRuntimeException)e.InnerException;

                    Assert.IsNotNull(de.FallbackException);
                    Assert.IsNotNull(de.CommandType);
                }
                else
                {
                    Assert.Fail("the cause should be HystrixRuntimeException");
                }
            }

            Assert.IsTrue(command.ExecutionTimeInMilliseconds > -1);
            Assert.IsTrue(command.IsFailedExecution);

            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(100, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(1, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());

            Hystrix.Reset();
        }

        /**
         * Test a command execution (asynchronously) that throws an unknown exception (not HystrixException) and didn't implement getFallback.
         */
        [TestMethod]
        public void Command_QueueUnknownFailureWithNoFallback()
        {
            TestHystrixCommand<bool> command = new UnknownFailureTestCommandWithoutFallback();
            try
            {
                command.Queue().Get();
                Assert.Fail("we shouldn't get here");
            }
            catch (Exception e)
            {

                if (e.InnerException is HystrixRuntimeException)
                {
                    HystrixRuntimeException de = (HystrixRuntimeException)e.InnerException;
                    Assert.IsNotNull(de.FallbackException);
                    Assert.IsNotNull(de.CommandType);
                }
                else
                {
                    Assert.Fail("the cause should be HystrixRuntimeException");
                }
            }

            Assert.IsTrue(command.ExecutionTimeInMilliseconds > -1);
            Assert.IsTrue(command.IsFailedExecution);

            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(100, command.Builder.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(1, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());

            Hystrix.Reset();
        }

        /**
         * Test a command execution (asynchronously) that fails but has a fallback.
         */
        [TestMethod]
        public void Command_QueueFailureWithFallback()
        {
            TestHystrixCommand<bool> command = new KnownFailureTestCommandWithFallback(new TestCircuitBreaker());
            try
            {
                IFuture<bool> future = command.Queue();
                Assert.AreEqual(false, future.Get());
            }
            catch (Exception)
            {

                Assert.Fail("We should have received a response from the fallback.");
            }

            Assert.IsTrue(command.ExecutionTimeInMilliseconds > -1);
            Assert.IsTrue(command.IsFailedExecution);

            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(100, command.Builder.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(1, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());

            Hystrix.Reset();
        }

        /**
         * Test a command execution (asynchronously) that fails, has getFallback implemented but that fails as well.
         */
        [TestMethod]
        public void Command_QueueFailureWithFallbackFailure()
        {
            TestHystrixCommand<bool> command = new KnownFailureTestCommandWithFallbackFailure();
            try
            {
                command.Queue().Get();
                Assert.Fail("we shouldn't get here");
            }
            catch (Exception e)
            {
                if (e.InnerException is HystrixRuntimeException)
                {
                    HystrixRuntimeException de = (HystrixRuntimeException)e.InnerException;

                    Assert.IsNotNull(de.FallbackException);
                }
                else
                {
                    Assert.Fail("the cause should be HystrixRuntimeException");
                }
            }

            Assert.IsTrue(command.ExecutionTimeInMilliseconds > -1);
            Assert.IsTrue(command.IsFailedExecution);

            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(100, command.Builder.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(1, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());

            Hystrix.Reset();
        }

        /**
         * Test that the circuit-breaker will 'trip' and prevent command execution on subsequent calls.
         */
        [TestMethod]
        public void Command_CircuitBreakerTripsAfterFailures()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
            /* fail 3 times and then it should trip the circuit and stop executing */
            // failure 1
            KnownFailureTestCommandWithFallback attempt1 = new KnownFailureTestCommandWithFallback(circuitBreaker);
            attempt1.Execute();
            Assert.IsTrue(attempt1.IsResponseFromFallback);
            Assert.IsFalse(attempt1.IsCircuitBreakerOpen);
            Assert.IsFalse(attempt1.IsResponseShortCircuited);

            // failure 2
            KnownFailureTestCommandWithFallback attempt2 = new KnownFailureTestCommandWithFallback(circuitBreaker);
            attempt2.Execute();
            Assert.IsTrue(attempt2.IsResponseFromFallback);
            Assert.IsFalse(attempt2.IsCircuitBreakerOpen);
            Assert.IsFalse(attempt2.IsResponseShortCircuited);

            // failure 3
            KnownFailureTestCommandWithFallback attempt3 = new KnownFailureTestCommandWithFallback(circuitBreaker);
            attempt3.Execute();
            Assert.IsTrue(attempt3.IsResponseFromFallback);
            Assert.IsFalse(attempt3.IsResponseShortCircuited);
            // it should now be 'open' and prevent further executions
            Assert.IsTrue(attempt3.IsCircuitBreakerOpen);

            // attempt 4
            KnownFailureTestCommandWithFallback attempt4 = new KnownFailureTestCommandWithFallback(circuitBreaker);
            attempt4.Execute();
            Assert.IsTrue(attempt4.IsResponseFromFallback);
            // this should now be true as the response will be short-circuited
            Assert.IsTrue(attempt4.IsResponseShortCircuited);
            // this should remain open
            Assert.IsTrue(attempt4.IsCircuitBreakerOpen);

            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(3, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(4, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(100, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(4, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());

            Hystrix.Reset();
        }

        /**
         * Test that the circuit-breaker will 'trip' and prevent command execution on subsequent calls.
         */
        [TestMethod]
        public void Command_CircuitBreakerTripsAfterFailuresViaQueue()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
            try
            {
                /* fail 3 times and then it should trip the circuit and stop executing */
                // failure 1
                KnownFailureTestCommandWithFallback attempt1 = new KnownFailureTestCommandWithFallback(circuitBreaker);
                attempt1.Queue().Get();
                Assert.IsTrue(attempt1.IsResponseFromFallback);
                Assert.IsFalse(attempt1.IsCircuitBreakerOpen);
                Assert.IsFalse(attempt1.IsResponseShortCircuited);

                // failure 2
                KnownFailureTestCommandWithFallback attempt2 = new KnownFailureTestCommandWithFallback(circuitBreaker);
                attempt2.Queue().Get();
                Assert.IsTrue(attempt2.IsResponseFromFallback);
                Assert.IsFalse(attempt2.IsCircuitBreakerOpen);
                Assert.IsFalse(attempt2.IsResponseShortCircuited);

                // failure 3
                KnownFailureTestCommandWithFallback attempt3 = new KnownFailureTestCommandWithFallback(circuitBreaker);
                attempt3.Queue().Get();
                Assert.IsTrue(attempt3.IsResponseFromFallback);
                Assert.IsFalse(attempt3.IsResponseShortCircuited);
                // it should now be 'open' and prevent further executions
                Assert.IsTrue(attempt3.IsCircuitBreakerOpen);

                // attempt 4
                KnownFailureTestCommandWithFallback attempt4 = new KnownFailureTestCommandWithFallback(circuitBreaker);
                attempt4.Queue().Get();
                Assert.IsTrue(attempt4.IsResponseFromFallback);
                // this should now be true as the response will be short-circuited
                Assert.IsTrue(attempt4.IsResponseShortCircuited);
                // this should remain open
                Assert.IsTrue(attempt4.IsCircuitBreakerOpen);

                Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
                Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
                Assert.AreEqual(3, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
                Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
                Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
                Assert.AreEqual(4, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
                Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
                Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
                Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
                Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
                Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

                Assert.AreEqual(100, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);

                Assert.AreEqual(4, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());
            }
            catch (Exception)
            {

                Assert.Fail("We should have received fallbacks.");
            }

            Hystrix.Reset();
        }

        /**
         * Test that the circuit-breaker is shared across HystrixCommand objects with the same CommandKey.
         * <p>
         * This will test HystrixCommand objects with a single circuit-breaker (as if each injected with same CommandKey)
         * <p>
         * Multiple HystrixCommand objects with the same dependency use the same circuit-breaker.
         */
        [TestMethod]
        public void Command_CircuitBreakerAcrossMultipleCommandsButSameCircuitBreaker()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
            /* fail 3 times and then it should trip the circuit and stop executing */
            // failure 1
            KnownFailureTestCommandWithFallback attempt1 = new KnownFailureTestCommandWithFallback(circuitBreaker);
            attempt1.Execute();
            Assert.IsTrue(attempt1.IsResponseFromFallback);
            Assert.IsFalse(attempt1.IsCircuitBreakerOpen);
            Assert.IsFalse(attempt1.IsResponseShortCircuited);

            // failure 2 with a different command, same circuit breaker
            KnownFailureTestCommandWithoutFallback attempt2 = new KnownFailureTestCommandWithoutFallback(circuitBreaker);
            try
            {
                attempt2.Execute();
            }
            catch (Exception)
            {
                // ignore ... this doesn't have a fallback so will throw an exception
            }
            Assert.IsTrue(attempt2.IsFailedExecution);
            Assert.IsFalse(attempt2.IsResponseFromFallback); // false because no fallback
            Assert.IsFalse(attempt2.IsCircuitBreakerOpen);
            Assert.IsFalse(attempt2.IsResponseShortCircuited);

            // failure 3 of the Hystrix, 2nd for this particular HystrixCommand
            KnownFailureTestCommandWithFallback attempt3 = new KnownFailureTestCommandWithFallback(circuitBreaker);
            attempt3.Execute();
            Assert.IsTrue(attempt2.IsFailedExecution);
            Assert.IsTrue(attempt3.IsResponseFromFallback);
            Assert.IsFalse(attempt3.IsResponseShortCircuited);

            // it should now be 'open' and prevent further executions
            // after having 3 failures on the Hystrix that these 2 different HystrixCommand objects are for
            Assert.IsTrue(attempt3.IsCircuitBreakerOpen);

            // attempt 4
            KnownFailureTestCommandWithFallback attempt4 = new KnownFailureTestCommandWithFallback(circuitBreaker);
            attempt4.Execute();
            Assert.IsTrue(attempt4.IsResponseFromFallback);
            // this should now be true as the response will be short-circuited
            Assert.IsTrue(attempt4.IsResponseShortCircuited);
            // this should remain open
            Assert.IsTrue(attempt4.IsCircuitBreakerOpen);

            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(3, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(3, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(100, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(4, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());
            Hystrix.Reset();
        }

        /**
         * Test that the circuit-breaker is different between HystrixCommand objects with a different Hystrix.
         */
        [TestMethod]
        public void Command_CircuitBreakerAcrossMultipleCommandsAndDifferentDependency()
        {
            TestCircuitBreaker circuitBreaker_one = new TestCircuitBreaker();
            TestCircuitBreaker circuitBreaker_two = new TestCircuitBreaker();
            /* fail 3 times, twice on one Hystrix, once on a different Hystrix ... circuit-breaker should NOT open */

            // failure 1
            KnownFailureTestCommandWithFallback attempt1 = new KnownFailureTestCommandWithFallback(circuitBreaker_one);
            attempt1.Execute();
            Assert.IsTrue(attempt1.IsResponseFromFallback);
            Assert.IsFalse(attempt1.IsCircuitBreakerOpen);
            Assert.IsFalse(attempt1.IsResponseShortCircuited);

            // failure 2 with a different HystrixCommand implementation and different Hystrix
            KnownFailureTestCommandWithFallback attempt2 = new KnownFailureTestCommandWithFallback(circuitBreaker_two);
            attempt2.Execute();
            Assert.IsTrue(attempt2.IsResponseFromFallback);
            Assert.IsFalse(attempt2.IsCircuitBreakerOpen);
            Assert.IsFalse(attempt2.IsResponseShortCircuited);

            // failure 3 but only 2nd of the Hystrix.ONE
            KnownFailureTestCommandWithFallback attempt3 = new KnownFailureTestCommandWithFallback(circuitBreaker_one);
            attempt3.Execute();
            Assert.IsTrue(attempt3.IsResponseFromFallback);
            Assert.IsFalse(attempt3.IsResponseShortCircuited);

            // it should remain 'closed' since we have only had 2 failures on Hystrix.ONE
            Assert.IsFalse(attempt3.IsCircuitBreakerOpen);

            // this one should also remain closed as it only had 1 failure for Hystrix.TWO
            Assert.IsFalse(attempt2.IsCircuitBreakerOpen);

            // attempt 4 (3rd attempt for Hystrix.ONE)
            KnownFailureTestCommandWithFallback attempt4 = new KnownFailureTestCommandWithFallback(circuitBreaker_one);
            attempt4.Execute();
            // this should NOW flip to true as this is the 3rd failure for Hystrix.ONE
            Assert.IsTrue(attempt3.IsCircuitBreakerOpen);
            Assert.IsTrue(attempt3.IsResponseFromFallback);
            Assert.IsFalse(attempt3.IsResponseShortCircuited);

            // Hystrix.TWO should still remain closed
            Assert.IsFalse(attempt2.IsCircuitBreakerOpen);

            Assert.AreEqual(0, circuitBreaker_one.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(0, circuitBreaker_one.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(3, circuitBreaker_one.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, circuitBreaker_one.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, circuitBreaker_one.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(3, circuitBreaker_one.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, circuitBreaker_one.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, circuitBreaker_one.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, circuitBreaker_one.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(0, circuitBreaker_one.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, circuitBreaker_one.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(100, circuitBreaker_one.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(0, circuitBreaker_two.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(0, circuitBreaker_two.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(1, circuitBreaker_two.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, circuitBreaker_two.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, circuitBreaker_two.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(1, circuitBreaker_two.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, circuitBreaker_two.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, circuitBreaker_two.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, circuitBreaker_two.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(0, circuitBreaker_two.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, circuitBreaker_two.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(100, circuitBreaker_two.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(4, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());
            Hystrix.Reset();
        }

        /**
         * Test that the circuit-breaker being disabled doesn't wreak havoc.
         */
        [TestMethod]
        public void Command_ExecutionSuccessWithCircuitBreakerDisabled()
        {
            TestHystrixCommand<bool> command = new TestCommandWithoutCircuitBreaker();
            try
            {
                Assert.AreEqual(true, command.Execute());
            }
            catch (Exception)
            {

                Assert.Fail("We received an exception.");
            }

            // we'll still get metrics ... just not the circuit breaker opening/closing
            Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(0, command.Builder.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(1, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());
            Hystrix.Reset();
        }

        /**
         * Test a command execution timeout where the command didn't implement getFallback.
         */
        [TestMethod]
        public void Command_ExecutionTimeoutWithNoFallback()
        {
            TestHystrixCommand<bool> command = new TestCommandWithTimeout(TimeSpan.FromMilliseconds(50.0), TestCommandWithTimeout.FallbackNotImplemented);
            try
            {
                command.Execute();
                Assert.Fail("we shouldn't get here");
            }
            catch (Exception e)
            {
                //                
                if (e is HystrixRuntimeException)
                {
                    HystrixRuntimeException de = (HystrixRuntimeException)e;
                    Assert.IsNotNull(de.FallbackException);
                    Assert.IsTrue(de.FallbackException is NotSupportedException);
                    Assert.IsNotNull(de.CommandType);
                    Assert.IsNotNull(de.InnerException);
                    Assert.IsTrue(de.InnerException is TimeoutException);
                }
                else
                {
                    Assert.Fail("the exception should be HystrixRuntimeException");
                }
            }
            // the time should be 50+ since we timeout at 50ms
            Assert.IsTrue(command.ExecutionTimeInMilliseconds >= 50, "Execution Time is: " + command.ExecutionTimeInMilliseconds);

            Assert.IsTrue(command.IsResponseTimedOut);
            Assert.IsFalse(command.IsResponseFromFallback);
            Assert.IsFalse(command.IsResponseRejected);

            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(100, command.Builder.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(1, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());
            Hystrix.Reset();
        }

        /**
         * Test a command execution timeout where the command implemented getFallback.
         */
        [TestMethod]
        public void Command_ExecutionTimeoutWithFallback()
        {
            TestHystrixCommand<bool> command = new TestCommandWithTimeout(TimeSpan.FromMilliseconds(50.0), TestCommandWithTimeout.FallbackSuccess);
            try
            {
                Assert.AreEqual(false, command.Execute());
                // the time should be 50+ since we timeout at 50ms
                Assert.IsTrue(command.ExecutionTimeInMilliseconds >= 50, "Execution Time is: " + command.ExecutionTimeInMilliseconds);
                Assert.IsTrue(command.IsResponseTimedOut);
                Assert.IsTrue(command.IsResponseFromFallback);
            }
            catch (Exception)
            {

                Assert.Fail("We should have received a response from the fallback.");
            }

            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(100, command.Builder.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(1, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());
            Hystrix.Reset();
        }

        /**
         * Test a command execution timeout where the command implemented getFallback but it fails.
         */
        [TestMethod]
        public void Command_ExecutionTimeoutFallbackFailure()
        {
            TestHystrixCommand<bool> command = new TestCommandWithTimeout(TimeSpan.FromMilliseconds(50.0), TestCommandWithTimeout.FallbackFailure);
            try
            {
                command.Execute();
                Assert.Fail("we shouldn't get here");
            }
            catch (Exception e)
            {
                if (e is HystrixRuntimeException)
                {
                    HystrixRuntimeException de = (HystrixRuntimeException)e;
                    Assert.IsNotNull(de.FallbackException);
                    Assert.IsFalse(de.FallbackException is NotSupportedException);
                    Assert.IsNotNull(de.CommandType);
                    Assert.IsNotNull(de.InnerException);
                    Assert.IsTrue(de.InnerException is TimeoutException);
                }
                else
                {
                    Assert.Fail("the exception should be HystrixRuntimeException");
                }
            }
            // the time should be 50+ since we timeout at 50ms
            Assert.IsTrue(command.ExecutionTimeInMilliseconds >= 50, "Execution Time is: " + command.ExecutionTimeInMilliseconds);
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(100, command.Builder.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(1, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());
            Hystrix.Reset();
        }

        /**
         * Test that the circuit-breaker counts a command execution timeout as a 'timeout' and not just failure.
         */
        [TestMethod]
        public void Command_CircuitBreakerOnExecutionTimeout()
        {
            TestHystrixCommand<bool> command = new TestCommandWithTimeout(TimeSpan.FromMilliseconds(50.0), TestCommandWithTimeout.FallbackSuccess);
            try
            {
                Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));

                command.Execute();

                Assert.IsTrue(command.IsResponseFromFallback);
                Assert.IsFalse(command.IsCircuitBreakerOpen);
                Assert.IsFalse(command.IsResponseShortCircuited);

                Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
                Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
                Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
                Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));

            }
            catch (Exception)
            {

                Assert.Fail("We should have received a response from the fallback.");
            }

            Assert.IsTrue(command.ExecutionTimeInMilliseconds > -1);
            Assert.IsTrue(command.IsResponseTimedOut);

            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(100, command.Builder.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(1, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());
            Hystrix.Reset();
        }

        /**
         * Test that the command finishing AFTER a timeout (because thread continues in background) does not register a Success
         */
        [TestMethod]
        public void Command_CountersOnExecutionTimeout()
        {
            TestHystrixCommand<bool> command = new TestCommandWithTimeout(TimeSpan.FromMilliseconds(50.0), TestCommandWithTimeout.FallbackSuccess);
            try
            {
                command.Execute();

                /* wait long enough for the command to have finished */
                Thread.Sleep(200);

                /* response should still be the same as 'testCircuitBreakerOnExecutionTimeout' */
                Assert.IsTrue(command.IsResponseFromFallback);
                Assert.IsFalse(command.IsCircuitBreakerOpen);
                Assert.IsFalse(command.IsResponseShortCircuited);

                Assert.IsTrue(command.ExecutionTimeInMilliseconds > -1);
                Assert.IsTrue(command.IsResponseTimedOut);
                Assert.IsFalse(command.IsSuccessfulExecution);

                /* failure and timeout count should be the same as 'testCircuitBreakerOnExecutionTimeout' */
                Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
                Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));

                /* we should NOT have a 'success' counter */
                Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));

            }
            catch (Exception)
            {

                Assert.Fail("We should have received a response from the fallback.");
            }

            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(100, command.Builder.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(1, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());
            Hystrix.Reset();
        }

        /**
         * Test a queued command execution timeout where the command didn't implement getFallback.
         * <p>
         * We specifically want to protect against developers queuing commands and using queue().Get() without a timeout (such as queue().get(3000, TimeUnit.Milliseconds)) and ending up blocking
         * indefinitely by skipping the timeout protection of the Execute() command.
         */
        [TestMethod]
        public void Command_QueuedExecutionTimeoutWithNoFallback()
        {
            TestHystrixCommand<bool> command = new TestCommandWithTimeout(TimeSpan.FromMilliseconds(50), TestCommandWithTimeout.FallbackNotImplemented);
            try
            {
                command.Queue().Get();
                Assert.Fail("we shouldn't get here");
            }
            catch (Exception e)
            {

                if (e is ExecutionException && e.InnerException is HystrixRuntimeException)
                {
                    HystrixRuntimeException de = (HystrixRuntimeException)e.InnerException;
                    Assert.IsNotNull(de.FallbackException);
                    Assert.IsTrue(de.FallbackException is NotSupportedException);
                    Assert.IsNotNull(de.CommandType);
                    Assert.IsNotNull(de.InnerException);
                    Assert.IsTrue(de.InnerException is TimeoutException);
                }
                else
                {
                    Assert.Fail("the exception should be ExecutionException with cause as HystrixRuntimeException");
                }
            }

            Assert.IsTrue(command.ExecutionTimeInMilliseconds > -1);
            Assert.IsTrue(command.IsResponseTimedOut);

            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(100, command.Builder.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(1, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());
            Hystrix.Reset();
        }

        /**
         * Test a queued command execution timeout where the command implemented getFallback.
         * <p>
         * We specifically want to protect against developers queuing commands and using queue().Get() without a timeout (such as queue().get(3000, TimeUnit.Milliseconds)) and ending up blocking
         * indefinitely by skipping the timeout protection of the Execute() command.
         */
        [TestMethod]
        public void Command_QueuedExecutionTimeoutWithFallback()
        {
            TestHystrixCommand<bool> command = new TestCommandWithTimeout(TimeSpan.FromMilliseconds(50.0), TestCommandWithTimeout.FallbackSuccess);
            try
            {
                Assert.AreEqual(false, command.Queue().Get());
            }
            catch (Exception)
            {

                Assert.Fail("We should have received a response from the fallback.");
            }

            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(100, command.Builder.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(1, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());
            Hystrix.Reset();
        }

        /**
         * Test a queued command execution timeout where the command implemented getFallback but it fails.
         * <p>
         * We specifically want to protect against developers queuing commands and using queue().Get() without a timeout (such as queue().get(3000, TimeUnit.Milliseconds)) and ending up blocking
         * indefinitely by skipping the timeout protection of the Execute() command.
         */
        [TestMethod]
        public void Command_QueuedExecutionTimeoutFallbackFailure()
        {
            TestHystrixCommand<bool> command = new TestCommandWithTimeout(TimeSpan.FromMilliseconds(50.0), TestCommandWithTimeout.FallbackFailure);
            try
            {
                command.Queue().Get();
                Assert.Fail("we shouldn't get here");
            }
            catch (Exception e)
            {
                if (e is ExecutionException && e.InnerException is HystrixRuntimeException)
                {
                    HystrixRuntimeException de = (HystrixRuntimeException)e.InnerException;
                    Assert.IsNotNull(de.FallbackException);
                    Assert.IsFalse(de.FallbackException is NotSupportedException);
                    Assert.IsNotNull(de.CommandType);
                    Assert.IsNotNull(de.InnerException);
                    Assert.IsTrue(de.InnerException is TimeoutException);
                }
                else
                {
                    Assert.Fail("the exception should be ExecutionException with cause as HystrixRuntimeException");
                }
            }

            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(100, command.Builder.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(1, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());
            Hystrix.Reset();
        }

        /**
         * Test that the circuit-breaker counts a command execution timeout as a 'timeout' and not just failure.
         */
        [TestMethod]
        public void Command_ShortCircuitFallbackCounter()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker().SetForceShortCircuit(true);
            try
            {
                new KnownFailureTestCommandWithFallback(circuitBreaker).Execute();

                Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));

                KnownFailureTestCommandWithFallback command = new KnownFailureTestCommandWithFallback(circuitBreaker);
                command.Execute();
                Assert.AreEqual(2, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));

                // will be -1 because it never attempted execution
                Assert.IsTrue(command.ExecutionTimeInMilliseconds == -1);
                Assert.IsTrue(command.IsResponseShortCircuited);
                Assert.IsFalse(command.IsResponseTimedOut);

                // because it was short-circuited to a fallback we don't count an error
                Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
                Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
                Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));

            }
            catch (Exception)
            {

                Assert.Fail("We should have received a response from the fallback.");
            }

            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(2, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(2, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(100, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(2, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());
            Hystrix.Reset();
        }

        /**
         * Test when a command fails to get queued up in the threadpool where the command didn't implement getFallback.
         * <p>
         * We specifically want to protect against developers getting random thread exceptions and instead just correctly receiving HystrixRuntimeException when no fallback exists.
         */
        [TestMethod]
        public void Command_RejectedThreadWithNoFallback()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
            SingleThreadedPool pool = new SingleThreadedPool(1);
            // fill up the queue
            pool.Queue.Add(new Runnable(() =>
            {
                try
                {
                    TestContext.WriteLine("**** queue filler1 ****");
                    Thread.Sleep(500);
                }
                catch (ThreadInterruptedException e)
                {
                    TestContext.WriteLine(e.ToString());
                }
            }));

            IFuture<bool> f = null;
            TestCommandRejection command = null;
            try
            {
                f = new TestCommandRejection(circuitBreaker, pool, TimeSpan.FromMilliseconds(500.0), TimeSpan.FromMilliseconds(600), TestCommandRejection.FallbackNotImplemented).Queue();
                command = new TestCommandRejection(circuitBreaker, pool, TimeSpan.FromMilliseconds(500.0), TimeSpan.FromMilliseconds(600), TestCommandRejection.FallbackNotImplemented);
                command.Queue();
                Assert.Fail("we shouldn't get here");
            }
            catch (Exception e)
            {


                // will be -1 because it never attempted execution
                Assert.IsTrue(command.ExecutionTimeInMilliseconds == -1);
                Assert.IsTrue(command.IsResponseRejected);
                Assert.IsFalse(command.IsResponseShortCircuited);
                Assert.IsFalse(command.IsResponseTimedOut);

                Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
                Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
                Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
                Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
                if (e is HystrixRuntimeException && e.InnerException is RejectedExecutionException)
                {
                    HystrixRuntimeException de = (HystrixRuntimeException)e;
                    Assert.IsNotNull(de.FallbackException);
                    Assert.IsTrue(de.FallbackException is NotSupportedException);
                    Assert.IsNotNull(de.CommandType);
                    Assert.IsNotNull(de.InnerException);
                    Assert.IsTrue(de.InnerException is RejectedExecutionException);
                }
                else
                {
                    Assert.Fail("the exception should be HystrixRuntimeException with cause as RejectedExecutionException");
                }
            }

            try
            {
                f.Get();
            }
            catch (Exception)
            {

                Assert.Fail("The first one should succeed.");
            }

            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(50, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(2, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());
            Hystrix.Reset();
        }

        /**
         * Test when a command fails to get queued up in the threadpool where the command implemented getFallback.
         * <p>
         * We specifically want to protect against developers getting random thread exceptions and instead just correctly receives a fallback.
         */
        [TestMethod]
        public void Command_RejectedThreadWithFallback()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
            SingleThreadedPool pool = new SingleThreadedPool(1);
            // fill up the queue
            pool.Queue.Add(new Runnable(() =>
            {
                try
                {
                    TestContext.WriteLine("**** queue filler1 ****");
                    Thread.Sleep(500);
                }
                catch (ThreadInterruptedException e)
                {
                    TestContext.WriteLine(e.ToString());
                }
            }));

            try
            {
                TestCommandRejection command1 = new TestCommandRejection(circuitBreaker, pool, TimeSpan.FromMilliseconds(500.0), TimeSpan.FromMilliseconds(600), TestCommandRejection.FallbackSuccess);
                command1.Queue();
                TestCommandRejection command2 = new TestCommandRejection(circuitBreaker, pool, TimeSpan.FromMilliseconds(500.0), TimeSpan.FromMilliseconds(600), TestCommandRejection.FallbackSuccess);
                Assert.AreEqual(false, command2.Queue().Get());
                Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
                Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
                Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
                Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
                Assert.IsFalse(command1.IsResponseRejected);
                Assert.IsFalse(command1.IsResponseFromFallback);
                Assert.IsTrue(command2.IsResponseRejected);
                Assert.IsTrue(command2.IsResponseFromFallback);
            }
            catch (Exception)
            {

                Assert.Fail("We should have received a response from the fallback.");
            }

            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(100, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(2, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());
            Hystrix.Reset();
        }

        /**
         * Test when a command fails to get queued up in the threadpool where the command implemented getFallback but it fails.
         * <p>
         * We specifically want to protect against developers getting random thread exceptions and instead just correctly receives an HystrixRuntimeException.
         */
        [TestMethod]
        public void Command_RejectedThreadWithFallbackFailure()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
            SingleThreadedPool pool = new SingleThreadedPool(1);
            // fill up the queue
            pool.Queue.Add(new Runnable(() =>
            {
                try
                {
                    TestContext.WriteLine("**** queue filler1 ****");
                    Thread.Sleep(500);
                }
                catch (ThreadInterruptedException e)
                {
                    TestContext.WriteLine(e.ToString());
                }
            }));

            try
            {
                new TestCommandRejection(circuitBreaker, pool, TimeSpan.FromMilliseconds(500.0), TimeSpan.FromMilliseconds(600), TestCommandRejection.FallbackFailure).Queue();
                Assert.AreEqual(false, new TestCommandRejection(circuitBreaker, pool, TimeSpan.FromMilliseconds(500.0), TimeSpan.FromMilliseconds(600), TestCommandRejection.FallbackFailure).Queue().Get());
                Assert.Fail("we shouldn't get here");
            }
            catch (Exception e)
            {

                Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
                if (e is HystrixRuntimeException && e.InnerException is RejectedExecutionException)
                {
                    HystrixRuntimeException de = (HystrixRuntimeException)e;
                    Assert.IsNotNull(de.FallbackException);
                    Assert.IsFalse(de.FallbackException is NotSupportedException);
                    Assert.IsNotNull(de.CommandType);
                    Assert.IsNotNull(de.InnerException);
                    Assert.IsTrue(de.InnerException is RejectedExecutionException);
                }
                else
                {
                    Assert.Fail("the exception should be HystrixRuntimeException with cause as RejectedExecutionException");
                }
            }

            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(100, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(2, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());
            Hystrix.Reset();
        }

        /**
         * Test that we can reject a thread using isQueueSpaceAvailable() instead of just when the pool rejects.
         * <p>
         * For example, we have queue size set to 100 but want to reject when we hit 10.
         * <p>
         * This allows us to use FastProperties to control our rejection point whereas we can't resize a queue after it's created.
         */
        [TestMethod]
        public void Command_RejectedThreadUsingQueueSize()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
            SingleThreadedPool pool = new SingleThreadedPool(10, 1);
            // put 1 item in the queue
            // the thread pool won't pick it up because we're bypassing the pool and adding to the queue directly so this will keep the queue full
            pool.Queue.Add(new Runnable(() =>
            {
                try
                {
                    TestContext.WriteLine("**** queue filler1 ****");
                    Thread.Sleep(500);
                }
                catch (ThreadInterruptedException e)
                {
                    TestContext.WriteLine(e.ToString());
                }
            }));

            TestCommandRejection command = null;
            try
            {
                // this should fail as we already have 1 in the queue
                command = new TestCommandRejection(circuitBreaker, pool, TimeSpan.FromMilliseconds(500.0), TimeSpan.FromMilliseconds(600), TestCommandRejection.FallbackNotImplemented);
                command.Queue();
                Assert.Fail("we shouldn't get here");
            }
            catch (Exception e)
            {


                // will be -1 because it never attempted execution
                Assert.IsTrue(command.ExecutionTimeInMilliseconds == -1);
                Assert.IsTrue(command.IsResponseRejected);
                Assert.IsFalse(command.IsResponseShortCircuited);
                Assert.IsFalse(command.IsResponseTimedOut);

                Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
                if (e is HystrixRuntimeException && e.InnerException is RejectedExecutionException)
                {
                    HystrixRuntimeException de = (HystrixRuntimeException)e;
                    Assert.IsNotNull(de.FallbackException);
                    Assert.IsTrue(de.FallbackException is NotSupportedException);
                    Assert.IsNotNull(de.CommandType);
                    Assert.IsNotNull(de.InnerException);
                    Assert.IsTrue(de.InnerException is RejectedExecutionException);
                }
                else
                {
                    Assert.Fail("the exception should be HystrixRuntimeException with cause as RejectedExecutionException");
                }
            }

            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(100, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(1, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());
            Hystrix.Reset();
        }

        [TestMethod]
        public void Command_TimedOutCommandDoesNotExecute()
        {
            SingleThreadedPool pool = new SingleThreadedPool(5);

            TestCircuitBreaker s1 = new TestCircuitBreaker();
            TestCircuitBreaker s2 = new TestCircuitBreaker();

            // execution will take 100ms, thread pool has a 600ms timeout
            CommandWithCustomThreadPool c1 = new CommandWithCustomThreadPool(s1, pool, TimeSpan.FromMilliseconds(100), UnitTestSetterFactory.GetCommandPropertiesSetter().WithExecutionIsolationThreadTimeoutInMilliseconds(600));
            // execution will take 200ms, thread pool has a 20ms timeout
            CommandWithCustomThreadPool c2 = new CommandWithCustomThreadPool(s2, pool, TimeSpan.FromMilliseconds(200), UnitTestSetterFactory.GetCommandPropertiesSetter().WithExecutionIsolationThreadTimeoutInMilliseconds(20));
            // queue up c1 first
            IFuture<bool> c1f = c1.Queue();
            // now queue up c2 and wait on it
            bool receivedException = false;
            try
            {
                c2.Queue().Get();
            }
            catch (Exception)
            {
                // we expect to get an exception here
                receivedException = true;
            }

            if (!receivedException)
            {
                Assert.Fail("We expect to receive an exception for c2 as it's supposed to timeout.");
            }

            // c1 will complete after 100ms
            try
            {
                c1f.Get();
            }
            catch (Exception)
            {
                Assert.Fail("we should not have failed while getting c1");
            }
            Assert.IsTrue(c1.DidExecute, "c1 is expected to executed but didn't");

            // c2 will timeout after 20 ms ... we'll wait longer than the 200ms time to make sure
            // the thread doesn't keep running in the background and execute
            try
            {
                Thread.Sleep(400);
            }
            catch (Exception)
            {
                throw new Exception("Failed to sleep");
            }
            Assert.IsFalse(c2.DidExecute, "c2 is not expected to execute, but did");

            Assert.AreEqual(1, s1.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(0, s1.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(0, s1.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, s1.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, s1.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(0, s1.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, s1.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, s1.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, s1.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(0, s1.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, s1.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(0, s1.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(0, s2.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(1, s2.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(0, s2.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, s2.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, s2.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(0, s2.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, s2.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, s2.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, s2.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(1, s2.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, s2.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(100, s2.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(2, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());
            Hystrix.Reset();
        }

        [TestMethod]
        public void Command_FallbackSemaphore()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
            // single thread should work
            try
            {
                bool result = new TestSemaphoreCommandWithSlowFallback(circuitBreaker, 1, TimeSpan.FromMilliseconds(200)).Queue().Get();
                Assert.IsTrue(result);
            }
            catch (Exception e)
            {
                // we shouldn't fail on this one
                throw new Exception("Unexpected exception.", e);
            }

            // 2 threads, the second should be rejected by the fallback semaphore
            bool exceptionReceived = false;
            IFuture<bool> result2 = null;
            try
            {
                result2 = new TestSemaphoreCommandWithSlowFallback(circuitBreaker, 1, TimeSpan.FromMilliseconds(400)).Queue();
                // make sure that thread gets a chance to run before queuing the next one
                Thread.Sleep(50);
                IFuture<bool> result3 = new TestSemaphoreCommandWithSlowFallback(circuitBreaker, 1, TimeSpan.FromMilliseconds(200)).Queue();
                result3.Get();
            }
            catch (Exception)
            {

                exceptionReceived = true;
            }

            try
            {
                Assert.IsTrue(result2.Get());
            }
            catch (Exception e)
            {
                throw new Exception("Unexpected exception.", e);
            }

            if (!exceptionReceived)
            {
                Assert.Fail("We expected an exception on the 2nd get");
            }

            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            // TestSemaphoreCommandWithSlowFallback always fails so all 3 should show failure
            Assert.AreEqual(3, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            // the 1st thread executes single-threaded and gets a fallback, the next 2 are concurrent so only 1 of them is permitted by the fallback semaphore so 1 is rejected
            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            // whenever a fallback_rejection occurs it is also a fallback_failure
            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(2, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            // we should not have rejected any via the "execution semaphore" but instead via the "fallback semaphore"
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            // the rest should not be involved in this test
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(3, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());
            Hystrix.Reset();
        }

        [TestMethod]
        public void Command_ExecutionSemaphoreWithQueue()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
            // single thread should work
            try
            {
                bool result = new TestSemaphoreCommand(circuitBreaker, 1, TimeSpan.FromMilliseconds(200)).Queue().Get();
                Assert.IsTrue(result);
            }
            catch (Exception e)
            {
                // we shouldn't fail on this one
                throw new Exception("Unexpected exception.", e);
            }

            AtomicBoolean exceptionReceived = new AtomicBoolean();

            TryableSemaphore semaphore =
                new TryableSemaphore(HystrixPropertyFactory.AsProperty(1));

            IRunnable r = new HystrixContextRunnable(new Runnable(() =>
            {
                try
                {
                    new TestSemaphoreCommand(circuitBreaker, semaphore, TimeSpan.FromMilliseconds(200)).Queue().Get();
                }
                catch (Exception)
                {

                    exceptionReceived.Value = true;
                }
            }));
            // 2 threads, the second should be rejected by the semaphore
            Thread t1 = new Thread(r.Run);
            Thread t2 = new Thread(r.Run);

            t1.Start();
            t2.Start();
            try
            {
                t1.Join();
                t2.Join();
            }
            catch (Exception)
            {

                Assert.Fail("failed waiting on threads");
            }

            if (!exceptionReceived.Value)
            {
                Assert.Fail("We expected an exception on the 2nd get");
            }

            Assert.AreEqual(2, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            // we don't have a fallback so threw an exception when rejected
            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            // not a failure as the command never executed so can't fail
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            // no fallback failure as there isn't a fallback implemented
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            // we should have rejected via semaphore
            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            // the rest should not be involved in this test
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(3, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());
            Hystrix.Reset();
        }

        [TestMethod]
        public void Command_ExecutionSemaphoreWithExecution()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
            // single thread should work
            try
            {
                TestSemaphoreCommand command = new TestSemaphoreCommand(circuitBreaker, 1, TimeSpan.FromMilliseconds(200));
                bool result = command.Execute();
                Assert.IsFalse(command.IsExecutedInThread);
                Assert.IsTrue(result);
            }
            catch (Exception e)
            {
                // we shouldn't fail on this one
                throw new Exception("Unexpected exception.", e);
            }

            ArrayBlockingQueue<bool> results = new ArrayBlockingQueue<bool>(2);

            AtomicBoolean exceptionReceived = new AtomicBoolean();

            TryableSemaphore semaphore =
                new TryableSemaphore(HystrixPropertyFactory.AsProperty(1));

            IRunnable r = new HystrixContextRunnable(new Runnable(() =>
            {
                try
                {
                    results.Add(new TestSemaphoreCommand(circuitBreaker, semaphore, TimeSpan.FromMilliseconds(200)).Execute());
                }
                catch (Exception)
                {

                    exceptionReceived.Value = true;
                }
            }));

            // 2 threads, the second should be rejected by the semaphore
            Thread t1 = new Thread(r.Run);
            Thread t2 = new Thread(r.Run);

            t1.Start();
            t2.Start();
            try
            {
                t1.Join();
                t2.Join();
            }
            catch (Exception)
            {

                Assert.Fail("failed waiting on threads");
            }

            if (!exceptionReceived.Value)
            {
                Assert.Fail("We expected an exception on the 2nd get");
            }

            // only 1 value is expected as the other should have thrown an exception
            Assert.AreEqual(1, results.Count);
            // should contain only a true result
            Assert.IsTrue(results.Contains(true));
            Assert.IsFalse(results.Contains(false));

            Assert.AreEqual(2, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            // no failure ... we throw an exception because of rejection but the command does not fail execution
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            // there is no fallback implemented so no failure can occur on it
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            // we rejected via semaphore
            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            // the rest should not be involved in this test
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(3, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());
            Hystrix.Reset();
        }

        [TestMethod]
        public void Command_RejectedExecutionSemaphoreWithFallback()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
            ArrayBlockingQueue<bool> results = new ArrayBlockingQueue<bool>(2);

            AtomicBoolean exceptionReceived = new AtomicBoolean();

            IRunnable r = new HystrixContextRunnable(new Runnable(() =>
            {
                try
                {
                    results.Add(new TestSemaphoreCommandWithFallback(circuitBreaker, 1, TimeSpan.FromMilliseconds(200), false).Execute());
                }
                catch (Exception)
                {

                    exceptionReceived.Value = true;
                }
            }));

            // 2 threads, the second should be rejected by the semaphore and return fallback
            Thread t1 = new Thread(r.Run);
            Thread t2 = new Thread(r.Run);

            t1.Start();
            t2.Start();
            try
            {
                t1.Join();
                t2.Join();
            }
            catch (Exception)
            {

                Assert.Fail("failed waiting on threads");
            }

            if (exceptionReceived.Value)
            {
                Assert.Fail("We should have received a fallback response");
            }

            // both threads should have returned values
            Assert.AreEqual(2, results.Count);
            // should contain both a true and false result
            Assert.IsTrue(results.Contains(true));
            Assert.IsTrue(results.Contains(false));

            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            // the rest should not be involved in this test
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            TestContext.WriteLine("**** DONE");

            Assert.AreEqual(2, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());
            Hystrix.Reset();
        }

        /**
         * Tests that semaphores are counted separately for commands with unique keys
         */
        [TestMethod]
        public void Command_SemaphorePermitsInUse()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();

            // this semaphore will be shared across multiple command instances
            TryableSemaphore sharedSemaphore =
                new TryableSemaphore(HystrixPropertyFactory.AsProperty(3));

            // used to wait until all commands have started
            CountdownEvent startLatch = new CountdownEvent(sharedSemaphore.NumberOfPermits.Get() + 1);

            // used to signal that all command can finish
            CountdownEvent sharedLatch = new CountdownEvent(1);

            IRunnable sharedSemaphoreRunnable = new HystrixContextRunnable(new Runnable(() =>
            {
                try
                {
                    new LatchedSemaphoreCommand(circuitBreaker, sharedSemaphore, startLatch, sharedLatch).Execute();
                }
                catch (Exception)
                {

                }
            }));

            // creates group of threads each using command sharing a single semaphore

            // I create extra threads and commands so that I can verify that some of them fail to obtain a semaphore
            int sharedThreadCount = sharedSemaphore.NumberOfPermits.Get() * 2;
            Thread[] sharedSemaphoreThreads = new Thread[sharedThreadCount];
            for (int i = 0; i < sharedThreadCount; i++)
            {
                sharedSemaphoreThreads[i] = new Thread(sharedSemaphoreRunnable.Run);
            }

            // creates thread using isolated semaphore
            TryableSemaphore isolatedSemaphore =
                new TryableSemaphore(HystrixPropertyFactory.AsProperty(1));

            CountdownEvent isolatedLatch = new CountdownEvent(1);

            // tracks failures to obtain semaphores
            AtomicInteger failureCount = new AtomicInteger();

            Thread isolatedThread = new Thread(new HystrixContextRunnable(new Runnable(() =>
            {
                try
                {
                    new LatchedSemaphoreCommand(circuitBreaker, isolatedSemaphore, startLatch, isolatedLatch).Execute();
                }
                catch (Exception)
                {

                    failureCount.IncrementAndGet();
                }
            })).Run);

            // verifies no permits in use before starting threads
            Assert.AreEqual(0, sharedSemaphore.GetNumberOfPermitsUsed(), "wrong number of permits for shared semaphore");
            Assert.AreEqual(0, isolatedSemaphore.GetNumberOfPermitsUsed(), "wrong number of permits for isolated semaphore");

            for (int i = 0; i < sharedThreadCount; i++)
            {
                sharedSemaphoreThreads[i].Start();
            }
            isolatedThread.Start();

            // waits until all commands have started
            try
            {
                startLatch.Wait(TimeSpan.FromSeconds(1.0));
            }
            catch (ThreadInterruptedException e)
            {
                throw new Exception("Unexpected exception.", e);
            }

            // verifies that all semaphores are in use
            Assert.AreEqual(sharedSemaphore.NumberOfPermits.Get(), sharedSemaphore.GetNumberOfPermitsUsed(), "wrong number of permits for shared semaphore");
            Assert.AreEqual(isolatedSemaphore.NumberOfPermits.Get(), isolatedSemaphore.GetNumberOfPermitsUsed(), "wrong number of permits for isolated semaphore");

            // signals commands to finish
            sharedLatch.Signal();
            isolatedLatch.Signal();

            try
            {
                for (int i = 0; i < sharedThreadCount; i++)
                {
                    sharedSemaphoreThreads[i].Join();
                }
                isolatedThread.Join();
            }
            catch (Exception)
            {

                Assert.Fail("failed waiting on threads");
            }

            // verifies no permits in use after finishing threads
            Assert.AreEqual(0, sharedSemaphore.GetNumberOfPermitsUsed(), "wrong number of permits for shared semaphore");
            Assert.AreEqual(0, isolatedSemaphore.GetNumberOfPermitsUsed(), "wrong number of permits for isolated semaphore");

            // verifies that some executions failed
            int expectedFailures = sharedSemaphore.GetNumberOfPermitsUsed();
            Assert.AreEqual(expectedFailures, failureCount.Value, "failures expected but did not happen");
            Hystrix.Reset();
        }

        /**
         * Test that HystrixOwner can be passed in dynamically.
         */
        [TestMethod]
        public void Command_DynamicOwner()
        {
            try
            {
                TestHystrixCommand<bool> command = new DynamicOwnerTestCommand(CommandGroupForUnitTest.OwnerOne);
                Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
                Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
                Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
                Assert.AreEqual(true, command.Execute());
                Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
                Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
                Assert.AreEqual(1, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            }
            catch (Exception)
            {

                Assert.Fail("We received an exception.");
            }
            Hystrix.Reset();
        }

        /**
         * Test a successful command execution.
         */
        [TestMethod]
        public void Command_DynamicOwnerFails()
        {
            try
            {
                TestHystrixCommand<bool> command = new DynamicOwnerTestCommand(null);
                Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
                Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
                Assert.AreEqual(0, command.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
                Assert.AreEqual(true, command.Execute());
                Assert.Fail("we should have thrown an exception as we need an owner");
            }
            catch (Exception)
            {
                // success if we get here
            }
            Hystrix.Reset();
        }

        /**
         * Test that HystrixCommandKey can be passed in dynamically.
         */
        [TestMethod]
        public void Command_DynamicKey()
        {
            try
            {
                DynamicOwnerAndKeyTestCommand command1 = new DynamicOwnerAndKeyTestCommand(CommandGroupForUnitTest.OwnerOne, CommandKeyForUnitTest.KeyOne);
                Assert.AreEqual(true, command1.Execute());
                DynamicOwnerAndKeyTestCommand command2 = new DynamicOwnerAndKeyTestCommand(CommandGroupForUnitTest.OwnerOne, CommandKeyForUnitTest.KeyTwo);
                Assert.AreEqual(true, command2.Execute());

                // 2 different circuit breakers should be created
                Assert.AreNotSame(command1.CircuitBreaker, command2.CircuitBreaker);
            }
            catch (Exception)
            {

                Assert.Fail("We received an exception.");
            }
            Hystrix.Reset();
        }

        /**
         * Test Request scoped caching of commands so that a 2nd duplicate call doesn't execute but returns the previous Future
         */
        [TestMethod]
        public void Command_RequestCache1()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
            SuccessfulCacheableCommand command1 = new SuccessfulCacheableCommand(circuitBreaker, true, "A");
            SuccessfulCacheableCommand command2 = new SuccessfulCacheableCommand(circuitBreaker, true, "A");

            Assert.IsTrue(command1.IsCommandRunningInThread);

            IFuture<String> f1 = command1.Queue();
            IFuture<String> f2 = command2.Queue();

            try
            {
                Assert.AreEqual("A", f1.Get());
                Assert.AreEqual("A", f2.Get());
            }
            catch (Exception e)
            {
                throw new Exception("Unexpected exception.", e);
            }

            Assert.IsTrue(command1.Executed);
            // the second one should not have executed as it should have received the cached value instead
            Assert.IsFalse(command2.Executed);

            // the execution log for command1 should show a Success
            Assert.AreEqual(1, command1.ExecutionEvents.Count());
            Assert.IsTrue(command1.ExecutionEvents.Contains(HystrixEventType.Success));
            Assert.IsTrue(command1.ExecutionTimeInMilliseconds > -1);
            Assert.IsFalse(command1.IsResponseFromCache);

            // the execution log for command2 should show it came from cache
            Assert.AreEqual(2, command2.ExecutionEvents.Count()); // it will include the Success + ResponseFromCache
            Assert.IsTrue(command2.ExecutionEvents.Contains(HystrixEventType.Success));
            Assert.IsTrue(command2.ExecutionEvents.Contains(HystrixEventType.ResponseFromCache));
            Assert.IsTrue(command2.ExecutionTimeInMilliseconds == -1);
            Assert.IsTrue(command2.IsResponseFromCache);

            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(0, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(2, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());
            Hystrix.Reset();
        }

        /**
         * Test Request scoped caching doesn't prevent different ones from executing
         */
        [TestMethod]
        public void Command_RequestCache2()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
            SuccessfulCacheableCommand command1 = new SuccessfulCacheableCommand(circuitBreaker, true, "A");
            SuccessfulCacheableCommand command2 = new SuccessfulCacheableCommand(circuitBreaker, true, "B");

            Assert.IsTrue(command1.IsCommandRunningInThread);

            IFuture<String> f1 = command1.Queue();
            IFuture<String> f2 = command2.Queue();

            try
            {
                Assert.AreEqual("A", f1.Get());
                Assert.AreEqual("B", f2.Get());
            }
            catch (Exception e)
            {
                throw new Exception("Unexpected exception.", e);
            }

            Assert.IsTrue(command1.Executed);
            // both should execute as they are different
            Assert.IsTrue(command2.Executed);

            // the execution log for command1 should show a Success
            Assert.AreEqual(1, command1.ExecutionEvents.Count());
            Assert.IsTrue(command1.ExecutionEvents.Contains(HystrixEventType.Success));

            // the execution log for command2 should show a Success
            Assert.AreEqual(1, command2.ExecutionEvents.Count());
            Assert.IsTrue(command2.ExecutionEvents.Contains(HystrixEventType.Success));
            Assert.IsTrue(command2.ExecutionTimeInMilliseconds > -1);
            Assert.IsFalse(command2.IsResponseFromCache);

            Assert.AreEqual(2, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(0, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(2, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());
            Hystrix.Reset();
        }

        /**
         * Test Request scoped caching with a mixture of commands
         */
        [TestMethod]
        public void Command_RequestCache3()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
            SuccessfulCacheableCommand command1 = new SuccessfulCacheableCommand(circuitBreaker, true, "A");
            SuccessfulCacheableCommand command2 = new SuccessfulCacheableCommand(circuitBreaker, true, "B");
            SuccessfulCacheableCommand command3 = new SuccessfulCacheableCommand(circuitBreaker, true, "A");

            Assert.IsTrue(command1.IsCommandRunningInThread);

            IFuture<String> f1 = command1.Queue();
            IFuture<String> f2 = command2.Queue();
            IFuture<String> f3 = command3.Queue();

            try
            {
                Assert.AreEqual("A", f1.Get());
                Assert.AreEqual("B", f2.Get());
                Assert.AreEqual("A", f3.Get());
            }
            catch (Exception e)
            {
                throw new Exception("Unexpected exception.", e);
            }

            Assert.IsTrue(command1.Executed);
            // both should execute as they are different
            Assert.IsTrue(command2.Executed);
            // but the 3rd should come from cache
            Assert.IsFalse(command3.Executed);

            // the execution log for command1 should show a Success
            Assert.AreEqual(1, command1.ExecutionEvents.Count());
            Assert.IsTrue(command1.ExecutionEvents.Contains(HystrixEventType.Success));

            // the execution log for command2 should show a Success
            Assert.AreEqual(1, command2.ExecutionEvents.Count());
            Assert.IsTrue(command2.ExecutionEvents.Contains(HystrixEventType.Success));

            // the execution log for command3 should show it came from cache
            Assert.AreEqual(2, command3.ExecutionEvents.Count()); // it will include the Success + ResponseFromCache
            Assert.IsTrue(command3.ExecutionEvents.Contains(HystrixEventType.ResponseFromCache));
            Assert.IsTrue(command3.ExecutionTimeInMilliseconds == -1);
            Assert.IsTrue(command3.IsResponseFromCache);

            Assert.AreEqual(2, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(0, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(3, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());
            Hystrix.Reset();
        }

        /**
         * Test Request scoped caching of commands so that a 2nd duplicate call doesn't execute but returns the previous Future
         */
        [TestMethod]
        public void Command_RequestCacheWithSlowExecution()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
            SlowCacheableCommand command1 = new SlowCacheableCommand(circuitBreaker, "A", TimeSpan.FromMilliseconds(200));
            SlowCacheableCommand command2 = new SlowCacheableCommand(circuitBreaker, "A", TimeSpan.FromMilliseconds(100));
            SlowCacheableCommand command3 = new SlowCacheableCommand(circuitBreaker, "A", TimeSpan.FromMilliseconds(100));
            SlowCacheableCommand command4 = new SlowCacheableCommand(circuitBreaker, "A", TimeSpan.FromMilliseconds(100));

            IFuture<String> f1 = command1.Queue();
            IFuture<String> f2 = command2.Queue();
            IFuture<String> f3 = command3.Queue();
            IFuture<String> f4 = command4.Queue();

            try
            {
                Assert.AreEqual("A", f2.Get());
                Assert.AreEqual("A", f3.Get());
                Assert.AreEqual("A", f4.Get());

                Assert.AreEqual("A", f1.Get());
            }
            catch (Exception e)
            {
                throw new Exception("Unexpected exception.", e);
            }

            Assert.IsTrue(command1.Executed);
            // the second one should not have executed as it should have received the cached value instead
            Assert.IsFalse(command2.Executed);
            Assert.IsFalse(command3.Executed);
            Assert.IsFalse(command4.Executed);

            // the execution log for command1 should show a Success
            Assert.AreEqual(1, command1.ExecutionEvents.Count());
            Assert.IsTrue(command1.ExecutionEvents.Contains(HystrixEventType.Success));
            Assert.IsTrue(command1.ExecutionTimeInMilliseconds > -1);
            Assert.IsFalse(command1.IsResponseFromCache);

            // the execution log for command2 should show it came from cache
            Assert.AreEqual(2, command2.ExecutionEvents.Count()); // it will include the Success + ResponseFromCache
            Assert.IsTrue(command2.ExecutionEvents.Contains(HystrixEventType.Success));
            Assert.IsTrue(command2.ExecutionEvents.Contains(HystrixEventType.ResponseFromCache));
            Assert.IsTrue(command2.ExecutionTimeInMilliseconds == -1);
            Assert.IsTrue(command2.IsResponseFromCache);

            Assert.IsTrue(command3.IsResponseFromCache);
            Assert.IsTrue(command3.ExecutionTimeInMilliseconds == -1);
            Assert.IsTrue(command4.IsResponseFromCache);
            Assert.IsTrue(command4.ExecutionTimeInMilliseconds == -1);

            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(3, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(0, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(4, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());

            TestContext.WriteLine("HystrixRequestLog: " + HystrixRequestLog.GetCurrentRequest().GetExecutedCommandsAsString());
            Hystrix.Reset();
        }

        /**
         * Test Request scoped caching with a mixture of commands
         */
        [TestMethod]
        public void Command_NoRequestCache3()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
            SuccessfulCacheableCommand command1 = new SuccessfulCacheableCommand(circuitBreaker, false, "A");
            SuccessfulCacheableCommand command2 = new SuccessfulCacheableCommand(circuitBreaker, false, "B");
            SuccessfulCacheableCommand command3 = new SuccessfulCacheableCommand(circuitBreaker, false, "A");

            Assert.IsTrue(command1.IsCommandRunningInThread);

            IFuture<String> f1 = command1.Queue();
            IFuture<String> f2 = command2.Queue();
            IFuture<String> f3 = command3.Queue();

            try
            {
                Assert.AreEqual("A", f1.Get());
                Assert.AreEqual("B", f2.Get());
                Assert.AreEqual("A", f3.Get());
            }
            catch (Exception e)
            {
                throw new Exception("Unexpected exception.", e);
            }

            Assert.IsTrue(command1.Executed);
            // both should execute as they are different
            Assert.IsTrue(command2.Executed);
            // this should also execute since we disabled the cache
            Assert.IsTrue(command3.Executed);

            // the execution log for command1 should show a Success
            Assert.AreEqual(1, command1.ExecutionEvents.Count());
            Assert.IsTrue(command1.ExecutionEvents.Contains(HystrixEventType.Success));

            // the execution log for command2 should show a Success
            Assert.AreEqual(1, command2.ExecutionEvents.Count());
            Assert.IsTrue(command2.ExecutionEvents.Contains(HystrixEventType.Success));

            // the execution log for command3 should show a Success
            Assert.AreEqual(1, command3.ExecutionEvents.Count());
            Assert.IsTrue(command3.ExecutionEvents.Contains(HystrixEventType.Success));

            Assert.AreEqual(3, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(0, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(3, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());
            Hystrix.Reset();
        }

        /**
         * Test Request scoped caching with a mixture of commands
         */
        [TestMethod]
        public void Command_RequestCacheViaQueueSemaphore1()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
            SuccessfulCacheableCommandViaSemaphore command1 = new SuccessfulCacheableCommandViaSemaphore(circuitBreaker, true, "A");
            SuccessfulCacheableCommandViaSemaphore command2 = new SuccessfulCacheableCommandViaSemaphore(circuitBreaker, true, "B");
            SuccessfulCacheableCommandViaSemaphore command3 = new SuccessfulCacheableCommandViaSemaphore(circuitBreaker, true, "A");

            Assert.IsFalse(command1.IsCommandRunningInThread);

            IFuture<String> f1 = command1.Queue();
            IFuture<String> f2 = command2.Queue();
            IFuture<String> f3 = command3.Queue();

            try
            {
                Assert.AreEqual("A", f1.Get());
                Assert.AreEqual("B", f2.Get());
                Assert.AreEqual("A", f3.Get());
            }
            catch (Exception e)
            {
                throw new Exception("Unexpected exception.", e);
            }

            Assert.IsTrue(command1.Executed);
            // both should execute as they are different
            Assert.IsTrue(command2.Executed);
            // but the 3rd should come from cache
            Assert.IsFalse(command3.Executed);

            // the execution log for command1 should show a Success
            Assert.AreEqual(1, command1.ExecutionEvents.Count());
            Assert.IsTrue(command1.ExecutionEvents.Contains(HystrixEventType.Success));

            // the execution log for command2 should show a Success
            Assert.AreEqual(1, command2.ExecutionEvents.Count());
            Assert.IsTrue(command2.ExecutionEvents.Contains(HystrixEventType.Success));

            // the execution log for command3 should show it comes from cache
            Assert.AreEqual(2, command3.ExecutionEvents.Count()); // it will include the Success + ResponseFromCache
            Assert.IsTrue(command3.ExecutionEvents.Contains(HystrixEventType.Success));
            Assert.IsTrue(command3.ExecutionEvents.Contains(HystrixEventType.ResponseFromCache));

            Assert.IsTrue(command3.IsResponseFromCache);
            Assert.IsTrue(command3.ExecutionTimeInMilliseconds == -1);

            Assert.AreEqual(2, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(0, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(3, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());
            Hystrix.Reset();
        }

        /**
         * Test Request scoped caching with a mixture of commands
         */
        [TestMethod]
        public void Command_NoRequestCacheViaQueueSemaphore1()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
            SuccessfulCacheableCommandViaSemaphore command1 = new SuccessfulCacheableCommandViaSemaphore(circuitBreaker, false, "A");
            SuccessfulCacheableCommandViaSemaphore command2 = new SuccessfulCacheableCommandViaSemaphore(circuitBreaker, false, "B");
            SuccessfulCacheableCommandViaSemaphore command3 = new SuccessfulCacheableCommandViaSemaphore(circuitBreaker, false, "A");

            Assert.IsFalse(command1.IsCommandRunningInThread);

            IFuture<String> f1 = command1.Queue();
            IFuture<String> f2 = command2.Queue();
            IFuture<String> f3 = command3.Queue();

            try
            {
                Assert.AreEqual("A", f1.Get());
                Assert.AreEqual("B", f2.Get());
                Assert.AreEqual("A", f3.Get());
            }
            catch (Exception e)
            {
                throw new Exception("Unexpected exception.", e);
            }

            Assert.IsTrue(command1.Executed);
            // both should execute as they are different
            Assert.IsTrue(command2.Executed);
            // this should also execute because caching is disabled
            Assert.IsTrue(command3.Executed);

            // the execution log for command1 should show a Success
            Assert.AreEqual(1, command1.ExecutionEvents.Count());
            Assert.IsTrue(command1.ExecutionEvents.Contains(HystrixEventType.Success));

            // the execution log for command2 should show a Success
            Assert.AreEqual(1, command2.ExecutionEvents.Count());
            Assert.IsTrue(command2.ExecutionEvents.Contains(HystrixEventType.Success));

            // the execution log for command3 should show a Success
            Assert.AreEqual(1, command3.ExecutionEvents.Count());
            Assert.IsTrue(command3.ExecutionEvents.Contains(HystrixEventType.Success));

            Assert.AreEqual(3, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(0, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(3, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());
            Hystrix.Reset();
        }

        /**
         * Test Request scoped caching with a mixture of commands
         */
        [TestMethod]
        public void Command_RequestCacheViaExecuteSemaphore1()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
            SuccessfulCacheableCommandViaSemaphore command1 = new SuccessfulCacheableCommandViaSemaphore(circuitBreaker, true, "A");
            SuccessfulCacheableCommandViaSemaphore command2 = new SuccessfulCacheableCommandViaSemaphore(circuitBreaker, true, "B");
            SuccessfulCacheableCommandViaSemaphore command3 = new SuccessfulCacheableCommandViaSemaphore(circuitBreaker, true, "A");

            Assert.IsFalse(command1.IsCommandRunningInThread);

            String f1 = command1.Execute();
            String f2 = command2.Execute();
            String f3 = command3.Execute();

            Assert.AreEqual("A", f1);
            Assert.AreEqual("B", f2);
            Assert.AreEqual("A", f3);

            Assert.IsTrue(command1.Executed);
            // both should execute as they are different
            Assert.IsTrue(command2.Executed);
            // but the 3rd should come from cache
            Assert.IsFalse(command3.Executed);

            // the execution log for command1 should show a Success
            Assert.AreEqual(1, command1.ExecutionEvents.Count());
            Assert.IsTrue(command1.ExecutionEvents.Contains(HystrixEventType.Success));

            // the execution log for command2 should show a Success
            Assert.AreEqual(1, command2.ExecutionEvents.Count());
            Assert.IsTrue(command2.ExecutionEvents.Contains(HystrixEventType.Success));

            // the execution log for command3 should show it comes from cache
            Assert.AreEqual(2, command3.ExecutionEvents.Count()); // it will include the Success + ResponseFromCache
            Assert.IsTrue(command3.ExecutionEvents.Contains(HystrixEventType.ResponseFromCache));

            Assert.AreEqual(2, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(0, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(3, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());
            Hystrix.Reset();
        }

        /**
         * Test Request scoped caching with a mixture of commands
         */
        [TestMethod]
        public void Command_NoRequestCacheViaExecuteSemaphore1()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
            SuccessfulCacheableCommandViaSemaphore command1 = new SuccessfulCacheableCommandViaSemaphore(circuitBreaker, false, "A");
            SuccessfulCacheableCommandViaSemaphore command2 = new SuccessfulCacheableCommandViaSemaphore(circuitBreaker, false, "B");
            SuccessfulCacheableCommandViaSemaphore command3 = new SuccessfulCacheableCommandViaSemaphore(circuitBreaker, false, "A");

            Assert.IsFalse(command1.IsCommandRunningInThread);

            String f1 = command1.Execute();
            String f2 = command2.Execute();
            String f3 = command3.Execute();

            Assert.AreEqual("A", f1);
            Assert.AreEqual("B", f2);
            Assert.AreEqual("A", f3);

            Assert.IsTrue(command1.Executed);
            // both should execute as they are different
            Assert.IsTrue(command2.Executed);
            // this should also execute because caching is disabled
            Assert.IsTrue(command3.Executed);

            // the execution log for command1 should show a Success
            Assert.AreEqual(1, command1.ExecutionEvents.Count());
            Assert.IsTrue(command1.ExecutionEvents.Contains(HystrixEventType.Success));

            // the execution log for command2 should show a Success
            Assert.AreEqual(1, command2.ExecutionEvents.Count());
            Assert.IsTrue(command2.ExecutionEvents.Contains(HystrixEventType.Success));

            // the execution log for command3 should show a Success
            Assert.AreEqual(1, command3.ExecutionEvents.Count());
            Assert.IsTrue(command3.ExecutionEvents.Contains(HystrixEventType.Success));

            Assert.AreEqual(3, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(0, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(3, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());
            Hystrix.Reset();
        }

        [TestMethod]
        public void Command_RequestCacheOnTimeoutCausesNullPointerException()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
            // Expect it to time out - all results should be false
            Assert.IsFalse(new RequestCacheNullPointerExceptionCase(circuitBreaker).Execute());
            Assert.IsFalse(new RequestCacheNullPointerExceptionCase(circuitBreaker).Execute()); // return from cache #1
            Assert.IsFalse(new RequestCacheNullPointerExceptionCase(circuitBreaker).Execute()); // return from cache #2
            Thread.Sleep(500); // timeout on command is set to 200ms
            bool value = new RequestCacheNullPointerExceptionCase(circuitBreaker).Execute(); // return from cache #3
            Assert.IsFalse(value);
            RequestCacheNullPointerExceptionCase c = new RequestCacheNullPointerExceptionCase(circuitBreaker);
            IFuture<bool> f = c.Queue(); // return from cache #4
            // the bug is that we're getting a null Future back, rather than a Future that returns false
            Assert.IsNotNull(f);
            Assert.IsFalse(f.Get());

            Assert.IsTrue(c.IsResponseFromFallback);
            Assert.IsTrue(c.IsResponseTimedOut);
            Assert.IsFalse(c.IsFailedExecution);
            Assert.IsFalse(c.IsResponseShortCircuited);

            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(4, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(100, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(5, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());

            HystrixCommand[] executeCommands = HystrixRequestLog.GetCurrentRequest().ExecutedCommands.ToArray();

            TestContext.WriteLine(":executeCommands[0].getExecutionEvents()" + executeCommands[0].ExecutionEvents);
            Assert.AreEqual(2, executeCommands[0].ExecutionEvents.Count());
            Assert.IsTrue(executeCommands[0].ExecutionEvents.Contains(HystrixEventType.FallbackSuccess));
            Assert.IsTrue(executeCommands[0].ExecutionEvents.Contains(HystrixEventType.Timeout));
            Assert.IsTrue(executeCommands[0].ExecutionTimeInMilliseconds > -1);
            Assert.IsTrue(executeCommands[0].IsResponseTimedOut);
            Assert.IsTrue(executeCommands[0].IsResponseFromFallback);
            Assert.IsFalse(executeCommands[0].IsResponseFromCache);

            Assert.AreEqual(3, executeCommands[1].ExecutionEvents.Count()); // it will include FallbackSuccess/Timeout + ResponseFromCache
            Assert.IsTrue(executeCommands[1].ExecutionEvents.Contains(HystrixEventType.ResponseFromCache));
            Assert.IsTrue(executeCommands[1].ExecutionTimeInMilliseconds == -1);
            Assert.IsTrue(executeCommands[1].IsResponseFromCache);
            Assert.IsTrue(executeCommands[1].IsResponseTimedOut);
            Assert.IsTrue(executeCommands[1].IsResponseFromFallback);
            Hystrix.Reset();
        }

        [TestMethod]
        public void Command_RequestCacheOnTimeoutThrowsException()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
            RequestCacheTimeoutWithoutFallback r1 = new RequestCacheTimeoutWithoutFallback(circuitBreaker);
            try
            {
                r1.Execute();
                // we should have thrown an exception
                Assert.Fail("expected a timeout");
            }
            catch (HystrixRuntimeException)
            {
                Assert.IsTrue(r1.IsResponseTimedOut);
                // what we want
            }

            RequestCacheTimeoutWithoutFallback r2 = new RequestCacheTimeoutWithoutFallback(circuitBreaker);
            try
            {
                r2.Execute();
                // we should have thrown an exception
                Assert.Fail("expected a timeout");
            }
            catch (HystrixRuntimeException)
            {
                Assert.IsTrue(r2.IsResponseTimedOut);
                // what we want
            }

            RequestCacheTimeoutWithoutFallback r3 = new RequestCacheTimeoutWithoutFallback(circuitBreaker);
            IFuture<bool> f3 = r3.Queue();
            try
            {
                f3.Get();
                // we should have thrown an exception
                Assert.Fail("expected a timeout");
            }
            catch (ExecutionException)
            {

                Assert.IsTrue(r3.IsResponseTimedOut);
                // what we want
            }

            Thread.Sleep(500); // timeout on command is set to 200ms

            RequestCacheTimeoutWithoutFallback r4 = new RequestCacheTimeoutWithoutFallback(circuitBreaker);
            try
            {
                r4.Execute();
                // we should have thrown an exception
                Assert.Fail("expected a timeout");
            }
            catch (HystrixRuntimeException)
            {
                Assert.IsTrue(r4.IsResponseTimedOut);
                Assert.IsFalse(r4.IsResponseFromFallback);
                // what we want
            }

            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(3, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(100, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(4, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());
            Hystrix.Reset();
        }

        [TestMethod]
        public void Command_RequestCacheOnThreadRejectionThrowsException()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
            CountdownEvent completionLatch = new CountdownEvent(1);
            RequestCacheThreadRejectionWithoutFallback r1 = new RequestCacheThreadRejectionWithoutFallback(circuitBreaker, completionLatch);
            try
            {
                TestContext.WriteLine("r1: " + r1.Execute());
                // we should have thrown an exception
                Assert.Fail("expected a rejection");
            }
            catch (HystrixRuntimeException)
            {
                Assert.IsTrue(r1.IsResponseRejected);
                // what we want
            }

            RequestCacheThreadRejectionWithoutFallback r2 = new RequestCacheThreadRejectionWithoutFallback(circuitBreaker, completionLatch);
            try
            {
                TestContext.WriteLine("r2: " + r2.Execute());
                // we should have thrown an exception
                Assert.Fail("expected a rejection");
            }
            catch (HystrixRuntimeException)
            {
                //                
                Assert.IsTrue(r2.IsResponseRejected);
                // what we want
            }

            RequestCacheThreadRejectionWithoutFallback r3 = new RequestCacheThreadRejectionWithoutFallback(circuitBreaker, completionLatch);
            IFuture<bool> f3 = r3.Queue();
            try
            {
                TestContext.WriteLine("f3: " + f3.Get());
                // we should have thrown an exception
                Assert.Fail("expected a rejection");
            }
            catch (ExecutionException)
            {
                //                
                Assert.IsTrue(r3.IsResponseRejected);
                // what we want
            }

            // let the command finish (only 1 should actually be blocked on this do to the response cache)
            completionLatch.Signal();

            // then another after the command has completed
            RequestCacheThreadRejectionWithoutFallback r4 = new RequestCacheThreadRejectionWithoutFallback(circuitBreaker, completionLatch);
            try
            {
                TestContext.WriteLine("r4: " + r4.Execute());
                // we should have thrown an exception
                Assert.Fail("expected a rejection");
            }
            catch (HystrixRuntimeException)
            {
                //                
                Assert.IsTrue(r4.IsResponseRejected);
                Assert.IsFalse(r4.IsResponseFromFallback);
                // what we want
            }

            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(3, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(100, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(4, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());
            Hystrix.Reset();
        }

        /**
         * Test that we can do basic execution without a RequestVariable being initialized.
         */
        [TestMethod]
        public void Command_BasicExecutionWorksWithoutRequestVariable()
        {
            try
            {
                /* force the RequestVariable to not be initialized */
                HystrixRequestContext.ContextForCurrentThread = null;

                TestHystrixCommand<bool> command = new SuccessfulTestCommand();
                Assert.AreEqual(true, command.Execute());

                TestHystrixCommand<bool> command2 = new SuccessfulTestCommand();
                Assert.AreEqual(true, command2.Queue().Get());

                // we should be able to execute without a RequestVariable if ...
                // 1) We don't have a cacheKey
                // 2) We don't ask for the RequestLog
                // 3) We don't do collapsing

            }
            catch (Exception e)
            {

                Assert.Fail("We received an exception => " + e.Message);
            }

            Hystrix.Reset();
        }

        /**
         * Test that if we try and execute a command with a cacheKey without initializing RequestVariable that it gives an error.
         */
        [TestMethod]
        public void Command_CacheKeyExecutionRequiresRequestVariable()
        {
            try
            {
                /* force the RequestVariable to not be initialized */
                HystrixRequestContext.ContextForCurrentThread = null;

                TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();

                SuccessfulCacheableCommand command = new SuccessfulCacheableCommand(circuitBreaker, true, "one");
                Assert.AreEqual(true, command.Execute());

                SuccessfulCacheableCommand command2 = new SuccessfulCacheableCommand(circuitBreaker, true, "two");
                Assert.AreEqual(true, command2.Queue().Get());

                Assert.Fail("We expect an exception because cacheKey requires RequestVariable.");

            }
            catch (Exception)
            {

            }
            Hystrix.Reset();
        }

        /**
         * Test that a BadRequestException can be thrown and not count towards errors and bypasses fallback.
         */
        [TestMethod]
        public void Command_BadRequestExceptionViaExecuteInThread()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
            try
            {
                new BadRequestCommand(circuitBreaker, ExecutionIsolationStrategy.Thread).Execute();
                Assert.Fail("we expect to receive a " + typeof(HystrixBadRequestException).Name);
            }
            catch (HystrixBadRequestException)
            {
                // success

            }
            catch (Exception e)
            {

                Assert.Fail("We expect a " + typeof(HystrixBadRequestException).Name + " but got a " + e.GetType().Name);
            }

            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));

            Hystrix.Reset();
        }

        /**
         * Test that a BadRequestException can be thrown and not count towards errors and bypasses fallback.
         */
        [TestMethod]
        public void Command_BadRequestExceptionViaQueueInThread()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
            try
            {
                new BadRequestCommand(circuitBreaker, ExecutionIsolationStrategy.Thread).Queue().Get();
                Assert.Fail("we expect to receive a " + typeof(HystrixBadRequestException).Name);
            }
            catch (ExecutionException e)
            {

                if (e.InnerException is HystrixBadRequestException)
                {
                    // success
                }
                else
                {
                    Assert.Fail("We expect a " + typeof(HystrixBadRequestException).Name + " but got a " + e.GetType().Name);
                }
            }
            catch (Exception)
            {

                Assert.Fail();
            }

            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));

            Hystrix.Reset();
        }

        /**
         * Test that a BadRequestException can be thrown and not count towards errors and bypasses fallback.
         */
        [TestMethod]
        public void Command_BadRequestExceptionViaExecuteInSemaphore()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
            try
            {
                new BadRequestCommand(circuitBreaker, ExecutionIsolationStrategy.Semaphore).Execute();
                Assert.Fail("we expect to receive a " + typeof(HystrixBadRequestException).Name);
            }
            catch (HystrixBadRequestException)
            {
                // success

            }
            catch (Exception e)
            {

                Assert.Fail("We expect a " + typeof(HystrixBadRequestException).Name + " but got a " + e.GetType().Name);
            }

            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Hystrix.Reset();
        }

        /**
         * Test that a BadRequestException can be thrown and not count towards errors and bypasses fallback.
         */
        [TestMethod]
        public void Command_BadRequestExceptionViaQueueInSemaphore()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
            try
            {
                new BadRequestCommand(circuitBreaker, ExecutionIsolationStrategy.Semaphore).Queue().Get();
                Assert.Fail("we expect to receive a " + typeof(HystrixBadRequestException).Name);
            }
            catch (HystrixBadRequestException)
            {
                // success

            }
            catch (Exception e)
            {

                Assert.Fail("We expect a " + typeof(HystrixBadRequestException).Name + " but got a " + e.GetType().Name);
            }

            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Hystrix.Reset();
        }

        /**
         * Test a checked Exception being thrown
         */
        [TestMethod]
        public void Command_CheckedException()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
            CommandWithCheckedException command = new CommandWithCheckedException(circuitBreaker);
            try
            {
                command.Execute();
                Assert.Fail("we expect to receive a " + typeof(Exception).Name);
            }
            catch (Exception e)
            {
                Assert.AreEqual("simulated checked exception message", e.InnerException.Message);
            }

            Assert.AreEqual("simulated checked exception message", command.FailedExecutionException.Message);

            Assert.IsTrue(command.ExecutionTimeInMilliseconds > -1);
            Assert.IsTrue(command.IsFailedExecution);

            Assert.AreEqual(0, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(1, circuitBreaker.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));

            Hystrix.Reset();
        }

        /**
         * Execution hook on successful execution
         */
        [TestMethod]
        public void Command_ExecutionHookSuccessfulCommand()
        {
            /* test with Execute() */
            TestHystrixCommand<bool> command = new SuccessfulTestCommand();
            command.Execute();

            // the run() method should run as we're not short-circuited or rejected
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartRun);
            // we expect a successful response from run()
            Assert.IsNotNull(command.Builder.ExecutionHook.RunSuccessResponse);
            // we do not expect an exception
            Assert.IsNull(command.Builder.ExecutionHook.RunFailureException);

            // the fallback() method should not be run as we were successful
            Assert.AreEqual(0, command.Builder.ExecutionHook.StartFallback);
            // null since it didn't run
            Assert.IsNull(command.Builder.ExecutionHook.FallbackSuccessResponse);
            // null since it didn't run
            Assert.IsNull(command.Builder.ExecutionHook.FallbackFailureException);

            // the Execute() method was used
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartExecute);
            // we should have a response from Execute() since run() succeeded
            Assert.IsNotNull(command.Builder.ExecutionHook.EndExecuteSuccessResponse);
            // we should not have an exception since run() succeeded
            Assert.IsNull(command.Builder.ExecutionHook.EndExecuteFailureException);

            // thread execution
            Assert.AreEqual(1, command.Builder.ExecutionHook.ThreadStart);
            Assert.AreEqual(1, command.Builder.ExecutionHook.ThreadComplete);

            /* test with queue() */
            command = new SuccessfulTestCommand();
            try
            {
                command.Queue().Get();
            }
            catch (Exception e)
            {
                throw new Exception("Unexpected exception.", e);
            }

            // the run() method should run as we're not short-circuited or rejected
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartRun);
            // we expect a successful response from run()
            Assert.IsNotNull(command.Builder.ExecutionHook.RunSuccessResponse);
            // we do not expect an exception
            Assert.IsNull(command.Builder.ExecutionHook.RunFailureException);

            // the fallback() method should not be run as we were successful
            Assert.AreEqual(0, command.Builder.ExecutionHook.StartFallback);
            // null since it didn't run
            Assert.IsNull(command.Builder.ExecutionHook.FallbackSuccessResponse);
            // null since it didn't run
            Assert.IsNull(command.Builder.ExecutionHook.FallbackFailureException);

            // the queue() method was used
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartExecute);
            // we should have a response from queue() since run() succeeded
            Assert.IsNotNull(command.Builder.ExecutionHook.EndExecuteSuccessResponse);
            // we should not have an exception since run() succeeded
            Assert.IsNull(command.Builder.ExecutionHook.EndExecuteFailureException);

            // thread execution
            Assert.AreEqual(1, command.Builder.ExecutionHook.ThreadStart);
            Assert.AreEqual(1, command.Builder.ExecutionHook.ThreadComplete);
            Hystrix.Reset();
        }

        /**
         * Execution hook on successful execution with "fire and forget" approach
         */
        [TestMethod]
        public void Command_ExecutionHookSuccessfulCommandViaFireAndForget()
        {
            TestHystrixCommand<bool> command = new SuccessfulTestCommand();
            try
            {
                // do not block on "get()" ... fire this asynchronously
                command.Queue();
            }
            catch (Exception e)
            {
                throw new Exception("Unexpected exception.", e);
            }

            // wait for command to execute without calling get on the future
            while (!command.IsExecutionComplete)
            {
                try
                {
                    Thread.Sleep(10);
                }
                catch (ThreadInterruptedException)
                {
                    throw new Exception("interrupted");
                }
            }

            /*
             * All the hooks should still work even though we didn't call get() on the future
             */

            // the run() method should run as we're not short-circuited or rejected
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartRun);
            // we expect a successful response from run()
            Assert.IsNotNull(command.Builder.ExecutionHook.RunSuccessResponse);
            // we do not expect an exception
            Assert.IsNull(command.Builder.ExecutionHook.RunFailureException);

            // the fallback() method should not be run as we were successful
            Assert.AreEqual(0, command.Builder.ExecutionHook.StartFallback);
            // null since it didn't run
            Assert.IsNull(command.Builder.ExecutionHook.FallbackSuccessResponse);
            // null since it didn't run
            Assert.IsNull(command.Builder.ExecutionHook.FallbackFailureException);

            // the queue() method was used
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartExecute);
            // we should have a response from queue() since run() succeeded
            Assert.IsNotNull(command.Builder.ExecutionHook.EndExecuteSuccessResponse);
            // we should not have an exception since run() succeeded
            Assert.IsNull(command.Builder.ExecutionHook.EndExecuteFailureException);

            // thread execution
            Assert.AreEqual(1, command.Builder.ExecutionHook.ThreadStart);
            Assert.AreEqual(1, command.Builder.ExecutionHook.ThreadComplete);
            Hystrix.Reset();
        }

        /**
         * Execution hook on successful execution with multiple get() calls to Future
         */
        [TestMethod]
        public void Command_ExecutionHookSuccessfulCommandWithMultipleGetsOnFuture()
        {
            TestHystrixCommand<bool> command = new SuccessfulTestCommand();
            try
            {
                IFuture<bool> f = command.Queue();
                f.Get();
                f.Get();
                f.Get();
                f.Get();
            }
            catch (Exception e)
            {
                throw new Exception("Unexpected exception.", e);
            }

            /*
             * Despite multiple calls to get() we should only have 1 call to the hooks.
             */

            // the run() method should run as we're not short-circuited or rejected
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartRun);
            // we expect a successful response from run()
            Assert.IsNotNull(command.Builder.ExecutionHook.RunSuccessResponse);
            // we do not expect an exception
            Assert.IsNull(command.Builder.ExecutionHook.RunFailureException);

            // the fallback() method should not be run as we were successful
            Assert.AreEqual(0, command.Builder.ExecutionHook.StartFallback);
            // null since it didn't run
            Assert.IsNull(command.Builder.ExecutionHook.FallbackSuccessResponse);
            // null since it didn't run
            Assert.IsNull(command.Builder.ExecutionHook.FallbackFailureException);

            // the queue() method was used
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartExecute);
            // we should have a response from queue() since run() succeeded
            Assert.IsNotNull(command.Builder.ExecutionHook.EndExecuteSuccessResponse);
            // we should not have an exception since run() succeeded
            Assert.IsNull(command.Builder.ExecutionHook.EndExecuteFailureException);

            // thread execution
            Assert.AreEqual(1, command.Builder.ExecutionHook.ThreadStart);
            Assert.AreEqual(1, command.Builder.ExecutionHook.ThreadComplete);
            Hystrix.Reset();
        }

        /**
         * Execution hook on failed execution without a fallback
         */
        [TestMethod]
        public void Command_ExecutionHookRunFailureWithoutFallback()
        {
            /* test with Execute() */
            TestHystrixCommand<bool> command = new UnknownFailureTestCommandWithoutFallback();
            try
            {
                command.Execute();
                Assert.Fail("Expecting exception");
            }
            catch (Exception)
            {
                // ignore
            }

            // the run() method should run as we're not short-circuited or rejected
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartRun);
            // we should not have a response
            Assert.IsNull(command.Builder.ExecutionHook.RunSuccessResponse);
            // we should have an exception
            Assert.IsNotNull(command.Builder.ExecutionHook.RunFailureException);

            // the fallback() method should be run since run() failed
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartFallback);
            // no response since fallback is not implemented
            Assert.IsNull(command.Builder.ExecutionHook.FallbackSuccessResponse);
            // not null since it's not implemented and throws an exception
            Assert.IsNotNull(command.Builder.ExecutionHook.FallbackFailureException);

            // the Execute() method was used
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartExecute);
            // we should not have a response from Execute() since we do not have a fallback and run() failed
            Assert.IsNull(command.Builder.ExecutionHook.EndExecuteSuccessResponse);
            // we should have an exception since run() failed
            Assert.IsNotNull(command.Builder.ExecutionHook.EndExecuteFailureException);
            // run() failure
            Assert.AreEqual(FailureType.CommandException, command.Builder.ExecutionHook.EndExecuteFailureType);

            // thread execution
            Assert.AreEqual(1, command.Builder.ExecutionHook.ThreadStart);
            Assert.AreEqual(1, command.Builder.ExecutionHook.ThreadComplete);

            /* test with queue() */
            command = new UnknownFailureTestCommandWithoutFallback();
            try
            {
                command.Queue().Get();
                Assert.Fail("Expecting exception");
            }
            catch (Exception)
            {
                // ignore
            }

            // the run() method should run as we're not short-circuited or rejected
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartRun);
            // we should not have a response
            Assert.IsNull(command.Builder.ExecutionHook.RunSuccessResponse);
            // we should have an exception
            Assert.IsNotNull(command.Builder.ExecutionHook.RunFailureException);

            // the fallback() method should be run since run() failed
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartFallback);
            // no response since fallback is not implemented
            Assert.IsNull(command.Builder.ExecutionHook.FallbackSuccessResponse);
            // not null since it's not implemented and throws an exception
            Assert.IsNotNull(command.Builder.ExecutionHook.FallbackFailureException);

            // the queue() method was used
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartExecute);
            // we should not have a response from queue() since we do not have a fallback and run() failed
            Assert.IsNull(command.Builder.ExecutionHook.EndExecuteSuccessResponse);
            // we should have an exception since run() failed
            Assert.IsNotNull(command.Builder.ExecutionHook.EndExecuteFailureException);
            // run() failure
            Assert.AreEqual(FailureType.CommandException, command.Builder.ExecutionHook.EndExecuteFailureType);

            // thread execution
            Assert.AreEqual(1, command.Builder.ExecutionHook.ThreadStart);
            Assert.AreEqual(1, command.Builder.ExecutionHook.ThreadComplete);

            Hystrix.Reset();
        }

        /**
         * Execution hook on failed execution with a fallback
         */
        [TestMethod]
        public void Command_ExecutionHookRunFailureWithFallback()
        {
            /* test with Execute() */
            TestHystrixCommand<bool> command = new KnownFailureTestCommandWithFallback(new TestCircuitBreaker());
            command.Execute();

            // the run() method should run as we're not short-circuited or rejected
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartRun);
            // we should not have a response from run since run() failed
            Assert.IsNull(command.Builder.ExecutionHook.RunSuccessResponse);
            // we should have an exception since run() failed
            Assert.IsNotNull(command.Builder.ExecutionHook.RunFailureException);

            // the fallback() method should be run since run() failed
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartFallback);
            // a response since fallback is implemented
            Assert.IsNotNull(command.Builder.ExecutionHook.FallbackSuccessResponse);
            // null since it's implemented and succeeds
            Assert.IsNull(command.Builder.ExecutionHook.FallbackFailureException);

            // the Execute() method was used
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartExecute);
            // we should have a response from Execute() since we expect a fallback despite failure of run()
            Assert.IsNotNull(command.Builder.ExecutionHook.EndExecuteSuccessResponse);
            // we should not have an exception because we expect a fallback
            Assert.IsNull(command.Builder.ExecutionHook.EndExecuteFailureException);

            // thread execution
            Assert.AreEqual(1, command.Builder.ExecutionHook.ThreadStart);
            Assert.AreEqual(1, command.Builder.ExecutionHook.ThreadComplete);

            /* test with queue() */
            command = new KnownFailureTestCommandWithFallback(new TestCircuitBreaker());
            try
            {
                command.Queue().Get();
            }
            catch (Exception e)
            {
                throw new Exception("Unexpected exception.", e);
            }

            // the run() method should run as we're not short-circuited or rejected
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartRun);
            // we should not have a response from run since run() failed
            Assert.IsNull(command.Builder.ExecutionHook.RunSuccessResponse);
            // we should have an exception since run() failed
            Assert.IsNotNull(command.Builder.ExecutionHook.RunFailureException);

            // the fallback() method should be run since run() failed
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartFallback);
            // a response since fallback is implemented
            Assert.IsNotNull(command.Builder.ExecutionHook.FallbackSuccessResponse);
            // null since it's implemented and succeeds
            Assert.IsNull(command.Builder.ExecutionHook.FallbackFailureException);

            // the queue() method was used
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartExecute);
            // we should have a response from queue() since we expect a fallback despite failure of run()
            Assert.IsNotNull(command.Builder.ExecutionHook.EndExecuteSuccessResponse);
            // we should not have an exception because we expect a fallback
            Assert.IsNull(command.Builder.ExecutionHook.EndExecuteFailureException);

            // thread execution
            Assert.AreEqual(1, command.Builder.ExecutionHook.ThreadStart);
            Assert.AreEqual(1, command.Builder.ExecutionHook.ThreadComplete);
            Hystrix.Reset();
        }

        /**
         * Execution hook on failed execution with a fallback failure
         */
        [TestMethod]
        public void Command_ExecutionHookRunFailureWithFallbackFailure()
        {
            /* test with Execute() */
            TestHystrixCommand<bool> command = new KnownFailureTestCommandWithFallbackFailure();
            try
            {
                command.Execute();
                Assert.Fail("Expecting exception");
            }
            catch (Exception)
            {
                // ignore
            }

            // the run() method should run as we're not short-circuited or rejected
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartRun);
            // we should not have a response because run() and fallback fail
            Assert.IsNull(command.Builder.ExecutionHook.RunSuccessResponse);
            // we should have an exception because run() and fallback fail
            Assert.IsNotNull(command.Builder.ExecutionHook.RunFailureException);

            // the fallback() method should be run since run() failed
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartFallback);
            // no response since fallback fails
            Assert.IsNull(command.Builder.ExecutionHook.FallbackSuccessResponse);
            // not null since it's implemented but fails
            Assert.IsNotNull(command.Builder.ExecutionHook.FallbackFailureException);

            // the Execute() method was used
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartExecute);
            // we should not have a response because run() and fallback fail
            Assert.IsNull(command.Builder.ExecutionHook.EndExecuteSuccessResponse);
            // we should have an exception because run() and fallback fail
            Assert.IsNotNull(command.Builder.ExecutionHook.EndExecuteFailureException);
            // run() failure
            Assert.AreEqual(FailureType.CommandException, command.Builder.ExecutionHook.EndExecuteFailureType);

            // thread execution
            Assert.AreEqual(1, command.Builder.ExecutionHook.ThreadStart);
            Assert.AreEqual(1, command.Builder.ExecutionHook.ThreadComplete);

            /* test with queue() */
            command = new KnownFailureTestCommandWithFallbackFailure();
            try
            {
                command.Queue().Get();
                Assert.Fail("Expecting exception");
            }
            catch (Exception)
            {
                // ignore
            }

            // the run() method should run as we're not short-circuited or rejected
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartRun);
            // we should not have a response because run() and fallback fail
            Assert.IsNull(command.Builder.ExecutionHook.RunSuccessResponse);
            // we should have an exception because run() and fallback fail
            Assert.IsNotNull(command.Builder.ExecutionHook.RunFailureException);

            // the fallback() method should be run since run() failed
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartFallback);
            // no response since fallback fails
            Assert.IsNull(command.Builder.ExecutionHook.FallbackSuccessResponse);
            // not null since it's implemented but fails
            Assert.IsNotNull(command.Builder.ExecutionHook.FallbackFailureException);

            // the queue() method was used
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartExecute);
            // we should not have a response because run() and fallback fail
            Assert.IsNull(command.Builder.ExecutionHook.EndExecuteSuccessResponse);
            // we should have an exception because run() and fallback fail
            Assert.IsNotNull(command.Builder.ExecutionHook.EndExecuteFailureException);
            // run() failure
            Assert.AreEqual(FailureType.CommandException, command.Builder.ExecutionHook.EndExecuteFailureType);

            // thread execution
            Assert.AreEqual(1, command.Builder.ExecutionHook.ThreadStart);
            Assert.AreEqual(1, command.Builder.ExecutionHook.ThreadComplete);
            Hystrix.Reset();
        }

        /**
         * Execution hook on timeout without a fallback
         */
        [TestMethod]
        public void Command_ExecutionHookTimeoutWithoutFallback()
        {
            TestHystrixCommand<bool> command = new TestCommandWithTimeout(TimeSpan.FromMilliseconds(50.0), TestCommandWithTimeout.FallbackNotImplemented);
            try
            {
                command.Queue().Get();
                Assert.Fail("Expecting exception");
            }
            catch (Exception)
            {
                // ignore
            }

            // the run() method should run as we're not short-circuited or rejected
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartRun);
            // we should not have a response because of timeout and no fallback
            Assert.IsNull(command.Builder.ExecutionHook.RunSuccessResponse);
            // we should not have an exception because run() didn't fail, it timed out
            Assert.IsNull(command.Builder.ExecutionHook.RunFailureException);

            // the fallback() method should be run due to timeout
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartFallback);
            // no response since no fallback
            Assert.IsNull(command.Builder.ExecutionHook.FallbackSuccessResponse);
            // not null since no fallback implementation
            Assert.IsNotNull(command.Builder.ExecutionHook.FallbackFailureException);

            // execution occurred
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartExecute);
            // we should not have a response because of timeout and no fallback
            Assert.IsNull(command.Builder.ExecutionHook.EndExecuteSuccessResponse);
            // we should have an exception because of timeout and no fallback
            Assert.IsNotNull(command.Builder.ExecutionHook.EndExecuteFailureException);
            // timeout failure
            Assert.AreEqual(FailureType.Timeout, command.Builder.ExecutionHook.EndExecuteFailureType);

            // thread execution
            Assert.AreEqual(1, command.Builder.ExecutionHook.ThreadStart);

            // we need to wait for the thread to complete before the onThreadComplete hook will be called
            try
            {
                Thread.Sleep(400);
            }
            catch (ThreadInterruptedException)
            {
                // ignore
            }
            Assert.AreEqual(1, command.Builder.ExecutionHook.ThreadComplete);
            Hystrix.Reset();
        }

        /**
         * Execution hook on timeout with a fallback
         */
        [TestMethod]
        public void Command_ExecutionHookTimeoutWithFallback()
        {
            TestHystrixCommand<bool> command = new TestCommandWithTimeout(TimeSpan.FromMilliseconds(50.0), TestCommandWithTimeout.FallbackSuccess);
            try
            {
                command.Queue().Get();
            }
            catch (Exception e)
            {
                throw new Exception("not expecting", e);
            }

            // the run() method should run as we're not short-circuited or rejected
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartRun);
            // we should not have a response because of timeout
            Assert.IsNull(command.Builder.ExecutionHook.RunSuccessResponse);
            // we should not have an exception because run() didn't fail, it timed out
            Assert.IsNull(command.Builder.ExecutionHook.RunFailureException);

            // the fallback() method should be run due to timeout
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartFallback);
            // response since we have a fallback
            Assert.IsNotNull(command.Builder.ExecutionHook.FallbackSuccessResponse);
            // null since fallback succeeds
            Assert.IsNull(command.Builder.ExecutionHook.FallbackFailureException);

            // execution occurred
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartExecute);
            // we should have a response because of fallback
            Assert.IsNotNull(command.Builder.ExecutionHook.EndExecuteSuccessResponse);
            // we should not have an exception because of fallback
            Assert.IsNull(command.Builder.ExecutionHook.EndExecuteFailureException);

            // thread execution
            Assert.AreEqual(1, command.Builder.ExecutionHook.ThreadStart);

            // we need to wait for the thread to complete before the onThreadComplete hook will be called
            try
            {
                Thread.Sleep(400);
            }
            catch (ThreadInterruptedException)
            {
                // ignore
            }
            Assert.AreEqual(1, command.Builder.ExecutionHook.ThreadComplete);
            Hystrix.Reset();
        }

        /**
         * Execution hook on rejected with a fallback
         */
        [TestMethod]
        public void Command_ExecutionHookRejectedWithFallback()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
            SingleThreadedPool pool = new SingleThreadedPool(1);

            try
            {
                // fill the queue
                new TestCommandRejection(circuitBreaker, pool, TimeSpan.FromMilliseconds(500.0), TimeSpan.FromMilliseconds(600.0), TestCommandRejection.FallbackSuccess).Queue();
                new TestCommandRejection(circuitBreaker, pool, TimeSpan.FromMilliseconds(500.0), TimeSpan.FromMilliseconds(600.0), TestCommandRejection.FallbackSuccess).Queue();
            }
            catch (Exception)
            {
                // ignore
            }

            TestCommandRejection command = new TestCommandRejection(circuitBreaker, pool, TimeSpan.FromMilliseconds(500.0), TimeSpan.FromMilliseconds(600.0), TestCommandRejection.FallbackSuccess);
            try
            {
                // now execute one that will be rejected
                command.Queue().Get();
            }
            catch (Exception e)
            {
                throw new Exception("not expecting", e);
            }

            Assert.IsTrue(command.IsResponseRejected);

            // the run() method should not run as we're rejected
            Assert.AreEqual(0, command.Builder.ExecutionHook.StartRun);
            // we should not have a response because of rejection
            Assert.IsNull(command.Builder.ExecutionHook.RunSuccessResponse);
            // we should not have an exception because we didn't run
            Assert.IsNull(command.Builder.ExecutionHook.RunFailureException);

            // the fallback() method should be run due to rejection
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartFallback);
            // response since we have a fallback
            Assert.IsNotNull(command.Builder.ExecutionHook.FallbackSuccessResponse);
            // null since fallback succeeds
            Assert.IsNull(command.Builder.ExecutionHook.FallbackFailureException);

            // execution occurred
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartExecute);
            // we should have a response because of fallback
            Assert.IsNotNull(command.Builder.ExecutionHook.EndExecuteSuccessResponse);
            // we should not have an exception because of fallback
            Assert.IsNull(command.Builder.ExecutionHook.EndExecuteFailureException);

            // thread execution
            Assert.AreEqual(0, command.Builder.ExecutionHook.ThreadStart);
            Assert.AreEqual(0, command.Builder.ExecutionHook.ThreadComplete);
            Hystrix.Reset();
        }

        /**
         * Execution hook on short-circuit with a fallback
         */
        [TestMethod]
        public void Command_ExecutionHookShortCircuitedWithFallbackViaQueue()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker().SetForceShortCircuit(true);
            KnownFailureTestCommandWithoutFallback command = new KnownFailureTestCommandWithoutFallback(circuitBreaker);
            try
            {
                // now execute one that will be short-circuited
                command.Queue().Get();
                Assert.Fail("we expect an error as there is no fallback");
            }
            catch (Exception)
            {
                // expecting
            }

            Assert.IsTrue(command.IsResponseShortCircuited);

            // the run() method should not run as we're rejected
            Assert.AreEqual(0, command.Builder.ExecutionHook.StartRun);
            // we should not have a response because of rejection
            Assert.IsNull(command.Builder.ExecutionHook.RunSuccessResponse);
            // we should not have an exception because we didn't run
            Assert.IsNull(command.Builder.ExecutionHook.RunFailureException);

            // the fallback() method should be run due to rejection
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartFallback);
            // no response since we don't have a fallback
            Assert.IsNull(command.Builder.ExecutionHook.FallbackSuccessResponse);
            // not null since fallback fails and throws an exception
            Assert.IsNotNull(command.Builder.ExecutionHook.FallbackFailureException);

            // execution occurred
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartExecute);
            // we should not have a response because fallback fails
            Assert.IsNull(command.Builder.ExecutionHook.EndExecuteSuccessResponse);
            // we won't have an exception because short-circuit doesn't have one
            Assert.IsNull(command.Builder.ExecutionHook.EndExecuteFailureException);
            // but we do expect to receive a onError call with FailureType.Shortcircuit
            Assert.AreEqual(FailureType.Shortcircuit, command.Builder.ExecutionHook.EndExecuteFailureType);

            // thread execution
            Assert.AreEqual(0, command.Builder.ExecutionHook.ThreadStart);
            Assert.AreEqual(0, command.Builder.ExecutionHook.ThreadComplete);
            Hystrix.Reset();
        }

        /**
         * Execution hook on short-circuit with a fallback
         */
        [TestMethod]
        public void Command_ExecutionHookShortCircuitedWithFallbackViaExecute()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker().SetForceShortCircuit(true);
            KnownFailureTestCommandWithoutFallback command = new KnownFailureTestCommandWithoutFallback(circuitBreaker);
            try
            {
                // now execute one that will be short-circuited
                command.Execute();
                Assert.Fail("we expect an error as there is no fallback");
            }
            catch (Exception)
            {
                // expecting
            }

            Assert.IsTrue(command.IsResponseShortCircuited);

            // the run() method should not run as we're rejected
            Assert.AreEqual(0, command.Builder.ExecutionHook.StartRun);
            // we should not have a response because of rejection
            Assert.IsNull(command.Builder.ExecutionHook.RunSuccessResponse);
            // we should not have an exception because we didn't run
            Assert.IsNull(command.Builder.ExecutionHook.RunFailureException);

            // the fallback() method should be run due to rejection
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartFallback);
            // no response since we don't have a fallback
            Assert.IsNull(command.Builder.ExecutionHook.FallbackSuccessResponse);
            // not null since fallback fails and throws an exception
            Assert.IsNotNull(command.Builder.ExecutionHook.FallbackFailureException);

            // execution occurred
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartExecute);
            // we should not have a response because fallback fails
            Assert.IsNull(command.Builder.ExecutionHook.EndExecuteSuccessResponse);
            // we won't have an exception because short-circuit doesn't have one
            Assert.IsNull(command.Builder.ExecutionHook.EndExecuteFailureException);
            // but we do expect to receive a onError call with FailureType.Shortcircuit
            Assert.AreEqual(FailureType.Shortcircuit, command.Builder.ExecutionHook.EndExecuteFailureType);

            // thread execution
            Assert.AreEqual(0, command.Builder.ExecutionHook.ThreadStart);
            Assert.AreEqual(0, command.Builder.ExecutionHook.ThreadComplete);
            Hystrix.Reset();
        }

        /**
         * Execution hook on successful execution with semaphore isolation
         */
        [TestMethod]
        public void Command_ExecutionHookSuccessfulCommandWithSemaphoreIsolation()
        {
            /* test with Execute() */
            TestSemaphoreCommand command = new TestSemaphoreCommand(new TestCircuitBreaker(), 1, TimeSpan.FromMilliseconds(10));
            command.Execute();

            Assert.IsFalse(command.IsExecutedInThread);

            // the run() method should run as we're not short-circuited or rejected
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartRun);
            // we expect a successful response from run()
            Assert.IsNotNull(command.Builder.ExecutionHook.RunSuccessResponse);
            // we do not expect an exception
            Assert.IsNull(command.Builder.ExecutionHook.RunFailureException);

            // the fallback() method should not be run as we were successful
            Assert.AreEqual(0, command.Builder.ExecutionHook.StartFallback);
            // null since it didn't run
            Assert.IsNull(command.Builder.ExecutionHook.FallbackSuccessResponse);
            // null since it didn't run
            Assert.IsNull(command.Builder.ExecutionHook.FallbackFailureException);

            // the Execute() method was used
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartExecute);
            // we should have a response from Execute() since run() succeeded
            Assert.IsNotNull(command.Builder.ExecutionHook.EndExecuteSuccessResponse);
            // we should not have an exception since run() succeeded
            Assert.IsNull(command.Builder.ExecutionHook.EndExecuteFailureException);

            // thread execution
            Assert.AreEqual(0, command.Builder.ExecutionHook.ThreadStart);
            Assert.AreEqual(0, command.Builder.ExecutionHook.ThreadComplete);

            /* test with queue() */
            command = new TestSemaphoreCommand(new TestCircuitBreaker(), 1, TimeSpan.FromMilliseconds(10));
            try
            {
                command.Queue().Get();
            }
            catch (Exception e)
            {
                throw new Exception("Unexpected exception.", e);
            }

            Assert.IsFalse(command.IsExecutedInThread);

            // the run() method should run as we're not short-circuited or rejected
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartRun);
            // we expect a successful response from run()
            Assert.IsNotNull(command.Builder.ExecutionHook.RunSuccessResponse);
            // we do not expect an exception
            Assert.IsNull(command.Builder.ExecutionHook.RunFailureException);

            // the fallback() method should not be run as we were successful
            Assert.AreEqual(0, command.Builder.ExecutionHook.StartFallback);
            // null since it didn't run
            Assert.IsNull(command.Builder.ExecutionHook.FallbackSuccessResponse);
            // null since it didn't run
            Assert.IsNull(command.Builder.ExecutionHook.FallbackFailureException);

            // the queue() method was used
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartExecute);
            // we should have a response from queue() since run() succeeded
            Assert.IsNotNull(command.Builder.ExecutionHook.EndExecuteSuccessResponse);
            // we should not have an exception since run() succeeded
            Assert.IsNull(command.Builder.ExecutionHook.EndExecuteFailureException);

            // thread execution
            Assert.AreEqual(0, command.Builder.ExecutionHook.ThreadStart);
            Assert.AreEqual(0, command.Builder.ExecutionHook.ThreadComplete);
            Hystrix.Reset();
        }

        /**
         * Execution hook on successful execution with semaphore isolation
         */
        [TestMethod]
        public void Command_ExecutionHookFailureWithSemaphoreIsolation()
        {
            /* test with Execute() */
            TryableSemaphore semaphore =
                new TryableSemaphore(HystrixPropertyFactory.AsProperty(0));

            TestSemaphoreCommand command = new TestSemaphoreCommand(new TestCircuitBreaker(), semaphore, TimeSpan.FromMilliseconds(200));
            try
            {
                command.Execute();
                Assert.Fail("we expect a failure");
            }
            catch (Exception)
            {
                // expected
            }

            Assert.IsFalse(command.IsExecutedInThread);
            Assert.IsTrue(command.IsResponseRejected);

            // the run() method should not run as we are rejected
            Assert.AreEqual(0, command.Builder.ExecutionHook.StartRun);
            // null as run() does not get invoked
            Assert.IsNull(command.Builder.ExecutionHook.RunSuccessResponse);
            // null as run() does not get invoked
            Assert.IsNull(command.Builder.ExecutionHook.RunFailureException);

            // the fallback() method should run because of rejection
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartFallback);
            // null since there is no fallback
            Assert.IsNull(command.Builder.ExecutionHook.FallbackSuccessResponse);
            // not null since the fallback is not implemented
            Assert.IsNotNull(command.Builder.ExecutionHook.FallbackFailureException);

            // the Execute() method was used
            Assert.AreEqual(1, command.Builder.ExecutionHook.StartExecute);
            // we should not have a response since fallback has nothing
            Assert.IsNull(command.Builder.ExecutionHook.EndExecuteSuccessResponse);
            // we won't have an exception because rejection doesn't have one
            Assert.IsNull(command.Builder.ExecutionHook.EndExecuteFailureException);
            // but we do expect to receive a onError call with FailureType.Shortcircuit
            Assert.AreEqual(FailureType.RejectedSemaphoreExecution, command.Builder.ExecutionHook.EndExecuteFailureType);

            // thread execution
            Assert.AreEqual(0, command.Builder.ExecutionHook.ThreadStart);
            Assert.AreEqual(0, command.Builder.ExecutionHook.ThreadComplete);
            Hystrix.Reset();
        }

        /**
         * Test a command execution that fails but has a fallback.
         */
        [TestMethod]
        public void Command_ExecutionFailureWithFallbackImplementedButDisabled()
        {
            TestHystrixCommand<bool> commandEnabled = new KnownFailureTestCommandWithFallback(new TestCircuitBreaker(), true);
            try
            {
                Assert.AreEqual(false, commandEnabled.Execute());
            }
            catch (Exception)
            {

                Assert.Fail("We should have received a response from the fallback.");
            }

            TestHystrixCommand<bool> commandDisabled = new KnownFailureTestCommandWithFallback(new TestCircuitBreaker(), false);
            try
            {
                Assert.AreEqual(false, commandDisabled.Execute());
                Assert.Fail("expect exception thrown");
            }
            catch (Exception)
            {
                // expected
            }

            Assert.AreEqual("we failed with a simulated issue", commandDisabled.FailedExecutionException.Message);

            Assert.IsTrue(commandDisabled.IsFailedExecution);

            Assert.AreEqual(0, commandDisabled.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Success));
            Assert.AreEqual(1, commandDisabled.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown));
            Assert.AreEqual(1, commandDisabled.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Failure));
            Assert.AreEqual(0, commandDisabled.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection));
            Assert.AreEqual(0, commandDisabled.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure));
            Assert.AreEqual(0, commandDisabled.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess));
            Assert.AreEqual(0, commandDisabled.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected));
            Assert.AreEqual(0, commandDisabled.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited));
            Assert.AreEqual(0, commandDisabled.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected));
            Assert.AreEqual(0, commandDisabled.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.Timeout));
            Assert.AreEqual(0, commandDisabled.Builder.Metrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache));

            Assert.AreEqual(100, commandDisabled.Builder.Metrics.GetHealthCounts().ErrorPercentage);

            Assert.AreEqual(2, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());
            Hystrix.Reset();
        }
    }
}
