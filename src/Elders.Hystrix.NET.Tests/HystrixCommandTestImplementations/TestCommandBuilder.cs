namespace Elders.Hystrix.NET.Test.HystrixCommandTestImplementations
{
    using Elders.Hystrix.NET.Test.CircuitBreakerTestImplementations;
    using Elders.Hystrix.NET;
    using Elders.Hystrix.NET.CircuitBreaker;
    using Elders.Hystrix.NET.ThreadPool;

    internal class TestCommandBuilder
    {
        public HystrixCommandGroupKey Owner { get; set; }
        public HystrixCommandKey DependencyKey { get; set; }
        public HystrixThreadPoolKey ThreadPoolKey { get; set; }
        public IHystrixCircuitBreaker CircuitBreaker { get; set; }
        public IHystrixThreadPool ThreadPool { get; set; }
        public HystrixCommandPropertiesSetter CommandPropertiesDefaults { get; set; }
        public HystrixThreadPoolPropertiesSetter ThreadPoolPropertiesDefaults { get; set; }
        public HystrixCommandMetrics Metrics { get; set; }
        public TryableSemaphore FallbackSemaphore { get; set; }
        public TryableSemaphore ExecutionSemaphore { get; set; }
        public TestExecutionHook ExecutionHook { get; private set; }

        public TestCommandBuilder()
        {
            TestCircuitBreaker cb = new TestCircuitBreaker();

            Owner = CommandGroupForUnitTest.OwnerOne;
            DependencyKey = null;
            ThreadPoolKey = null;
            CircuitBreaker = cb;
            ThreadPool = null;
            CommandPropertiesDefaults = UnitTestSetterFactory.GetCommandPropertiesSetter();
            ThreadPoolPropertiesDefaults = UnitTestSetterFactory.GetThreadPoolPropertiesSetter();
            Metrics = cb.Metrics;
            FallbackSemaphore = null;
            ExecutionSemaphore = null;
            ExecutionHook = new TestExecutionHook();
        }
    }
}
