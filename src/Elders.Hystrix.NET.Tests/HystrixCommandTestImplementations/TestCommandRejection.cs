namespace Elders.Hystrix.NET.Test.HystrixCommandTestImplementations
{
    using System;
    using System.Threading;
    using Elders.Hystrix.NET.Test.CircuitBreakerTestImplementations;
    using Elders.Hystrix.NET;
    using Elders.Hystrix.NET.ThreadPool;

    internal class TestCommandRejection : TestHystrixCommand<bool>
    {
        public const int FallbackNotImplemented = 1;
        public const int FallbackSuccess = 2;
        public const int FallbackFailure = 3;

        private readonly int fallbackBehavior;
        private readonly TimeSpan sleepTime;

        public TestCommandRejection(TestCircuitBreaker circuitBreaker, IHystrixThreadPool threadPool, TimeSpan sleepTime, TimeSpan timeout, int fallbackBehavior)
            : base(new TestCommandBuilder()
            {
                ThreadPool = threadPool,
                CircuitBreaker = circuitBreaker,
                Metrics = circuitBreaker.Metrics,
                CommandPropertiesDefaults = UnitTestSetterFactory.GetCommandPropertiesSetter().WithExecutionIsolationThreadTimeout(timeout)
            })
        {
            this.fallbackBehavior = fallbackBehavior;
            this.sleepTime = sleepTime;
        }

        protected override bool Run()
        {
            try
            {
                Thread.Sleep(this.sleepTime);
            }
            catch (ThreadInterruptedException)
            {
            }
            return true;
        }
        protected override bool GetFallback()
        {
            if (this.fallbackBehavior == FallbackSuccess)
            {
                return false;
            }
            else if (this.fallbackBehavior == FallbackFailure)
            {
                throw new Exception("failed on fallback");
            }
            else
            {
                // FALLBACK_NOT_IMPLEMENTED
                return base.GetFallback();
            }
        }
    }
}
