namespace Hystrix.Test
{
    using Netflix.Hystrix.ThreadPool;

    internal static class ThreadPoolKeyForUnitTest
    {
        public static readonly HystrixThreadPoolKey ThreadPoolOne = new HystrixThreadPoolKey("ThreadPoolOne");
        public static readonly HystrixThreadPoolKey ThreadPoolTwo = new HystrixThreadPoolKey("ThreadPoolTwo");
    }
}
