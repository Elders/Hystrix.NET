namespace Elders.Hystrix.NET
{
    using Java.Util.Concurrent;

    public interface IHystrixExecutable<T>
    {
        T Execute();
        IFuture<T> Queue();
    }
}
