namespace Hystrix.Test.HystrixCommandTestImplementations
{
    using Netflix.Hystrix;

    /// <summary>
    /// Successful execution - no fallback implementation.
    /// </summary>
    internal class SuccessfulTestCommand : TestHystrixCommand<bool>
    {
        public SuccessfulTestCommand()
            : this(UnitTestSetterFactory.GetCommandPropertiesSetter())
        {
        }

        public SuccessfulTestCommand(HystrixCommandPropertiesSetter properties)
            : base(new TestCommandBuilder() { CommandPropertiesDefaults = properties })
        {
        }

        protected override bool Run()
        {
            return true;
        }
    }
}
