namespace Netflix.Hystrix.Strategy.Metrics
{
    using Netflix.Hystrix.CircuitBreaker;
    using Netflix.Hystrix.ThreadPool;

    public interface IHystrixMetricsPublisher
    {
        IHystrixMetricsPublisherCommand GetMetricsPublisherForCommand(HystrixCommandKey commandKey, HystrixCommandGroupKey commandGroupKey, HystrixCommandMetrics metrics, IHystrixCircuitBreaker circuitBreaker, IHystrixCommandProperties properties);
        IHystrixMetricsPublisherThreadPool GetMetricsPublisherForThreadPool(HystrixThreadPoolKey threadPoolKey, HystrixThreadPoolMetrics metrics, IHystrixThreadPoolProperties properties);
    }
}
