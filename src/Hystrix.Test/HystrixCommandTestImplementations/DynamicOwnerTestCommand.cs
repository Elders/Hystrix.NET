namespace Hystrix.Test.HystrixCommandTestImplementations
{
    using Netflix.Hystrix;

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
