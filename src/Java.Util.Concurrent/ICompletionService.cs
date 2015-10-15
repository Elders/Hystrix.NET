namespace Java.Util.Concurrent
{
    using System;

    public interface ICompletionService<T>
    {
        IFuture<T> Poll();
        IFuture<T> Poll(TimeSpan timeout);
        IFuture<T> Submit(ICallable<T> task);
        IFuture<T> Submit(IRunnable task, T result);
        IFuture<T> Take();
    }
}
