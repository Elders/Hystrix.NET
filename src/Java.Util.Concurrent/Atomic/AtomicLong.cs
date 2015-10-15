namespace Java.Util.Concurrent.Atomic
{
    using System;
    using System.Globalization;
    using System.Threading;

    [Serializable]
    public class AtomicLong : IFormattable
    {
        private long longValue;

        /// <summary>
        /// Gets or sets the current value.
        /// </summary>
        public long Value
        {
            get
            {
                return Interlocked.Read(ref this.longValue);
            }
            set
            {
                Interlocked.Exchange(ref this.longValue, value);
            }
        }

        public AtomicLong()
            : this(0)
        {
        }
        public AtomicLong(long initialValue)
        {
            this.longValue = initialValue;
        }

        public long AddAndGet(long delta)
        {
            return Interlocked.Add(ref this.longValue, delta);
        }
        public bool CompareAndSet(long expect, long update)
        {
            return Interlocked.CompareExchange(ref this.longValue, update, expect) == expect;
        }
        public long DecrementAndGet()
        {
            return Interlocked.Decrement(ref this.longValue);
        }
        public long GetAndDecrement()
        {
            return Interlocked.Decrement(ref this.longValue) + 1;
        }
        public long GetAndIncrement()
        {
            return Interlocked.Increment(ref this.longValue) - 1;
        }
        public long GetAndSet(long newValue)
        {
            return Interlocked.Exchange(ref this.longValue, newValue);
        }
        public long IncrementAndGet()
        {
            return Interlocked.Increment(ref this.longValue);
        }
        public bool WeakCompareAndSet(long expect, long update)
        {
            return CompareAndSet(expect, update);
        }

        public override bool Equals(object obj)
        {
            return obj as AtomicLong == this;
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

        public static bool operator ==(AtomicLong left, AtomicLong right)
        {
            if (Object.ReferenceEquals(left, null) || Object.ReferenceEquals(right, null))
                return false;

            return left.Value == right.Value;
        }
        public static bool operator !=(AtomicLong left, AtomicLong right)
        {
            return !(left == right);
        }
        public static implicit operator long(AtomicLong atomic)
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
