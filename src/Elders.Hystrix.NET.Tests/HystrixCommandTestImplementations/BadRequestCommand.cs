namespace Elders.Hystrix.NET.Test.HystrixCommandTestImplementations
{
    using Elders.Hystrix.NET.Test.CircuitBreakerTestImplementations;
    using Elders.Hystrix.NET;
    using Elders.Hystrix.NET.Exceptions;

    internal class BadRequestCommand : TestHystrixCommand<bool>
    {
        public BadRequestCommand(TestCircuitBreaker circuitBreaker, ExecutionIsolationStrategy isolationType)
            : base(new TestCommandBuilder()
            {
                CircuitBreaker = circuitBreaker,
                CommandPropertiesDefaults = UnitTestSetterFactory.GetCommandPropertiesSetter().WithExecutionIsolationStrategy(isolationType),
            })
        {

        }

        protected override bool Run()
        {
            throw new HystrixBadRequestException("Message to developer that they passed in bad data or something like that.");
        }

        protected override bool GetFallback()
        {
            return false;
        }
    }
}
