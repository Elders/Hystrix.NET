namespace Elders.Hystrix.NET.Test
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Elders.Hystrix.NET;
    using Elders.Hystrix.NET.ThreadPool;

    [TestClass]
    public class HystrixThreadPoolTest
    {
        [TestMethod]
        public void ThreadPool_Shutdown()
        {
            // other unit tests will probably have run before this so get the count
            int count = HystrixThreadPoolFactory.ThreadPoolCount;

            IHystrixThreadPool pool = HystrixThreadPoolFactory.GetInstance("threadPoolFactoryTest",
                    UnitTestSetterFactory.GetThreadPoolPropertiesSetter());

            Assert.AreEqual(count + 1, HystrixThreadPoolFactory.ThreadPoolCount);
            Assert.IsFalse(pool.Executor.IsShutdown);

            HystrixThreadPoolFactory.Shutdown();

            // ensure all pools were removed from the cache
            Assert.AreEqual(0, HystrixThreadPoolFactory.ThreadPoolCount);
            Assert.IsTrue(pool.Executor.IsShutdown);
        }

        [TestMethod]
        public void ThreadPool_ShutdownWithWait()
        {
            // other unit tests will probably have run before this so get the count
            int count = HystrixThreadPoolFactory.ThreadPoolCount;

            IHystrixThreadPool pool = HystrixThreadPoolFactory.GetInstance("threadPoolFactoryTest",
                    UnitTestSetterFactory.GetThreadPoolPropertiesSetter());

            Assert.AreEqual(count + 1, HystrixThreadPoolFactory.ThreadPoolCount);
            Assert.IsFalse(pool.Executor.IsShutdown);

            HystrixThreadPoolFactory.Shutdown(TimeSpan.FromSeconds(1.0));

            // ensure all pools were removed from the cache
            Assert.AreEqual(0, HystrixThreadPoolFactory.ThreadPoolCount);
            Assert.IsTrue(pool.Executor.IsShutdown);
        }
    }
}
