namespace Hystrix.Test.HystrixCommandTestImplementations
{
    using System;

    /// <summary>
    /// Failed execution - fallback implementation throws exception.
    /// </summary>
    internal class KnownFailureTestCommandWithFallbackFailure : TestHystrixCommand<bool>
    {
        public KnownFailureTestCommandWithFallbackFailure()
            : base(new TestCommandBuilder())
        {
        }

        protected override bool Run()
        {
            throw new Exception("we failed with a simulated issue");
        }

        protected override bool GetFallback()
        {
            throw new Exception("failed while getting fallback");
        }
    }
}
