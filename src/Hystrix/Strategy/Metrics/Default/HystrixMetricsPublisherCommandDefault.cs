namespace Netflix.Hystrix.Strategy.Metrics
{
    using Netflix.Hystrix.CircuitBreaker;

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
