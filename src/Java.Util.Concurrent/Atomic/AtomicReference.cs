#pragma warning disable 420
namespace Java.Util.Concurrent.Atomic
{
    using System;
    using System.Globalization;
    using System.Threading;

    [Serializable]
    public class AtomicReference<T> where T : class
    {
        private volatile T reference;

        /// <summary>
        /// Gets or sets the current value.
        /// </summary>
        public T Value
        {
            get { return reference; }
            set { this.reference = value; }
        }

        public AtomicReference()
            : this(default(T))
        {
        }
        public AtomicReference(T initialValue)
        {
            reference = initialValue;
        }

        public bool CompareAndSet(T expect, T update)
        {
            return ReferenceEquals(expect, Interlocked.CompareExchange(ref this.reference, update, expect));
        }
        public T GetAndSet(T newValue)
        {
            return Interlocked.Exchange(ref this.reference, newValue);
        }
        public bool WeakCompareAndSet(T expect, T update)
        {
            return CompareAndSet(expect, update);
        }

        public override bool Equals(object obj)
        {
            return obj as AtomicReference<T> == this;
        }
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
        public override string ToString()
        {
            return Value.ToString();
        }

        public static bool operator ==(AtomicReference<T> left, AtomicReference<T> right)
        {
            if (Object.ReferenceEquals(left, null) || Object.ReferenceEquals(right, null))
                return false;

            return Object.ReferenceEquals(left.Value, right.Value);
        }
        public static bool operator !=(AtomicReference<T> left, AtomicReference<T> right)
        {
            return !(left == right);
        }
        public static implicit operator T(AtomicReference<T> atomic)
        {
            if (atomic == null)
            {
                return default(T);
            }
            else
            {
                return atomic.Value;
            }
        }
    }
}
#pragma warning restore 420