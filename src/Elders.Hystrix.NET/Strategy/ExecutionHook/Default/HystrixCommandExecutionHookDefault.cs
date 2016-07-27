namespace Elders.Hystrix.NET.Strategy.ExecutionHook
{
    using System;
    using Elders.Hystrix.NET.Exceptions;

    public class HystrixCommandExecutionHookDefault : IHystrixCommandExecutionHook
    {
        private static HystrixCommandExecutionHookDefault instance = new HystrixCommandExecutionHookDefault();
        public static HystrixCommandExecutionHookDefault Instance { get { return instance; } }

        protected HystrixCommandExecutionHookDefault()
        {
        }

        public virtual void OnRunStart<T>(HystrixCommand<T> commandInstance)
        {
            // do nothing by default
        }
        public virtual T OnRunSuccess<T>(HystrixCommand<T> commandInstance, T response)
        {
            // pass-thru by default
            return response;
        }
        public virtual Exception OnRunError<T>(HystrixCommand<T> commandInstance, Exception e)
        {
            // pass-thru by default
            return e;
        }
        public virtual void OnFallbackStart<T>(HystrixCommand<T> commandInstance)
        {
            // do nothing by default
        }
        public virtual T OnFallbackSuccess<T>(HystrixCommand<T> commandInstance, T fallbackResponse)
        {
            // pass-thru by default
            return fallbackResponse;
        }
        public virtual Exception OnFallbackError<T>(HystrixCommand<T> commandInstance, Exception e)
        {
            // pass-thru by default
            return e;
        }
        public virtual void OnStart<T>(HystrixCommand<T> commandInstance)
        {
            // do nothing by default
        }
        public virtual T OnComplete<T>(HystrixCommand<T> commandInstance, T response)
        {
            // pass-thru by default
            return response;
        }
        public virtual Exception OnError<T>(HystrixCommand<T> commandInstance, FailureType failureType, Exception e)
        {
            // pass-thru by default
            return e;
        }
        public virtual void OnThreadStart<T>(HystrixCommand<T> commandInstance)
        {
            // do nothing by default
        }
        public virtual void OnThreadComplete<T>(HystrixCommand<T> commandInstance)
        {
            // do nothing by default
        }
    }
}
