namespace Elders.Hystrix.NET.Strategy.EventNotifier
{
    using System;
    using System.Collections.Generic;

    public class HystrixEventNotifierDefault : IHystrixEventNotifier
    {
        private static HystrixEventNotifierDefault instance = new HystrixEventNotifierDefault();
        public static HystrixEventNotifierDefault Instance { get { return instance; } }

        protected HystrixEventNotifierDefault()
        {
        }

        public virtual void MarkEvent(HystrixEventType eventType, HystrixCommandKey key)
        {
            // do nothing
        }

        public virtual void MarkCommandExecution(HystrixCommandKey key, ExecutionIsolationStrategy isolationStrategy, TimeSpan duration, IEnumerable<HystrixEventType> eventsDuringExecution)
        {
            // do nothing
        }
    }
}
