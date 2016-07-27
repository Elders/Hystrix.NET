namespace Elders.Hystrix.NET.Strategy.ExecutionHook
{
    using System;
    using Elders.Hystrix.NET.Exceptions;

    public interface IHystrixCommandExecutionHook
    {
        void OnRunStart<T>(HystrixCommand<T> commandInstance);
        T OnRunSuccess<T>(HystrixCommand<T> commandInstance, T response);
        Exception OnRunError<T>(HystrixCommand<T> commandInstance, Exception e);
        void OnFallbackStart<T>(HystrixCommand<T> commandInstance);
        T OnFallbackSuccess<T>(HystrixCommand<T> commandInstance, T fallbackResponse);
        Exception OnFallbackError<T>(HystrixCommand<T> commandInstance, Exception e);
        void OnStart<T>(HystrixCommand<T> commandInstance);
        T OnComplete<T>(HystrixCommand<T> commandInstance, T response);
        Exception OnError<T>(HystrixCommand<T> commandInstance, FailureType failureType, Exception e);
        void OnThreadStart<T>(HystrixCommand<T> commandInstance);
        void OnThreadComplete<T>(HystrixCommand<T> commandInstance);
    }
}
