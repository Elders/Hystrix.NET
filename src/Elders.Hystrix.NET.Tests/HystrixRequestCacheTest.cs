namespace Elders.Hystrix.NET.Test
{
    using System;
    using Java.Util.Concurrent;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Elders.Hystrix.NET;
    using Elders.Hystrix.NET.Strategy.Concurrency;

    [TestClass]
    public class HystrixRequestCacheTest
    {
        [TestMethod]
        public void RequestCache_Cache()
        {
            IHystrixConcurrencyStrategy strategy = HystrixConcurrencyStrategyDefault.Instance;
            HystrixRequestContext context = HystrixRequestContext.InitializeContext();
            try
            {
                HystrixRequestCache cache1 = HystrixRequestCache.GetInstance("command1", strategy);
                cache1.PutIfAbsent("valueA", new TestFuture("a1"));
                cache1.PutIfAbsent("valueA", new TestFuture("a2"));
                cache1.PutIfAbsent("valueB", new TestFuture("b1"));

                HystrixRequestCache cache2 = HystrixRequestCache.GetInstance("command2", strategy);
                cache2.PutIfAbsent("valueA", new TestFuture("a3"));

                Assert.AreEqual("a1", cache1.Get<string>("valueA").Get());
                Assert.AreEqual("b1", cache1.Get<string>("valueB").Get());

                Assert.AreEqual("a3", cache2.Get<string>("valueA").Get());
                Assert.IsNull(cache2.Get<string>("valueB"));
            }
            catch (Exception e)
            {
                Assert.Fail("Exception: " + e.Message);
                Console.WriteLine(e.ToString());
            }
            finally
            {
                context.Shutdown();
            }

            context = HystrixRequestContext.InitializeContext();
            try
            {
                // with a new context  the instance should have nothing in it
                HystrixRequestCache cache = HystrixRequestCache.GetInstance("command1", strategy);
                Assert.IsNull(cache.Get<string>("valueA"));
                Assert.IsNull(cache.Get<string>("valueB"));
            }
            finally
            {
                context.Shutdown();
            }
        }

        [TestMethod]
        public void RequestCache_ClearCache()
        {
            IHystrixConcurrencyStrategy strategy = HystrixConcurrencyStrategyDefault.Instance;
            HystrixRequestContext context = HystrixRequestContext.InitializeContext();
            try
            {
                HystrixRequestCache cache1 = HystrixRequestCache.GetInstance("command1", strategy);
                cache1.PutIfAbsent("valueA", new TestFuture("a1"));
                Assert.AreEqual("a1", cache1.Get<string>("valueA").Get());
                cache1.Clear("valueA");
                Assert.IsNull(cache1.Get<string>("valueA"));
            }
            catch (Exception e)
            {
                Assert.Fail("Exception: " + e.Message);
                Console.WriteLine(e.ToString());
            }
            finally
            {
                context.Shutdown();
            }
        }

        private class TestFuture : IFuture<string>
        {
            private readonly string value;

            public TestFuture(string value)
            {
                this.value = value;
            }

            public bool IsCancelled
            {
                get { return false; }
            }
            public bool IsDone
            {
                get { return false; }
            }

            public string Get()
            {
                return this.value;
            }
            public string Get(TimeSpan timeout)
            {
                return this.value;
            }

            //object IFuture.Get()
            //{
            //    return Get();
            //}
            //object IFuture.Get(TimeSpan timeout)
            //{
            //    return Get(timeout);
            //}

            public bool Cancel(bool mayInterruptIfRunning)
            {
                return false;
            }
        }
    }
}
