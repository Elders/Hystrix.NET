namespace Netflix.Hystrix.Strategy.Concurrency
{
    using Java.Util.Concurrent;

    public class HystrixContextRunnable : IRunnable
    {
        private readonly IRunnable actual;
        private readonly HystrixRequestContext parentThreadState;

        public HystrixContextRunnable(IRunnable actual)
        {
            this.actual = actual;
            this.parentThreadState = HystrixRequestContext.ContextForCurrentThread;
        }

        public void Run()
        {
            HystrixRequestContext existingState = HystrixRequestContext.ContextForCurrentThread;
            try
            {
                HystrixRequestContext.ContextForCurrentThread = this.parentThreadState;
                this.actual.Run();
            }
            finally
            {
                HystrixRequestContext.ContextForCurrentThread = existingState;
            }
        }
    }
}
