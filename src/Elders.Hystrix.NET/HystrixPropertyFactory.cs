namespace Elders.Hystrix.NET
{
    using System;
    using System.Collections.Generic;

    public static class HystrixPropertyFactory
    {
        public static IHystrixProperty<T> AsProperty<T>(T value)
        {
            return new HystrixPropertyDefault<T>(value);
        }
        public static IHystrixProperty<T> AsProperty<T>(T? value, T defaultValue) where T : struct
        {
            return new HystrixPropertyDefault<T>(value.HasValue ? value.Value : defaultValue);
        }
        public static IHystrixProperty<T> AsProperty<T>(IHystrixProperty<T> value, T defaultValue) where T : class
        {
            return new HystrixPropertyWrapperProperty<T>(value, defaultValue);
        }
        public static IHystrixProperty<T> AsProperty<T>(params IHystrixProperty<T>[] values) where T : class
        {
            return AsProperty((IEnumerable<IHystrixProperty<T>>)values);
        }
        public static IHystrixProperty<T> AsProperty<T>(IEnumerable<IHystrixProperty<T>> values) where T : class
        {
            return new HystrixChainedProperty<T>(values);
        }
        public static IHystrixProperty<T> NullProperty<T>() where T : class
        {
            return new HystrixNullProperty<T>();
        }

        private class HystrixPropertyDefault<T> : IHystrixProperty<T>
        {
            private T value;

            public HystrixPropertyDefault(T value)
            {
                this.value = value;
            }

            public T Get()
            {
                return this.value;
            }
        }
        private class HystrixPropertyWrapperProperty<T> : IHystrixProperty<T> where T : class
        {
            private IHystrixProperty<T> value;
            private T defaultValue;

            public HystrixPropertyWrapperProperty(IHystrixProperty<T> value, T defaultValue)
            {
                this.value = value;
                this.defaultValue = defaultValue;
            }

            public T Get()
            {
                return this.value.Get() ?? this.defaultValue;
            }
        }
        private class HystrixChainedProperty<T> : IHystrixProperty<T> where T : class
        {
            private IEnumerable<IHystrixProperty<T>> values;

            public HystrixChainedProperty(IEnumerable<IHystrixProperty<T>> values)
            {
                this.values = values;
            }

            public T Get()
            {
                foreach (IHystrixProperty<T> value in this.values)
                {
                    T v = value.Get();
                    if (v != null)
                    {
                        return v;
                    }
                }
                return null;
            }
        }
        private class HystrixNullProperty<T> : IHystrixProperty<T> where T : class
        {
            public T Get()
            {
                return null;
            }
        }
    }
}
