namespace Elders.Hystrix.NET
{
    public enum HystrixEventType
    {
        Success,
        Failure,
        Timeout,
        ShortCircuited,
        ThreadPoolRejected,
        SemaphoreRejected,
        FallbackSuccess,
        FallbackFailure,
        FallbackRejection,
        ExceptionThrown,
        ResponseFromCache,
        Collapsed,
    }
}
