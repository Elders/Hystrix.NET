namespace Elders.Hystrix.NET
{
    using Java.Util.Concurrent;

    internal interface ICommandFuture<K> : IFuture<K>
    {
        /// <summary>
        /// Allow retrieving the executionResult from 1 Future in another Future (due to request caching). 
        /// </summary>
        /// <returns></returns>
        ExecutionResult GetExecutionResult();
    }
}
