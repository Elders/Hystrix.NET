namespace Elders.Hystrix.NET.Test
{
    using Elders.Hystrix.NET.ThreadPool;

    internal static class ThreadPoolKeyForUnitTest
    {
        public static readonly HystrixThreadPoolKey ThreadPoolOne = new HystrixThreadPoolKey("ThreadPoolOne");
        public static readonly HystrixThreadPoolKey ThreadPoolTwo = new HystrixThreadPoolKey("ThreadPoolTwo");
    }
}
