namespace Hystrix.Test.HystrixCommandTestImplementations
{
    using System;
    using System.Threading;
    using Hystrix.Test.CircuitBreakerTestImplementations;
    using Netflix.Hystrix;

    /// <summary>
    /// The run() will take time. Contains fallback.
    /// </summary>
    internal class TestSemaphoreCommandWithFallback : TestHystrixCommand<bool>
    {
        private readonly TimeSpan executionSleep;
        private readonly bool fallback;

        public TestSemaphoreCommandWithFallback(TestCircuitBreaker circuitBreaker, int executionSemaphoreCount, TimeSpan executionSleep, bool fallback)
            : base(new TestCommandBuilder()
            {
                CircuitBreaker = circuitBreaker,
                Metrics = circuitBreaker.Metrics,
                CommandPropertiesDefaults = UnitTestSetterFactory.GetCommandPropertiesSetter()
                    .WithExecutionIsolationStrategy(ExecutionIsolationStrategy.Semaphore)
                    .WithExecutionIsolationSemaphoreMaxConcurrentRequests(executionSemaphoreCount),
            })
        {
            this.executionSleep = executionSleep;
            this.fallback = fallback;
        }

        protected override bool Run()
        {
            try
            {
                Thread.Sleep(this.executionSleep);
            }
            catch (ThreadInterruptedException)
            {
            }
            return true;
        }
        protected override bool GetFallback()
        {
            return this.fallback;
        }
    }
}
