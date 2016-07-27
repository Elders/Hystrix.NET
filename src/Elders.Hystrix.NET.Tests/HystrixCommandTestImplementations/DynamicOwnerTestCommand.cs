namespace Elders.Hystrix.NET.Test.HystrixCommandTestImplementations
{
    using Elders.Hystrix.NET;

    /// <summary>
    /// Successful execution - no fallback implementation.
    /// </summary>
    internal class DynamicOwnerTestCommand : TestHystrixCommand<bool>
    {
        public DynamicOwnerTestCommand(HystrixCommandGroupKey owner)
            : base(new TestCommandBuilder() { Owner = owner })
        {
        }

        protected override bool Run()
        {
            return true;
        }
    }
}
