namespace Elders.Hystrix.NET.Strategy.Metrics
{
    using System;

    public class HystrixDelegateMetricsPublisherThreadPool : IHystrixMetricsPublisherThreadPool
    {
        private Action initialize;

        public HystrixDelegateMetricsPublisherThreadPool(Action initialize)
        {
            if (initialize == null)
                throw new ArgumentNullException("initialize");

            this.initialize = initialize;
        }

        public void Initialize()
        {
            this.initialize();
        }
    }
}
