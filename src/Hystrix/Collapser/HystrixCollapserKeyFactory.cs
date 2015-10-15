namespace Netflix.Hystrix
{
    using System.Collections.Concurrent;

    public static class HystrixCollapserKeyFactory
    {
        private static ConcurrentDictionary<string, IHystrixCollapserKey> intern = new ConcurrentDictionary<string, IHystrixCollapserKey>();

        public static IHystrixCollapserKey AsKey(string name)
        {
            return intern.GetOrAdd(name, w => new HystrixCollapserKeyDefault(w));
        }

        private class HystrixCollapserKeyDefault : IHystrixCollapserKey
        {
            public string Name { get; private set; }

            public HystrixCollapserKeyDefault(string name)
            {
                Name = name;
            }
        }
    }
}
