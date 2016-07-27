namespace Elders.Hystrix.NET.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Elders.Hystrix.NET;
    using Elders.Hystrix.NET.Strategy.EventNotifier;

    [TestClass]
    public class HystrixCommandMetricsTest
    {
        /**
         * Testing the ErrorPercentage because this method could be easy to miss when making changes elsewhere.
         */
        [TestMethod]
        public void CommandMetrics_GetErrorPercentage()
        {
            HystrixCommandPropertiesSetter properties = UnitTestSetterFactory.GetCommandPropertiesSetter();
            HystrixCommandMetrics metrics = GetMetrics(properties);

            metrics.MarkSuccess(100);
            Assert.AreEqual(0, metrics.GetHealthCounts().ErrorPercentage);

            metrics.MarkFailure(1000);
            Assert.AreEqual(50, metrics.GetHealthCounts().ErrorPercentage);

            metrics.MarkSuccess(100);
            metrics.MarkSuccess(100);
            Assert.AreEqual(25, metrics.GetHealthCounts().ErrorPercentage);

            metrics.MarkTimeout(5000);
            metrics.MarkTimeout(5000);
            Assert.AreEqual(50, metrics.GetHealthCounts().ErrorPercentage);

            metrics.MarkSuccess(100);
            metrics.MarkSuccess(100);
            metrics.MarkSuccess(100);

            // latent
            metrics.MarkSuccess(5000);

            // 6 success + 1 latent success + 1 failure + 2 timeout = 10 total
            // latent success not considered error
            // error percentage = 1 failure + 2 timeout / 10
            Assert.AreEqual(30, metrics.GetHealthCounts().ErrorPercentage);
        }

        /**
         * Utility method for creating {@link HystrixCommandMetrics} for unit tests.
         */
        private static HystrixCommandMetrics GetMetrics(HystrixCommandPropertiesSetter properties)
        {
            return new HystrixCommandMetrics(CommandKeyForUnitTest.KeyOne, CommandGroupForUnitTest.OwnerOne, new MockingHystrixCommandProperties(properties), HystrixEventNotifierDefault.Instance);
        }
    }
}
