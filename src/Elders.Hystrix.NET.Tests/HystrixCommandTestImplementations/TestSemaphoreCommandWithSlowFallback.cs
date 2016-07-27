namespace Elders.Hystrix.NET.Test.HystrixCommandTestImplementations
{
    using System;
    using System.Threading;
    using Elders.Hystrix.NET.Test.CircuitBreakerTestImplementations;

    /// <summary>
    /// The Run() will fail and GetFallback() take a long time.
    /// </summary>
    internal class TestSemaphoreCommandWithSlowFallback : TestHystrixCommand<bool>
    {
        private readonly TimeSpan fallbackSleep;

        public TestSemaphoreCommandWithSlowFallback(TestCircuitBreaker circuitBreaker, int fallbackSemaphoreExecutionCount, TimeSpan fallbackSleep)
            : base(new TestCommandBuilder()
            {
                CircuitBreaker = circuitBreaker,
                Metrics = circuitBreaker.Metrics,
                CommandPropertiesDefaults = UnitTestSetterFactory.GetCommandPropertiesSetter().WithFallbackIsolationSemaphoreMaxConcurrentRequests(fallbackSemaphoreExecutionCount)
            })
        {
            this.fallbackSleep = fallbackSleep;
        }

        protected override bool Run()
        {
            throw new Exception("run fails");
        }

        protected override bool GetFallback()
        {
            try
            {
                Thread.Sleep(this.fallbackSleep);
            }
            catch (ThreadInterruptedException)
            {
            }
            return true;
        }
    }
}
