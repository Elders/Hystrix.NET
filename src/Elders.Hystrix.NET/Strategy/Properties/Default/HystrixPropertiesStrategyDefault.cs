namespace Elders.Hystrix.NET.Strategy.Properties
{
    using Elders.Hystrix.NET.ThreadPool;

    public class HystrixPropertiesStrategyDefault : IHystrixPropertiesStrategy
    {
        private static readonly HystrixPropertiesStrategyDefault instance = new HystrixPropertiesStrategyDefault();
        public static HystrixPropertiesStrategyDefault Instance { get { return instance; } }

        protected HystrixPropertiesStrategyDefault()
        {
        }

        public virtual IHystrixCommandProperties GetCommandProperties(HystrixCommandKey commandKey, HystrixCommandPropertiesSetter setter)
        {
            return new HystrixPropertiesCommandDefault(setter);
        }
        public virtual string GetCommandPropertiesCacheKey(HystrixCommandKey commandKey, HystrixCommandPropertiesSetter setter)
        {
            return commandKey.Name;
        }

        public virtual IHystrixThreadPoolProperties GetThreadPoolProperties(HystrixThreadPoolKey threadPoolKey, HystrixThreadPoolPropertiesSetter setter)
        {
            return new HystrixThreadPoolPropertiesDefault(setter);
        }
        public virtual string GetThreadPoolPropertiesCacheKey(HystrixThreadPoolKey threadPoolKey, HystrixThreadPoolPropertiesSetter setter)
        {
            return threadPoolKey.Name;
        }

        public virtual IHystrixCollapserProperties GetCollapserProperties(IHystrixCollapserKey collapserKey, HystrixCollapserPropertiesSetter setter)
        {
            return new HystrixPropertiesCollapserDefault(setter);
        }
        public virtual string GetCollapserPropertiesCacheKey(IHystrixCollapserKey collapserKey, HystrixCollapserPropertiesSetter setter)
        {
            return collapserKey.Name;
        }
    }
}
