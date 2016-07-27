using System;
using Elders.Hystrix.NET.CircuitBreaker;
using Elders.Hystrix.NET.ThreadPool;

namespace Elders.Hystrix.NET.Strategy.Metrics
{
    public interface IHystrixMetricsPublisher : IDisposable
    {
        IHystrixMetricsPublisherCommand GetMetricsPublisherForCommand(HystrixCommandKey commandKey, HystrixCommandGroupKey commandGroupKey, HystrixCommandMetrics metrics, IHystrixCircuitBreaker circuitBreaker, IHystrixCommandProperties properties);
        IHystrixMetricsPublisherThreadPool GetMetricsPublisherForThreadPool(HystrixThreadPoolKey threadPoolKey, HystrixThreadPoolMetrics metrics, IHystrixThreadPoolProperties properties);
    }
}
