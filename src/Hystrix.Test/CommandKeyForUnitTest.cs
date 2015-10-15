namespace Hystrix.Test
{
    using Netflix.Hystrix;

    public static class CommandKeyForUnitTest
    {
        public static readonly HystrixCommandKey KeyOne = new HystrixCommandKey("KeyOne");
        public static readonly HystrixCommandKey KeyTwo = new HystrixCommandKey("KeyTwo");
    }
}
