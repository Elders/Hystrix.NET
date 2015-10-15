namespace Hystrix.Test.HystrixCommandTestImplementations
{
    using System;
    using System.Threading;
    using Hystrix.Test.CircuitBreakerTestImplementations;
    using Java.Util.Concurrent;
    using Netflix.Hystrix;
    using Netflix.Hystrix.ThreadPool;

    internal class RequestCacheThreadRejectionWithoutFallback : TestHystrixCommand<bool>
    {
        private readonly CountdownEvent completionLatch;

        public RequestCacheThreadRejectionWithoutFallback(TestCircuitBreaker circuitBreaker, CountdownEvent completionLatch)
            : base(new TestCommandBuilder()
            {
                CircuitBreaker = circuitBreaker,
                Metrics = circuitBreaker.Metrics,
                ThreadPool = new RejectingThreadPool(),
            })
        {
            this.completionLatch = completionLatch;
        }

        protected override bool Run()
        {
            try
            {
                if (this.completionLatch.Wait(TimeSpan.FromSeconds(1.0)))
                    throw new Exception("timed out waiting on completionLatch");
            }
            catch (ThreadInterruptedException e)
            {
                throw new Exception("Unexpected exception.", e);
            }
            return true;
        }

        protected override string GetCacheKey()
        {
            return "A";
        }

        private class RejectingThreadPool : IHystrixThreadPool
        {
            public ThreadPoolExecutor Executor
            {
                get { return null; }
            }

            public void MarkThreadExecution()
            {
            }

            public void MarkThreadCompletion()
            {
            }

            public bool IsQueueSpaceAvailable
            {
                get { return false; }
            }
        }
    }
}
