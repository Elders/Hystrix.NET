namespace Elders.Hystrix.NET.Test
{
    using Elders.Hystrix.NET;

    public static class CommandKeyForUnitTest
    {
        public static readonly HystrixCommandKey KeyOne = new HystrixCommandKey("KeyOne");
        public static readonly HystrixCommandKey KeyTwo = new HystrixCommandKey("KeyTwo");
    }
}
