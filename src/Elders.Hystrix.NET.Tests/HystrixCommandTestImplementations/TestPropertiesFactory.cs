namespace Elders.Hystrix.NET.Test.HystrixCommandTestImplementations
{
    using System;
    using Elders.Hystrix.NET;
    using Elders.Hystrix.NET.Strategy.Properties;
    using Elders.Hystrix.NET.ThreadPool;

    internal class TestPropertiesFactory : IHystrixPropertiesStrategy
    {
        private static readonly TestPropertiesFactory instance = new TestPropertiesFactory();
        public static TestPropertiesFactory Instance { get { return instance; } }

        public IHystrixCommandProperties GetCommandProperties(HystrixCommandKey commandKey, HystrixCommandPropertiesSetter setter)
        {
            if (setter == null)
            {
                setter = UnitTestSetterFactory.GetCommandPropertiesSetter();
            }
            return new MockingHystrixCommandProperties(setter);
        }
        public string GetCommandPropertiesCacheKey(HystrixCommandKey commandKey, HystrixCommandPropertiesSetter setter)
        {
            return null;
        }

        public IHystrixThreadPoolProperties GetThreadPoolProperties(HystrixThreadPoolKey threadPoolKey, HystrixThreadPoolPropertiesSetter setter)
        {
            if (setter == null)
            {
                setter = UnitTestSetterFactory.GetThreadPoolPropertiesSetter();
            }
            return new MockingHystrixThreadPoolProperties(setter);
        }
        public string GetThreadPoolPropertiesCacheKey(HystrixThreadPoolKey threadPoolKey, HystrixThreadPoolPropertiesSetter setter)
        {
            return null;
        }

        public IHystrixCollapserProperties GetCollapserProperties(IHystrixCollapserKey collapserKey, HystrixCollapserPropertiesSetter setter)
        {
            throw new InvalidOperationException();
        }
        public string GetCollapserPropertiesCacheKey(IHystrixCollapserKey collapserKey, HystrixCollapserPropertiesSetter setter)
        {
            return null;
        }
    }
}
