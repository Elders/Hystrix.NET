namespace Netflix.Hystrix.Strategy
{
    using System;
    using System.Collections.Concurrent;
    using Netflix.Hystrix.Strategy.Properties;
    using Netflix.Hystrix.ThreadPool;

    public static class HystrixPropertiesFactory
    {
        private static readonly ConcurrentDictionary<string, IHystrixCommandProperties> commandProperties = new ConcurrentDictionary<string, IHystrixCommandProperties>();
        public static IHystrixCommandProperties GetCommandProperties(HystrixCommandKey commandKey, HystrixCommandPropertiesSetter setter)
        {
            if (commandKey == null)
                throw new ArgumentNullException("commandKey");

            IHystrixPropertiesStrategy strategy = HystrixPlugins.Instance.PropertiesStrategy;
            string cacheKey = strategy.GetCommandPropertiesCacheKey(commandKey, setter);
            if (String.IsNullOrEmpty(cacheKey))
            {
                return strategy.GetCommandProperties(commandKey, setter);
            }
            else
            {
                return commandProperties.GetOrAdd(cacheKey, w =>
                {
                    if (setter == null)
                    {
                        setter = new HystrixCommandPropertiesSetter();
                    }

                    return strategy.GetCommandProperties(commandKey, setter);
                });
            }
        }

        private static readonly ConcurrentDictionary<string, IHystrixThreadPoolProperties> threadPoolProperties = new ConcurrentDictionary<string, IHystrixThreadPoolProperties>();
        public static IHystrixThreadPoolProperties GetThreadPoolProperties(HystrixThreadPoolKey threadPoolKey, HystrixThreadPoolPropertiesSetter setter)
        {
            if (threadPoolKey == null)
                throw new ArgumentNullException("threadPoolKey");

            IHystrixPropertiesStrategy strategy = HystrixPlugins.Instance.PropertiesStrategy;
            string cacheKey = strategy.GetThreadPoolPropertiesCacheKey(threadPoolKey, setter);
            if (String.IsNullOrEmpty(cacheKey))
            {
                return strategy.GetThreadPoolProperties(threadPoolKey, setter);
            }
            else
            {
                return threadPoolProperties.GetOrAdd(cacheKey, w =>
                {
                    if (setter == null)
                    {
                        setter = new HystrixThreadPoolPropertiesSetter();
                    }

                    return strategy.GetThreadPoolProperties(threadPoolKey, setter);
                });
            }
        }

        private static readonly ConcurrentDictionary<string, IHystrixCollapserProperties> collapserProperties = new ConcurrentDictionary<string, IHystrixCollapserProperties>();
        public static IHystrixCollapserProperties GetCollapserProperties(IHystrixCollapserKey collapserKey, HystrixCollapserPropertiesSetter setter)
        {
            if (collapserKey == null)
                throw new ArgumentNullException("collapserKey");

            IHystrixPropertiesStrategy strategy = HystrixPlugins.Instance.PropertiesStrategy;
            string cacheKey = strategy.GetCollapserPropertiesCacheKey(collapserKey, setter);
            if (String.IsNullOrEmpty(cacheKey))
            {
                return strategy.GetCollapserProperties(collapserKey, setter);
            }
            else
            {
                return collapserProperties.GetOrAdd(cacheKey, w =>
                {
                    if (setter == null)
                    {
                        setter = new HystrixCollapserPropertiesSetter();
                    }

                    return strategy.GetCollapserProperties(collapserKey, setter);
                });
            }
        }
    }
}
