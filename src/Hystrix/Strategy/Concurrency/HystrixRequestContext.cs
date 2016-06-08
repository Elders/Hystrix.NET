namespace Netflix.Hystrix.Strategy.Concurrency
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    public class HystrixRequestContext : IDisposable
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(HystrixRequestContext));

        private static readonly ThreadLocal<HystrixRequestContext> requestContexts = new ThreadLocal<HystrixRequestContext>();

        public static bool IsCurrentThreadInitialized
        {
            get
            {
                HystrixRequestContext requestContext = requestContexts.Value;
                return requestContext != null && requestContext.State != null;
            }
        }
        public static HystrixRequestContext InitializeContext()
        {
            HystrixRequestContext requestContext = new HystrixRequestContext();
            requestContexts.Value = requestContext;
            return requestContext;
        }
        public static HystrixRequestContext ContextForCurrentThread
        {
            get
            {
                HystrixRequestContext requestContext = requestContexts.Value;
                if (requestContext != null && requestContext.State != null)
                {
                    // context.state can be null when context is not null
                    // if a thread is being re-used and held a context previously, the context was shut down
                    // but the thread was not cleared
                    return requestContext;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                requestContexts.Value = value;
            }
        }

        internal ConcurrentDictionary<IHystrixRequestVariable, object> State { get; private set; }

        private HystrixRequestContext()
        {
            State = new ConcurrentDictionary<IHystrixRequestVariable, object>();
        }

        public void Dispose()
        {
            if (State != null)
            {
                foreach (IHystrixRequestVariable rv in State.Keys)
                {
                    try
                    {
                        rv.Shutdown(State[rv]);
                    }
                    catch (Exception e)
                    {
                        logger.Error("Error in shutdown, will continue with shutdown of other variables", e);
                    }
                }
                State = null;
            }
        }

        internal void Shutdown()
        {
        }
    }
}
