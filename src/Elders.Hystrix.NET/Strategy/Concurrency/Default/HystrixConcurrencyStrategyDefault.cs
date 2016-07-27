namespace Elders.Hystrix.NET.Strategy.Concurrency
{
    using System;
    using System.Threading;
    using Java.Util.Concurrent;
    using Elders.Hystrix.NET.ThreadPool;

    public class HystrixConcurrencyStrategyDefault : IHystrixConcurrencyStrategy
    {
        private static HystrixConcurrencyStrategyDefault instance = new HystrixConcurrencyStrategyDefault();
        public static HystrixConcurrencyStrategyDefault Instance { get { return instance; } }

        protected HystrixConcurrencyStrategyDefault()
        {
        }

        public virtual ThreadPoolExecutor GetThreadPool(HystrixThreadPoolKey threadPoolKey, IHystrixProperty<int> corePoolSize, IHystrixProperty<int> maximumPoolSize, IHystrixProperty<TimeSpan> keepAliveTime, IBlockingQueue<IRunnable> workQueue)
        {
            return new ThreadPoolExecutor(corePoolSize.Get(), maximumPoolSize.Get(), keepAliveTime.Get(), workQueue, new DefaultThreadFactory(threadPoolKey));
        }

        public virtual IBlockingQueue<IRunnable> GetBlockingQueue(int maxQueueSize)
        {
            if (maxQueueSize <= 0)
            {
                return new SynchronousQueue<IRunnable>();
            }
            else
            {
                return new LinkedBlockingQueue<IRunnable>(maxQueueSize);
            }
        }

        public virtual ICallable<T> WrapCallable<T>(ICallable<T> callable)
        {
            return callable;
        }

        public virtual IHystrixRequestVariable GetRequestVariable(IHystrixRequestVariableLifecycle rv)
        {
            return new HystrixRequestVariableDefault(rv);
        }
        public virtual IHystrixRequestVariable<T> GetRequestVariable<T>(IHystrixRequestVariableLifecycle<T> rv)
        {
            return new HystrixRequestVariableDefault<T>(rv);
        }

        private class DefaultThreadFactory : IThreadFactory
        {
            private int threadNumber = 0;
            private HystrixThreadPoolKey key;

            public DefaultThreadFactory(HystrixThreadPoolKey key)
            {
                this.key = key;
            }

            public Thread NewThread(IRunnable runnable)
            {
                Thread thread = new Thread(runnable.Run);
                thread.Name = "hystrix-" + this.key.Name + "-" + Interlocked.Increment(ref this.threadNumber);
                return thread;
            }
        }
    }
}
