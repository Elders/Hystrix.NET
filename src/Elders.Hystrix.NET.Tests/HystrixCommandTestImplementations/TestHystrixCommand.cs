namespace Elders.Hystrix.NET.Test.HystrixCommandTestImplementations
{
    using Elders.Hystrix.NET;

    internal abstract class TestHystrixCommand<T> : HystrixCommand<T>
    {
        private readonly TestCommandBuilder builder;

        public TestCommandBuilder Builder { get { return this.builder; } }

        protected TestHystrixCommand(TestCommandBuilder builder)
            : base(builder.Owner, builder.DependencyKey, builder.ThreadPoolKey, builder.CircuitBreaker, builder.ThreadPool,
                   builder.CommandPropertiesDefaults, builder.ThreadPoolPropertiesDefaults, builder.Metrics,
                   builder.FallbackSemaphore, builder.ExecutionSemaphore, TestPropertiesFactory.Instance, builder.ExecutionHook)
        {
            this.builder = builder;
        }
    }
}
