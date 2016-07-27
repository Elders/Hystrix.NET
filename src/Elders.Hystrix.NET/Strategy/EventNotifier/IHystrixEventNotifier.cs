namespace Elders.Hystrix.NET.Strategy.EventNotifier
{
    using System;
    using System.Collections.Generic;

    public interface IHystrixEventNotifier
    {
        void MarkEvent(HystrixEventType eventType, HystrixCommandKey key);
        void MarkCommandExecution(HystrixCommandKey key, ExecutionIsolationStrategy isolationStrategy, TimeSpan duration, IEnumerable<HystrixEventType> eventsDuringExecution);
    }
}
