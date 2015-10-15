namespace Netflix.Hystrix.Strategy.Concurrency
{
    using System;

    public class DelegateRequestVariableLifecycle : IHystrixRequestVariableLifecycle
    {
        private Func<object> initialValue;
        private Action<object> shutdown;

        public DelegateRequestVariableLifecycle(Func<object> initialValue)
            : this(initialValue, null)
        {
        }
        public DelegateRequestVariableLifecycle(Func<object> initialValue, Action<object> shutdown)
        {
            if (initialValue == null)
                throw new ArgumentNullException("initialValue");

            this.initialValue = initialValue;
            this.shutdown = shutdown;
        }

        public object InitialValue()
        {
            return this.initialValue();
        }

        public void Shutdown(object value)
        {
            if (this.shutdown != null)
            {
                this.shutdown(value);
            }
        }
    }

    public class DelegateRequestVariableLifecycle<T> : IHystrixRequestVariableLifecycle<T>
    {
        private Func<T> initialValue;
        private Action<T> shutdown;

        public DelegateRequestVariableLifecycle(Func<T> initialValue)
            : this(initialValue, null)
        {
        }
        public DelegateRequestVariableLifecycle(Func<T> initialValue, Action<T> shutdown)
        {
            if (initialValue == null)
                throw new ArgumentNullException("initialValue");

            this.initialValue = initialValue;
            this.shutdown = shutdown;
        }

        public T InitialValue()
        {
            return this.initialValue();
        }

        public void Shutdown(T value)
        {
            if (this.shutdown != null)
            {
                this.shutdown(value);
            }
        }

        object IHystrixRequestVariableLifecycle.InitialValue()
        {
            return InitialValue();
        }
        void IHystrixRequestVariableLifecycle.Shutdown(object value)
        {
            Shutdown((T)value);
        }
    }
}
