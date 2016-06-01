using System;
using Netflix.Hystrix.CircuitBreaker;
using Netflix.Hystrix.ThreadPool;

namespace Netflix.Hystrix.Strategy.Metrics
{
    public interface IHystrixMetricsPublisher : IDisposable
    {
        IHystrixMetricsPublisherCommand GetMetricsPublisherForCommand(HystrixCommandKey commandKey, HystrixCommandGroupKey commandGroupKey, HystrixCommandMetrics metrics, IHystrixCircuitBreaker circuitBreaker, IHystrixCommandProperties properties);
        IHystrixMetricsPublisherThreadPool GetMetricsPublisherForThreadPool(HystrixThreadPoolKey threadPoolKey, HystrixThreadPoolMetrics metrics, IHystrixThreadPoolProperties properties);
    }
}
