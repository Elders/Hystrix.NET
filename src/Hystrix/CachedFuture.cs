namespace Netflix.Hystrix
{
    using System;
    using Java.Util.Concurrent;

    internal class CachedFuture<R> : IFuture<R>
    {
        private HystrixCommand<R> command;
        private ICommandFuture<R> commandFuture;

        public CachedFuture(HystrixCommand<R> command, ICommandFuture<R> commandFuture)
        {
            this.command = command;
            this.commandFuture = commandFuture;
        }

        public R Get()
        {
            try
            {
                return this.commandFuture.Get();
            }
            finally
            {
                // set this instance to the result that is from cache
                this.command.executionResult = this.commandFuture.GetExecutionResult();
                // add that this came from cache
                this.command.executionResult = this.command.executionResult.AddEvents(HystrixEventType.ResponseFromCache);
                // set the execution time to 0 since we retrieved from cache
                this.command.executionResult = this.command.executionResult.SetExecutionTime(-1);
            }
        }

        public R Get(TimeSpan timeout)
        {
            try
            {
                return this.commandFuture.Get(timeout);
            }
            finally
            {
                // set this instance to the result that is from cache
                this.command.executionResult = this.commandFuture.GetExecutionResult();
                // add that this came from cache
                this.command.executionResult = this.command.executionResult.AddEvents(HystrixEventType.ResponseFromCache);
                // set the execution time to 0 since we retrieved from cache
                this.command.executionResult = this.command.executionResult.SetExecutionTime(-1);
            }
        }

        public bool IsCancelled
        {
            get { return this.commandFuture.IsCancelled; }
        }

        public bool IsDone
        {
            get { return this.commandFuture.IsDone; }
        }

        public bool Cancel(bool mayInterruptIfRunning)
        {
            return this.commandFuture.Cancel(mayInterruptIfRunning);
        }
    }
}
