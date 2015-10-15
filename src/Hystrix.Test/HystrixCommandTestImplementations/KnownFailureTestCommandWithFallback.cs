namespace Hystrix.Test.HystrixCommandTestImplementations
{
    using System;
    using Hystrix.Test.CircuitBreakerTestImplementations;

    /// <summary>
    /// Failed execution - fallback implementation successfully returns value.
    /// </summary>
    internal class KnownFailureTestCommandWithFallback : TestHystrixCommand<bool>
    {
        public KnownFailureTestCommandWithFallback(TestCircuitBreaker circuitBreaker)
            : base(new TestCommandBuilder()
            {
                CircuitBreaker = circuitBreaker,
                Metrics = circuitBreaker.Metrics
            })
        {
        }

        public KnownFailureTestCommandWithFallback(TestCircuitBreaker circuitBreaker, bool fallbackEnabled)
            : base(new TestCommandBuilder()
            {
                CircuitBreaker = circuitBreaker,
                Metrics = circuitBreaker.Metrics,
                CommandPropertiesDefaults = UnitTestSetterFactory.GetCommandPropertiesSetter().WithFallbackEnabled(fallbackEnabled)
            })
        {
        }

        protected override bool Run()
        {
            throw new Exception("we failed with a simulated issue");
        }

        protected override bool GetFallback()
        {
            return false;
        }
    }
}
