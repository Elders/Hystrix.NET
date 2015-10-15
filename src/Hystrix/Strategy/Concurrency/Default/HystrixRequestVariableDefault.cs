namespace Netflix.Hystrix.Strategy.Concurrency
{
    using System;

    public class HystrixRequestVariableDefault : IHystrixRequestVariable
    {
        private IHystrixRequestVariableLifecycle lifecycle;

        public object Value
        {
            get
            {
                if (!HystrixRequestContext.IsCurrentThreadInitialized)
                    throw new InvalidOperationException(String.Format("{0}.InitializeContext() must be called at the beginning of each request before RequestVariable functionality can be used."));

                return HystrixRequestContext.ContextForCurrentThread.State.GetOrAdd(this, InitialValue());
            }
            set
            {
                HystrixRequestContext.ContextForCurrentThread.State.AddOrUpdate(this, value, (key, oldValue) => value);
            }
        }

        public HystrixRequestVariableDefault(IHystrixRequestVariableLifecycle lifecycle)
        {
            if (lifecycle == null)
                throw new ArgumentNullException("lifecycle");

            this.lifecycle = lifecycle;
        }

        public virtual void Shutdown(object value)
        {
            this.lifecycle.Shutdown(value);
        }
        public virtual object InitialValue()
        {
            return lifecycle.InitialValue();
        }
    }
    public class HystrixRequestVariableDefault<T> : IHystrixRequestVariable<T>
    {
        private IHystrixRequestVariableLifecycle<T> lifecycle;

        public T Value
        {
            get
            {
                if (!HystrixRequestContext.IsCurrentThreadInitialized)
                    throw new InvalidOperationException(String.Format("{0}.InitializeContext() must be called at the beginning of each request before RequestVariable functionality can be used."));

                return (T)HystrixRequestContext.ContextForCurrentThread.State.GetOrAdd(this, default(T));
            }
            set
            {
                HystrixRequestContext.ContextForCurrentThread.State.AddOrUpdate(this, value, (key, oldValue) => value);
            }
        }

        public HystrixRequestVariableDefault(IHystrixRequestVariableLifecycle<T> lifecycle)
        {
            if (lifecycle == null)
                throw new ArgumentNullException("lifecycle");

            this.lifecycle = lifecycle;
        }

        public virtual void Shutdown(T value)
        {
            this.lifecycle.Shutdown(value);
        }
        public virtual T InitialValue()
        {
            return lifecycle.InitialValue();
        }

        object IHystrixRequestVariable.Value
        {
            get
            {
                return Value;
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
