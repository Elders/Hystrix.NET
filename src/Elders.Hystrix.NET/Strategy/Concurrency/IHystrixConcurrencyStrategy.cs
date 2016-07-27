namespace Elders.Hystrix.NET.Strategy.Concurrency
{
    using System;
    using Java.Util.Concurrent;
    using Elders.Hystrix.NET.ThreadPool;

    public interface IHystrixConcurrencyStrategy
    {
        ThreadPoolExecutor GetThreadPool(HystrixThreadPoolKey threadPoolKey, IHystrixProperty<int> corePoolSize, IHystrixProperty<int> maximumPoolSize, IHystrixProperty<TimeSpan> keepAliveTime, IBlockingQueue<IRunnable> workQueue);
        IBlockingQueue<IRunnable> GetBlockingQueue(int maxQueueSize);
        ICallable<T> WrapCallable<T>(ICallable<T> callable);
        IHystrixRequestVariable GetRequestVariable(IHystrixRequestVariableLifecycle rv);
        IHystrixRequestVariable<T> GetRequestVariable<T>(IHystrixRequestVariableLifecycle<T> rv);
    }
}
