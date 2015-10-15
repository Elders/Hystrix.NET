namespace Netflix.Hystrix
{
    public class Reference<T>
    {
        public virtual T Value { get; set; }

        public Reference()
            : this(default(T))
        {
        }
        public Reference(T value)
        {
            Value = value;
        }

        public virtual void Clear()
        {
            Value = default(T);
        }
    }
}
