using Java.Util.Concurrent;
namespace Netflix.Hystrix
{
    internal class ExceptionFuture<K> : ICommandFuture<K>
    {
        private System.Exception exception;
        private ExecutionResult executionResult;

        public ExceptionFuture(System.Exception exception, ExecutionResult executionResult)
        {
            this.exception = exception;
            this.executionResult = executionResult;
        }

        public bool IsCancelled { get { return false; } }
        public bool IsDone { get { return true; } }

        public K Get()
        {
            throw new ExecutionException(this.exception);
        }
        public K Get(System.TimeSpan timeout)
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
        //object IFuture.Get(System.TimeSpan timeout)
        //{
        //    return Get(timeout);
        //}
    }
}
