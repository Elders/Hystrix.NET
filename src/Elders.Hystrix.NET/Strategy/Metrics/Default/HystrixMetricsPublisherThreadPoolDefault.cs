namespace Elders.Hystrix.NET.Strategy.Metrics
{
    using Elders.Hystrix.NET.ThreadPool;

    public class HystrixMetricsPublisherThreadPoolDefault : IHystrixMetricsPublisherThreadPool
    {
        public HystrixMetricsPublisherThreadPoolDefault(HystrixThreadPoolKey threadPoolKey, HystrixThreadPoolMetrics metrics, IHystrixThreadPoolProperties properties)
        {
            // do nothing by default
        }

        public void Initialize()
        {
            // do nothing by default
        }
    }
}
