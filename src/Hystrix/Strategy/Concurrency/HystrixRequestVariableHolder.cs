namespace Netflix.Hystrix.Strategy.Concurrency
{
    using Logging;
    using System.Collections.Concurrent;

    public class HystrixRequestVariableHolder
    {
        internal static readonly ILog logger = LogProvider.GetLogger(typeof(HystrixRequestVariableHolder));

        private static readonly ConcurrentDictionary<HystrixRequestVariableCacheKey, IHystrixRequestVariable> requestVariableInstance = new ConcurrentDictionary<HystrixRequestVariableCacheKey, IHystrixRequestVariable>();

        private readonly IHystrixRequestVariableLifecycle lifeCycleMethods;

        public HystrixRequestVariableHolder(IHystrixRequestVariableLifecycle lifeCycleMethods)
        {
            this.lifeCycleMethods = lifeCycleMethods;
        }

        public object Get(IHystrixConcurrencyStrategy concurrencyStrategy)
        {
            HystrixRequestVariableCacheKey key = new HystrixRequestVariableCacheKey(this, concurrencyStrategy);
            IHystrixRequestVariable rvInstance = requestVariableInstance.GetOrAdd(key, w =>
            {
                if (requestVariableInstance.Count > 100)
                {
                    logger.Warn("Over 100 instances of HystrixRequestVariable are being stored. This is likely the sign of a memory leak caused by using unique instances of HystrixConcurrencyStrategy instead of a single instance.");
                }

                return concurrencyStrategy.GetRequestVariable(lifeCycleMethods);
            });

            return rvInstance.Value;
        }
    }

    public class HystrixRequestVariableHolder<T> : HystrixRequestVariableHolder
    {
        public HystrixRequestVariableHolder(IHystrixRequestVariableLifecycle<T> lifeCycleMethods)
            : base(lifeCycleMethods)
        {
        }

        public new T Get(IHystrixConcurrencyStrategy concurrencyStrategy)
        {
            return (T)base.Get(concurrencyStrategy);
        }
    }
}
