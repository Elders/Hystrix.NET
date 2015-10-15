namespace Netflix.Hystrix.Strategy.Concurrency
{
    public interface IHystrixRequestVariableLifecycle
    {
        object InitialValue();
        void Shutdown(object value);
    }
    public interface IHystrixRequestVariableLifecycle<T> : IHystrixRequestVariableLifecycle
    {
        new T InitialValue();
        void Shutdown(T value);
    }
}
