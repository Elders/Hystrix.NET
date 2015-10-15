namespace Hystrix.Test.HystrixCommandTestImplementations
{
    using System;
    using System.Threading;

    /// <summary>
    /// This should timeout.
    /// </summary>
    internal class TestCommandWithTimeout : TestHystrixCommand<bool>
    {
        public const int FallbackNotImplemented = 1;
        public const int FallbackSuccess = 2;
        public const int FallbackFailure = 3;

        private readonly TimeSpan timeout;
        private readonly int fallbackBehavior;

        public TestCommandWithTimeout(TimeSpan timeout, int fallbackBehavior)
            : base(new TestCommandBuilder()
            {
                CommandPropertiesDefaults = UnitTestSetterFactory.GetCommandPropertiesSetter().WithExecutionIsolationThreadTimeout(timeout)
            })
        {
            this.timeout = timeout;
            this.fallbackBehavior = fallbackBehavior;
        }

        protected override bool Run()
        {
            try
            {
                Thread.Sleep((int)(this.timeout.TotalMilliseconds * 10));
            }
            catch (ThreadInterruptedException)
            {
                // ignore and sleep some more to simulate a dependency that doesn't obey interrupts
                try
                {
                    Thread.Sleep((int)(this.timeout.TotalMilliseconds * 2));
                }
                catch (Exception)
                {
                    // ignore
                }
            }
            return false;
        }

        protected override bool GetFallback()
        {
            if (this.fallbackBehavior == FallbackSuccess)
            {
                return false;
            }
            else if (fallbackBehavior == FallbackFailure)
            {
                throw new Exception("failed on fallback");
            }
            else
            {
                // FALLBACK_NOT_IMPLEMENTED
                return base.GetFallback();
            }
        }
    }
}
