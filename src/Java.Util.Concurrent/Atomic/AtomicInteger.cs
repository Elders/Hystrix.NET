#pragma warning disable 420
namespace Java.Util.Concurrent.Atomic
{
    using System;
    using System.Globalization;
    using System.Threading;

    [Serializable]
    public class AtomicInteger : IFormattable
    {
        private volatile int integerValue;

        /// <summary>
        /// Gets or sets the current value.
        /// </summary>
        public int Value
        {
            get { return this.integerValue; }
            set { this.integerValue = value; }
        }

        public AtomicInteger()
            : this(0)
        {
        }
        public AtomicInteger(int initialValue)
        {
            this.integerValue = initialValue;
        }

        public int AddAndGet(int delta)
        {
            return Interlocked.Add(ref this.integerValue, delta);
        }
        public bool CompareAndSet(int expect, int update)
        {
            return Interlocked.CompareExchange(ref this.integerValue, update, expect) == expect;
        }
        public int DecrementAndGet()
        {
            return Interlocked.Decrement(ref this.integerValue);
        }
        public int GetAndDecrement()
        {
            return Interlocked.Decrement(ref this.integerValue) + 1;
        }
        public int GetAndIncrement()
        {
            return Interlocked.Increment(ref this.integerValue) - 1;
        }
        public int GetAndSet(int newValue)
        {
            return Interlocked.Exchange(ref this.integerValue, newValue);
        }
        public int IncrementAndGet()
        {
            return Interlocked.Increment(ref this.integerValue);
        }
        public bool WeakCompareAndSet(int expect, int update)
        {
            return CompareAndSet(expect, update);
        }

        public override bool Equals(object obj)
        {
            return obj as AtomicInteger == this;
        }
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
        public override string ToString()
        {
            return ToString(CultureInfo.CurrentCulture);
        }
        public string ToString(IFormatProvider formatProvider)
        {
            return Value.ToString(formatProvider);
        }
        public string ToString(string format)
        {
            return ToString(format, CultureInfo.CurrentCulture);
        }
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return Value.ToString(formatProvider);
        }

        public static bool operator ==(AtomicInteger left, AtomicInteger right)
        {
            if (Object.ReferenceEquals(left, null) || Object.ReferenceEquals(right, null))
                return false;

            return left.Value == right.Value;
        }
        public static bool operator !=(AtomicInteger left, AtomicInteger right)
        {
            return !(left == right);
        }
        public static implicit operator int(AtomicInteger atomic)
        {
            if (atomic == null)
            {
                return 0;
            }
            else
            {
                return atomic.Value;
            }
        }
    }
}
#pragma warning restore 420