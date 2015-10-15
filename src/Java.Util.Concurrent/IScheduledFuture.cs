namespace Java.Util.Concurrent
{
#pragma warning disable 1574 //TODO PHASED
    /// <summary> 
    /// A delayed result-bearing action that can be cancelled.
    /// </summary>
    /// <remarks>
    /// Usually a scheduled future is the result of scheduling
    /// a task with a <see cref="IScheduledExecutorService"/>.
    /// </remarks>
    /// <author>Doug Lea</author>
    /// <author>Griffin Caprio(.NET)</author>
    public interface IScheduledFuture<T> : IDelayed, IFuture<T> //JDK_1_6
#pragma warning restore 1574
    {
    }
}