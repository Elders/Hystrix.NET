namespace Elders.Hystrix.NET
{
    public class HystrixCollapserSetter
    {
        private readonly IHystrixCollapserKey collapserKey;

        public IHystrixCollapserKey CollapserKey { get { return this.collapserKey; } }
        public HystrixCollapserScope Scope { get; private set; }
        public HystrixCollapserPropertiesSetter PropertiesSetter { get; private set; }

        private HystrixCollapserSetter(IHystrixCollapserKey collapserKey)
        {
            this.collapserKey = collapserKey;
        }

        public static HystrixCollapserSetter WithCollapserKey(IHystrixCollapserKey collapserKey)
        {
            return new HystrixCollapserSetter(collapserKey);
        }
        public HystrixCollapserSetter AndScope(HystrixCollapserScope scope)
        {
            Scope = scope;
            return this;
        }
        public HystrixCollapserSetter AndCollapserPropertiesDefaults(HystrixCollapserPropertiesSetter propertiesSetter)
        {
            PropertiesSetter = propertiesSetter;
            return this;
        }
    }
}
