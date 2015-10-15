namespace Netflix.Hystrix.Strategy
{
    using System.Collections.Concurrent;
    using Netflix.Hystrix.CircuitBreaker;
    using Netflix.Hystrix.Strategy.Metrics;
    using Netflix.Hystrix.ThreadPool;

    /// <summary>
    /// Factory for constructing metrics publisher implementations using a <see cref="IHystrixMetricsPublisher"/> implementation provided by <see cref="HystrixPlugins"/>.
    /// </summary>
    public class HystrixMetricsPublisherFactory
    {
        private static readonly HystrixMetricsPublisherFactory instance = new HystrixMetricsPublisherFactory();

        public static IHystrixMetricsPublisherThreadPool CreateOrRetrievePublisherForThreadPool(HystrixThreadPoolKey threadPoolKey, HystrixThreadPoolMetrics metrics, IHystrixThreadPoolProperties properties)
        {
            return instance.GetPublisherForThreadPool(threadPoolKey, metrics, properties);
        }
        public static IHystrixMetricsPublisherCommand CreateOrRetrievePublisherForCommand(HystrixCommandKey commandKey, HystrixCommandGroupKey commandOwner, HystrixCommandMetrics metrics, IHystrixCircuitBreaker circuitBreaker, IHystrixCommandProperties properties)
        {
            return instance.GetPublisherForCommand(commandKey, commandOwner, metrics, circuitBreaker, properties);
        }
        
        private readonly IHystrixMetricsPublisher strategy;

        internal HystrixMetricsPublisherFactory()
            : this(HystrixPlugins.Instance.MetricsPublisher)
        {
        }
        internal HystrixMetricsPublisherFactory(IHystrixMetricsPublisher strategy)
        {
            this.strategy = strategy;
        }

        private readonly ConcurrentDictionary<string, IHystrixMetricsPublisherCommand> commandPublishers = new ConcurrentDictionary<string, IHystrixMetricsPublisherCommand>();
        public IHystrixMetricsPublisherCommand GetPublisherForCommand(HystrixCommandKey commandKey, HystrixCommandGroupKey commandOwner, HystrixCommandMetrics metrics, IHystrixCircuitBreaker circuitBreaker, IHystrixCommandProperties properties)
        {
            return this.commandPublishers.GetOrAdd(commandKey.Name,
                w => this.strategy.GetMetricsPublisherForCommand(commandKey, commandOwner, metrics, circuitBreaker, properties),
                w => w.Initialize());
        }

        private readonly ConcurrentDictionary<string, IHystrixMetricsPublisherThreadPool> threadPoolPublishers = new ConcurrentDictionary<string, IHystrixMetricsPublisherThreadPool>();
        public IHystrixMetricsPublisherThreadPool GetPublisherForThreadPool(HystrixThreadPoolKey threadPoolKey, HystrixThreadPoolMetrics metrics, IHystrixThreadPoolProperties properties)
        {
            return this.threadPoolPublishers.GetOrAdd(threadPoolKey.Name,
                w => this.strategy.GetMetricsPublisherForThreadPool(threadPoolKey, metrics, properties),
                w => w.Initialize());
        }
    }
}
