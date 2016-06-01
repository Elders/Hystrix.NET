using System;
using System.Threading;
using Netflix.Hystrix.CircuitBreaker;
using Netflix.Hystrix.Strategy;
using Netflix.Hystrix.ThreadPool;

namespace Netflix.Hystrix
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
