namespace Java.Util.Concurrent
{
    using System;

    public interface IFuture<T>
    {
        bool Cancel(bool mayInterruptIfRunning);
        T Get();
        T Get(TimeSpan timeout);
        bool IsCancelled { get; }
        bool IsDone { get; }
    }
}