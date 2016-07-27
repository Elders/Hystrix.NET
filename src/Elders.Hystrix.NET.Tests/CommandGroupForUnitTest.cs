namespace Elders.Hystrix.NET.Test
{
    using Elders.Hystrix.NET;

    internal static class CommandGroupForUnitTest
    {
        public static readonly HystrixCommandGroupKey OwnerOne = new HystrixCommandGroupKey("OwnerOne");
        public static readonly HystrixCommandGroupKey OwnerTwo = new HystrixCommandGroupKey("OwnerTwo");
    }
}
