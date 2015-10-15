namespace Hystrix.Test.Strategy.Metrics
{
    using System.Collections.Generic;
    using System.Threading;
    using Java.Util.Concurrent.Atomic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Netflix.Hystrix.CircuitBreaker;
    using Netflix.Hystrix.Strategy;
    using Netflix.Hystrix.Strategy.Metrics;
    using Netflix.Hystrix.ThreadPool;

    [TestClass]
    public class HystrixMetricPublisherFactoryTest
    {
        [TestMethod]
        public void MetricPublisherFactory_SingleInitializePerKey()
        {
            TestHystrixMetricsPublisher publisher = new TestHystrixMetricsPublisher();
            HystrixMetricsPublisherFactory factory = new HystrixMetricsPublisherFactory(publisher);
            List<Thread> threads = new List<Thread>();
            for (int i = 0; i < 20; i++)
            {
                threads.Add(new Thread(() =>
                {
                    factory.GetPublisherForCommand(CommandKeyForUnitTest.KeyOne, null, null, null, null);
                    factory.GetPublisherForCommand(CommandKeyForUnitTest.KeyTwo, null, null, null, null);
                    factory.GetPublisherForThreadPool(ThreadPoolKeyForUnitTest.ThreadPoolOne, null, null);
                }));
            }

            // start them
            foreach (Thread t in threads)
            {
                t.Start();
            }

            // wait for them to finish
            foreach (Thread t in threads)
            {
                t.Join();
            }

            // we should see 2 commands and 1 threadPool publisher created
            Assert.AreEqual(2, publisher.CommandCounter);
            Assert.AreEqual(1, publisher.ThreadCounter);
        }


        private class TestHystrixMetricsPublisher : IHystrixMetricsPublisher
        {
            private AtomicInteger commandCounter = new AtomicInteger();
            private AtomicInteger threadCounter = new AtomicInteger();

            public int CommandCounter { get { return this.commandCounter.Value; } }
            public int ThreadCounter { get { return this.threadCounter.Value; } }

            public IHystrixMetricsPublisherCommand GetMetricsPublisherForCommand(Netflix.Hystrix.HystrixCommandKey commandKey, Netflix.Hystrix.HystrixCommandGroupKey commandGroupKey, Netflix.Hystrix.HystrixCommandMetrics metrics, IHystrixCircuitBreaker circuitBreaker, Netflix.Hystrix.IHystrixCommandProperties properties)
            {
                return new HystrixDelegateMetricsPublisherCommand(() => this.commandCounter.IncrementAndGet());
            }

            public IHystrixMetricsPublisherThreadPool GetMetricsPublisherForThreadPool(HystrixThreadPoolKey threadPoolKey, HystrixThreadPoolMetrics metrics, IHystrixThreadPoolProperties properties)
            {
                return new HystrixDelegateMetricsPublisherThreadPool(() => this.threadCounter.IncrementAndGet());
            }
        }
    }
}
