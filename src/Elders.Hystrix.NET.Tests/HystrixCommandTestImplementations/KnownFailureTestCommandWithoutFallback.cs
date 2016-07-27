namespace Elders.Hystrix.NET.Test.HystrixCommandTestImplementations
{
    using System;
    using Elders.Hystrix.NET.Test.CircuitBreakerTestImplementations;

    /// <summary>
    /// Failed execution with known exception (HystrixException) - no fallback implementation.
    /// </summary>
    internal class KnownFailureTestCommandWithoutFallback : TestHystrixCommand<bool>
    {
        public KnownFailureTestCommandWithoutFallback(TestCircuitBreaker circuitBreaker)
            : base(new TestCommandBuilder()
            {
                CircuitBreaker = circuitBreaker,
                Metrics = circuitBreaker.Metrics,
            })
        {
        }

        protected override bool Run()
        {
            throw new Exception("we failed with a simulated issue");
        }
    }
}
