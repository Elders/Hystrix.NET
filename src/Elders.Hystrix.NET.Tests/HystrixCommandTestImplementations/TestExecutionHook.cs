namespace Elders.Hystrix.NET.Test.HystrixCommandTestImplementations
{
    using System;
    using Java.Util.Concurrent.Atomic;
    using Elders.Hystrix.NET;
    using Elders.Hystrix.NET.Exceptions;
    using Elders.Hystrix.NET.Strategy.ExecutionHook;

    internal class TestExecutionHook : HystrixCommandExecutionHookDefault
    {
        private AtomicInteger startExecute = new AtomicInteger();
        private object endExecuteSuccessResponse = null;
        private Exception endExecuteFailureException = null;
        private FailureType? endExecuteFailureType = null;
        private AtomicInteger startRun = new AtomicInteger();
        private object runSuccessResponse = null;
        private Exception runFailureException = null;
        private AtomicInteger threadComplete = new AtomicInteger();
        private AtomicInteger threadStart = new AtomicInteger();
        private Exception fallbackFailureException = null;
        private AtomicInteger startFallback = new AtomicInteger();
        private object fallbackSuccessResponse = null;

        public int StartExecute { get { return this.startExecute.Value; } }
        public object EndExecuteSuccessResponse { get { return this.endExecuteSuccessResponse; } }
        public Exception EndExecuteFailureException { get { return this.endExecuteFailureException; } }
        public FailureType? EndExecuteFailureType { get { return this.endExecuteFailureType; } }
        public int StartRun { get { return this.startRun.Value; } }
        public object RunSuccessResponse { get { return this.runSuccessResponse; } }
        public Exception RunFailureException { get { return this.runFailureException; } }
        public int ThreadComplete { get { return this.threadComplete.Value; } }
        public int ThreadStart { get { return this.threadStart.Value; } }
        public Exception FallbackFailureException { get { return this.fallbackFailureException; } }
        public int StartFallback { get { return this.startFallback.Value; } }
        public object FallbackSuccessResponse { get { return this.fallbackSuccessResponse; } }

        public override void OnStart<T>(HystrixCommand<T> commandInstance)
        {
            this.startExecute.IncrementAndGet();
            base.OnStart(commandInstance);
        }
        public override T OnComplete<T>(HystrixCommand<T> commandInstance, T response)
        {
            this.endExecuteSuccessResponse = response;
            return base.OnComplete(commandInstance, response);
        }
        public override Exception OnError<T>(HystrixCommand<T> commandInstance, FailureType failureType, Exception e)
        {
            this.endExecuteFailureException = e;
            this.endExecuteFailureType = failureType;
            return base.OnError(commandInstance, failureType, e);
        }
        public override void OnRunStart<T>(HystrixCommand<T> commandInstance)
        {
            this.startRun.IncrementAndGet();
            base.OnRunStart(commandInstance);
        }
        public override T OnRunSuccess<T>(HystrixCommand<T> commandInstance, T response)
        {
            this.runSuccessResponse = response;
            return base.OnRunSuccess(commandInstance, response);
        }
        public override Exception OnRunError<T>(HystrixCommand<T> commandInstance, Exception e)
        {
            this.runFailureException = e;
            return base.OnRunError(commandInstance, e);
        }
        public override void OnFallbackStart<T>(HystrixCommand<T> commandInstance)
        {
            this.startFallback.IncrementAndGet();
            base.OnFallbackStart(commandInstance);
        }
        public override T OnFallbackSuccess<T>(HystrixCommand<T> commandInstance, T response)
        {
            this.fallbackSuccessResponse = response;
            return base.OnFallbackSuccess(commandInstance, response);
        }
        public override Exception OnFallbackError<T>(HystrixCommand<T> commandInstance, Exception e)
        {
            this.fallbackFailureException = e;
            return base.OnFallbackError(commandInstance, e);
        }
        public override void OnThreadStart<T>(HystrixCommand<T> commandInstance)
        {
            this.threadStart.IncrementAndGet();
            base.OnThreadStart(commandInstance);
        }
        public override void OnThreadComplete<T>(HystrixCommand<T> commandInstance)
        {
            this.threadComplete.IncrementAndGet();
            base.OnThreadComplete(commandInstance);
        }
    }
}
