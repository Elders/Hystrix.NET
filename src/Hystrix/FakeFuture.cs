namespace Netflix.Hystrix
{
    using System;

    internal class FakeFuture<K> : ICommandFuture<K>
    {
        private K value;
        private ExecutionResult executionResult;

        public FakeFuture(K value, ExecutionResult executionResult)
        {
            this.value = value;
            this.executionResult = executionResult;
        }

        public bool IsCancelled { get { return false; } }
        public bool IsDone { get { return true; } }

        public K Get()
        {
            return this.value;
        }
        public K Get(TimeSpan timeout)
        {
            return Get();
        }

        public bool Cancel(bool mayInterruptIfRunning)
        {
            return false;
        }
        public ExecutionResult GetExecutionResult()
        {
            return this.executionResult;
        }


        //object IFuture.Get()
        //{
        //    return Get();
        //}
        //object IFuture.Get(TimeSpan timeout)
        //{
        //    return Get(timeout);
        //}
    }
}
