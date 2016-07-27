namespace Elders.Hystrix.NET.Test.HystrixCommandTestImplementations
{
    using System.Threading;
    using Elders.Hystrix.NET.Test.CircuitBreakerTestImplementations;
    using Elders.Hystrix.NET;

    /// <summary>
    /// Semaphore based command that allows caller to use latches to know when it has started and signal when it would like the command to finish
    /// </summary>
    internal class LatchedSemaphoreCommand : TestHystrixCommand<bool>
    {
        private readonly CountdownEvent startLatch, waitLatch;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="circuitBreaker"></param>
        /// <param name="semaphore"></param>
        /// <param name="startLatch">this command calls {@link java.util.concurrent.CountDownLatch#countDown()} immediately upon running</param>
        /// <param name="waitLatch">this command calls {@link java.util.concurrent.CountDownLatch#await()} once it starts to run.  The caller can use the latch to signal the command to finish</param>
        public LatchedSemaphoreCommand(TestCircuitBreaker circuitBreaker, TryableSemaphore semaphore, CountdownEvent startLatch, CountdownEvent waitLatch)
            : base(new TestCommandBuilder()
            {
                CircuitBreaker = circuitBreaker,
                Metrics = circuitBreaker.Metrics,
                CommandPropertiesDefaults = UnitTestSetterFactory.GetCommandPropertiesSetter()
                    .WithExecutionIsolationStrategy(ExecutionIsolationStrategy.Semaphore),
                ExecutionSemaphore = semaphore,
            })
        {
            this.startLatch = startLatch;
            this.waitLatch = waitLatch;
        }

        protected override bool Run()
        {
            // signals caller that run has started
            this.startLatch.Signal();

            try
            {
                // waits for caller to countDown latch
                this.waitLatch.Wait();
            }
            catch (ThreadInterruptedException)
            {
                return false;
            }
            return true;
        }
    }
}
