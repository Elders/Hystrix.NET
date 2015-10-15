namespace Hystrix.Test.HystrixCommandTestImplementations
{
    using System;
    using Java.Util.Concurrent;
    using Netflix.Hystrix;
    using Netflix.Hystrix.ThreadPool;

    internal class SingleThreadedPool : IHystrixThreadPool
    {
        private readonly LinkedBlockingQueue<IRunnable> queue;
        private readonly ThreadPoolExecutor pool;
        private readonly int rejectionQueueSizeThreshold;

        public SingleThreadedPool(int queueSize)
            : this(queueSize, 100)
        {
        }
        public SingleThreadedPool(int queueSize, int rejectionQueueSizeThreshold)
        {
            this.queue = new LinkedBlockingQueue<IRunnable>(queueSize);
            this.pool = new ThreadPoolExecutor(1, 1, TimeSpan.FromMinutes(1.0), this.queue);
            this.rejectionQueueSizeThreshold = rejectionQueueSizeThreshold;
        }

        public LinkedBlockingQueue<IRunnable> Queue { get { return this.queue; } }
        public ThreadPoolExecutor Executor { get { return this.pool; } }
        public bool IsQueueSpaceAvailable { get { return this.queue.Count < this.rejectionQueueSizeThreshold; } }

        public void MarkThreadExecution()
        {
            // not used for this test
        }
        public void MarkThreadCompletion()
        {
            // not used for this test
        }
    }
}
