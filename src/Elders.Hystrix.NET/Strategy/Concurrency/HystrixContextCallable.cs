namespace Elders.Hystrix.NET.Strategy.Concurrency
{
    using System;
    using Java.Util.Concurrent;

    public class HystrixContextCallable<T> : ICallable<T>
    {
        private readonly ICallable<T> actual;
        private readonly HystrixRequestContext parentThreadState;

        public HystrixContextCallable(ICallable<T> actual)
        {
            this.actual = actual;
            this.parentThreadState = HystrixRequestContext.ContextForCurrentThread;
        }

        public T Call()
        {
            HystrixRequestContext existingState = HystrixRequestContext.ContextForCurrentThread;
            try
            {
                HystrixRequestContext.ContextForCurrentThread = this.parentThreadState;
                return this.actual.Call();
            }
            finally
            {
                HystrixRequestContext.ContextForCurrentThread = existingState;
            }
        }
    }
}
