using System;
using System.Threading;
using Elders.Hystrix.NET.CircuitBreaker;
using Elders.Hystrix.NET.Strategy;
using Elders.Hystrix.NET.ThreadPool;

namespace Elders.Hystrix.NET
{
    public static class Hystrix
    {
        public static void Reset()
        {
            Reset(Timeout.InfiniteTimeSpan);
        }

        public static void Reset(TimeSpan timeout)
        {
            HystrixThreadPoolFactory.Shutdown(timeout);
            HystrixCommandMetrics.Reset();
            //HystrixCollapser.Reset();
            HystrixCircuitBreakerFactory.Reset();
            HystrixPlugins.Reset();

            //HystrixPropertiesFactory.reset();
            //currentCommand.set(new LinkedList<HystrixCommandKey>());
        }
    }
}
