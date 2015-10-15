namespace Netflix.Hystrix
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    internal class SemaphoreQueuedWrapperFuture<T> : ICommandFuture<T>
    {
        private Reference<T> value;
        private CountdownEvent executionCompleted;
        private HystrixCommand<T> command;

        public SemaphoreQueuedWrapperFuture(Reference<T> value, CountdownEvent executionCompleted, HystrixCommand<T> command)
        {
            this.value = value;
            this.executionCompleted = executionCompleted;
            this.command = command;
        }

        public T Get()
        {
            this.executionCompleted.Wait();
            return this.value.Value;
        }

        public T Get(TimeSpan timeout)
        {
            return Get();
        }

        public bool IsCancelled
        {
            get { return false; }
        }

        public bool IsDone
        {
            get { return true; }
        }

        //object IFuture.Get()
        //{
        //    return Get();
        //}

        //object IFuture.Get(TimeSpan timeout)
        //{
        //    return Get(timeout);
        //}

        public bool Cancel(bool mayInterruptIfRunning)
        {
            return false;
        }

        public ExecutionResult GetExecutionResult()
        {
            return this.command.executionResult;
        }
    }
}
