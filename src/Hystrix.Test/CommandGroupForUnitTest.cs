namespace Hystrix.Test
{
    using Netflix.Hystrix;

    internal static class CommandGroupForUnitTest
    {
        public static readonly HystrixCommandGroupKey OwnerOne = new HystrixCommandGroupKey("OwnerOne");
        public static readonly HystrixCommandGroupKey OwnerTwo = new HystrixCommandGroupKey("OwnerTwo");
    }
}
