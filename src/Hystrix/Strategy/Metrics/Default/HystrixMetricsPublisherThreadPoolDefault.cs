namespace Netflix.Hystrix.Strategy.Metrics
{
    using Netflix.Hystrix.ThreadPool;

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
