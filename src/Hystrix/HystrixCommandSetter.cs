namespace Netflix.Hystrix
{
    using Netflix.Hystrix.ThreadPool;

    public class HystrixCommandSetter
    {
        private readonly HystrixCommandGroupKey groupKey;

        public HystrixCommandGroupKey GroupKey { get { return this.groupKey; } }
        public HystrixCommandKey CommandKey { get; private set; }
        public HystrixThreadPoolKey ThreadPoolKey { get; private set; }
        public HystrixCommandPropertiesSetter CommandPropertiesDefaults { get; private set; }
        public HystrixThreadPoolPropertiesSetter ThreadPoolPropertiesDefaults { get; private set; }

        public HystrixCommandSetter(HystrixCommandGroupKey groupKey)
        {
            this.groupKey = groupKey;
        }

        public static HystrixCommandSetter WithGroupKey(HystrixCommandGroupKey groupKey)
        {
            return new HystrixCommandSetter(groupKey);
        }
        public HystrixCommandSetter AndCommandKey(HystrixCommandKey commandKey)
        {
            CommandKey = commandKey;
            return this;
        }
        public HystrixCommandSetter AndThreadPoolKey(HystrixThreadPoolKey threadPoolKey)
        {
            ThreadPoolKey = threadPoolKey;
            return this;
        }
        public HystrixCommandSetter AndCommandPropertiesDefaults(HystrixCommandPropertiesSetter commandPropertiesDefaults)
        {
            CommandPropertiesDefaults = commandPropertiesDefaults;
            return this;
        }
        public HystrixCommandSetter AndThreadPoolPropertiesDefaults(HystrixThreadPoolPropertiesSetter threadPoolPropertiesDefaults)
        {
            ThreadPoolPropertiesDefaults = threadPoolPropertiesDefaults;
            return this;
        }
    }
}
