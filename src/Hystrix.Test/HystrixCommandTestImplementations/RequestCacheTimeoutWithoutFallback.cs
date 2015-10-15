namespace Hystrix.Test.HystrixCommandTestImplementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Hystrix.Test.CircuitBreakerTestImplementations;

    internal class RequestCacheTimeoutWithoutFallback : TestHystrixCommand<bool>
    {
        public RequestCacheTimeoutWithoutFallback(TestCircuitBreaker circuitBreaker)
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

        protected override string GetCacheKey()
        {
            return "A";
        }
    }
}
