namespace Hystrix.Test.HystrixCommandTestImplementations
{
    using System;
    using System.Threading;
    using Hystrix.Test.CircuitBreakerTestImplementations;

    /// <summary>
    /// A Command implementation that supports caching and execution takes a while.
    /// Used to test scenario where Futures are returned with a backing call still executing.
    /// </summary>
    internal class SlowCacheableCommand : TestHystrixCommand<string>
    {
        private readonly string value;
        private readonly TimeSpan duration;
        private volatile bool executed = false;

        public bool Executed { get { return this.executed; } }

        public SlowCacheableCommand(TestCircuitBreaker circuitBreaker, string value, TimeSpan duration)
            : base(new TestCommandBuilder()
            {
                CircuitBreaker = circuitBreaker,
                Metrics = circuitBreaker.Metrics,
            })
        {
            this.value = value;
            this.duration = duration;
        }

        protected override string Run()
        {
            this.executed = true;
            try
            {
                Thread.Sleep(this.duration);
            }
            catch (ThreadInterruptedException)
            {
            }
            return this.value;
        }

        protected override string GetCacheKey()
        {
            return this.value;
        }
    }
}
