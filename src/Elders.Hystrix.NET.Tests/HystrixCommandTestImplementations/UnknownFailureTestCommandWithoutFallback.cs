namespace Elders.Hystrix.NET.Test.HystrixCommandTestImplementations
{
    using System;

    /// <summary>
    /// Failed execution with unknown exception (not HystrixException) - no fallback implementation.
    /// </summary>
    internal class UnknownFailureTestCommandWithoutFallback : TestHystrixCommand<bool>
    {
        public UnknownFailureTestCommandWithoutFallback()
            : base(new TestCommandBuilder())
        {
        }

        protected override bool Run()
        {
            throw new Exception("we failed with an unknown issue");
        }
    }
}
