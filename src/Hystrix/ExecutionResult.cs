namespace Netflix.Hystrix
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /**
     * Immutable holder class for the status of command execution.
     * <p>
     * Contained within a class to simplify the sharing of it across Futures/threads that result from request caching.
     * <p>
     * This object can be referenced and "modified" by parent and child threads as well as by different instances of HystrixCommand since
     * 1 instance could create an ExecutionResult, cache a Future that refers to it, a 2nd instance execution then retrieves a Future
     * from cache and wants to append RESPONSE_FROM_CACHE to whatever the ExecutionResult was from the first command execution.
     * <p>
     * This being immutable forces and ensure thread-safety instead of using AtomicInteger/ConcurrentLinkedQueue and determining
     * when it's safe to mutate the object directly versus needing to deep-copy clone to a new instance.
     */
    internal class ExecutionResult
    {
        private readonly List<HystrixEventType> events;
        private readonly int executionTime;
        private readonly System.Exception exception;

        public int ExecutionTimeInMilliseconds { get { return this.executionTime; } }
        public IEnumerable<HystrixEventType> Events { get { return this.events; } }
        public System.Exception Exception { get { return this.exception; } }

        private ExecutionResult(params HystrixEventType[] events)
            : this(events.ToList(), -1, null)
        {
        }

        public ExecutionResult SetExecutionTime(int executionTime)
        {
            return new ExecutionResult(events, executionTime, exception);
        }

        public ExecutionResult SetException(System.Exception e)
        {
            return new ExecutionResult(events, executionTime, e);
        }

        private ExecutionResult(List<HystrixEventType> events, int executionTime, System.Exception e)
        {
            // we are safe assigning the List reference instead of deep-copying
            // because we control the original list in 'newEvent'
            this.events = events;
            this.executionTime = executionTime;
            this.exception = e;
        }

        // we can return a static version since it's immutable
        public static ExecutionResult Empty = new ExecutionResult(new HystrixEventType[0]);

        /**
         * Creates a new ExecutionResult by adding the defined 'events' to the ones on the current instance.
         *
         * @param events
         * @return
         */
        public ExecutionResult AddEvents(params HystrixEventType[] events)
        {
            List<HystrixEventType> newEvents = new List<HystrixEventType>();
            newEvents.AddRange(this.events);
            foreach (HystrixEventType e in events)
            {
                newEvents.Add(e);
            }
            return new ExecutionResult(newEvents, executionTime, exception);
        }
    }
}
