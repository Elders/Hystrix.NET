namespace Elders.Hystrix.NET.Test.HystrixCommandTestImplementations
{
    using System.Threading;
    using Elders.Hystrix.NET.Test.CircuitBreakerTestImplementations;

    internal class RequestCacheNullPointerExceptionCase : TestHystrixCommand<bool>
    {
        public RequestCacheNullPointerExceptionCase(TestCircuitBreaker circuitBreaker)
            : base(new TestCommandBuilder()
            {
                CircuitBreaker = circuitBreaker,
                Metrics = circuitBreaker.Metrics,
                CommandPropertiesDefaults = UnitTestSetterFactory.GetCommandPropertiesSetter().WithExecutionIsolationThreadTimeoutInMilliseconds(200),
            })
        {
            // we want it to timeout
        }

        protected override bool Run()
        {
            try
            {
                Thread.Sleep(500);
            }
            catch (ThreadInterruptedException)
            {
            }
            return true;
        }

        protected override bool GetFallback()
        {
            return false;
        }

        protected override string GetCacheKey()
        {
            return "A";
        }
    }
}
