namespace Elders.Hystrix.NET.Test.HystrixCommandTestImplementations
{
    using System;
    using System.Threading;
    using Elders.Hystrix.NET.Test.CircuitBreakerTestImplementations;
    using Elders.Hystrix.NET;

    /// <summary>
    /// The run() will take time. No fallback implementation.
    /// </summary>
    internal class TestSemaphoreCommand : TestHystrixCommand<bool>
    {
        private readonly TimeSpan executionSleep;

        public TestSemaphoreCommand(TestCircuitBreaker circuitBreaker, int executionSemaphoreCount, TimeSpan executionSleep)
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
        }
        public TestSemaphoreCommand(TestCircuitBreaker circuitBreaker, TryableSemaphore semaphore, TimeSpan executionSleep)
            : base(new TestCommandBuilder()
            {
                CircuitBreaker = circuitBreaker,
                Metrics = circuitBreaker.Metrics,
                CommandPropertiesDefaults = UnitTestSetterFactory.GetCommandPropertiesSetter()
                    .WithExecutionIsolationStrategy(ExecutionIsolationStrategy.Semaphore),
                ExecutionSemaphore = semaphore,
            })
        {
            this.executionSleep = executionSleep;
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
    }
}
