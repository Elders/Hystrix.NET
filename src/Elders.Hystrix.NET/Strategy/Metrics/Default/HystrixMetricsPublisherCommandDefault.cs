namespace Elders.Hystrix.NET.Strategy.Metrics
{
    using Elders.Hystrix.NET.CircuitBreaker;

    public class HystrixMetricsPublisherCommandDefault : IHystrixMetricsPublisherCommand
    {
        public HystrixMetricsPublisherCommandDefault(HystrixCommandKey commandKey, HystrixCommandGroupKey commandGroupKey, HystrixCommandMetrics metrics, IHystrixCircuitBreaker circuitBreaker, IHystrixCommandProperties properties)
        {
            // do nothing by default
        }

        public void Initialize()
        {
            // do nothing by default
        }
    }
}
