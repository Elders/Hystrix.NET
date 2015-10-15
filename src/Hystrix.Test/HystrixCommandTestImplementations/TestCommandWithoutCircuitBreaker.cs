namespace Hystrix.Test.HystrixCommandTestImplementations
{
    /// <summary>
    /// Successful execution - no fallback implementation, circuit-breaker disabled.
    /// </summary>
    internal class TestCommandWithoutCircuitBreaker : TestHystrixCommand<bool>
    {
        public TestCommandWithoutCircuitBreaker()
            : base(new TestCommandBuilder()
            {
                CommandPropertiesDefaults = UnitTestSetterFactory.GetCommandPropertiesSetter().WithCircuitBreakerEnabled(false)
            })
        {
        }

        protected override bool Run()
        {
            return true;
        }
    }
}
