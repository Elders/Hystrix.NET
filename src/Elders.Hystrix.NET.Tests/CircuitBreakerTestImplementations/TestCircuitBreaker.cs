namespace Elders.Hystrix.NET.Test.CircuitBreakerTestImplementations
{
    using Elders.Hystrix.NET.Test.HystrixCommandTestImplementations;
    using Elders.Hystrix.NET;
    using Elders.Hystrix.NET.CircuitBreaker;
    using Elders.Hystrix.NET.Util;

    internal class TestCircuitBreaker : IHystrixCircuitBreaker
    {
        private readonly HystrixCommandMetrics metrics;
        private bool forceShortCircuit = false;

        public HystrixCommandMetrics Metrics { get { return this.metrics; } }

        public TestCircuitBreaker()
        {
            this.metrics = HystrixCircuitBreakerTest.GetMetrics(UnitTestSetterFactory.GetCommandPropertiesSetter());
            forceShortCircuit = false;
        }

        public TestCircuitBreaker SetForceShortCircuit(bool value)
        {
            this.forceShortCircuit = value;
            return this;
        }

        public bool IsOpen()
        {
            if (this.forceShortCircuit)
            {
                return true;
            }
            else
            {
                return metrics.GetCumulativeCount(HystrixRollingNumberEvent.Failure) >= 3;
            }
        }

        public void MarkSuccess()
        {
            // we don't need to do anything since we're going to permanently trip the circuit
        }

        public bool AllowRequest()
        {
            return !IsOpen();
        }
    }
}
