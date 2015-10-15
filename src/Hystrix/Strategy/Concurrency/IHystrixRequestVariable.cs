namespace Netflix.Hystrix.Strategy.Concurrency
{
    public interface IHystrixRequestVariable : IHystrixRequestVariableLifecycle
    {
        object Value { get; }
    }
    public interface IHystrixRequestVariable<T> : IHystrixRequestVariable, IHystrixRequestVariableLifecycle<T>
    {
        new T Value { get; }
    }
}
