namespace Elders.Hystrix.NET.Test.HystrixCommandTestImplementations
{
    using Elders.Hystrix.NET;

    /// <summary>
    /// Successful execution - no fallback implementation.
    /// </summary>
    internal class DynamicOwnerAndKeyTestCommand : TestHystrixCommand<bool>
    {
        public DynamicOwnerAndKeyTestCommand(HystrixCommandGroupKey owner, HystrixCommandKey key)
            : base(new TestCommandBuilder()
            {
                Owner = owner,
                DependencyKey = key,
                CircuitBreaker = null,
                Metrics = null,
            })
        {
            // we specifically are NOT passing in a circuit breaker here so we test that it creates a new one correctly based on the dynamic key
        }

        protected override bool Run()
        {
            return true;
        }
    }
}
