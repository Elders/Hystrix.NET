using Elders.Hystrix.NET.Test.CircuitBreakerTestImplementations;
namespace Elders.Hystrix.NET.Test.HystrixCommandTestImplementations
{
    /// <summary>
    /// A Command implementation that supports caching.
    /// </summary>
    internal class SuccessfulCacheableCommand : TestHystrixCommand<string>
    {
        private readonly bool cacheEnabled;
        private volatile bool executed = false;
        private readonly string value;

        public bool Executed { get { return this.executed; } }

        public SuccessfulCacheableCommand(TestCircuitBreaker circuitBreaker, bool cacheEnabled, string value)
            : base(new TestCommandBuilder()
            {
                CircuitBreaker = circuitBreaker,
                Metrics = circuitBreaker.Metrics,
            })
        {
            this.value = value;
            this.cacheEnabled = cacheEnabled;
        }

        protected override string Run()
        {
            this.executed = true;
            return value;
        }

        public bool IsCommandRunningInThread { get { return Properties.ExecutionIsolationStrategy.Get() == Elders.Hystrix.NET.ExecutionIsolationStrategy.Thread; } }

        protected override string GetCacheKey()
        {
            if (this.cacheEnabled)
                return this.value;
            else
                return null;
        }
    }
}
