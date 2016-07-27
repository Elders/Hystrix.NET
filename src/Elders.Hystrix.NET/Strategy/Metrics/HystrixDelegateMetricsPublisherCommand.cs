namespace Elders.Hystrix.NET.Strategy.Metrics
{
    using System;

    public class HystrixDelegateMetricsPublisherCommand : IHystrixMetricsPublisherCommand
    {
        private Action initialize;

        public HystrixDelegateMetricsPublisherCommand(Action initialize)
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
