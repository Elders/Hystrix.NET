namespace Java.Util.Concurrent.Atomic
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;

    [Serializable]
    public class AtomicIntegerArray
    {
        private readonly int[] array;

        public int Length
        {
            get
            {
                return array.Length;
            }
        }

        public AtomicIntegerArray(int length)
        {
            this.array = new int[length];
        }
        public AtomicIntegerArray(int[] array)
        {
            if (array == null) throw new ArgumentNullException("array");

            this.array = new int[array.Length];
            Array.Copy(array, 0, this.array, 0, array.Length);
        }
        public AtomicIntegerArray(IEnumerable<int> items)
        {
            if (items == null)
                throw new ArgumentNullException("items");

            this.array = items.ToArray();
        }

        public int this[int index]
        {
            get
            { 
                return Thread.VolatileRead(ref this.array[index]);
            }
            set
            {
                Thread.VolatileWrite(ref this.array[index], value);
            }
        }

        public int AddAndGet(int index, int delta)
        {
            return Interlocked.Add(ref this.array[index], delta);
        }
        public bool CompareAndSet(int index, int expect, int update)
        {
            return Interlocked.CompareExchange(ref this.array[index], update, expect) == expect;
        }
        public int DecrementAndGet(int index)
        {
            return Interlocked.Decrement(ref this.array[index]);
        }
        public int GetAndDecrement(int index)
        {
            return Interlocked.Decrement(ref this.array[index]) + 1;
        }
        public int GetAndIncrement(int index)
        {
            return Interlocked.Increment(ref this.array[index]) - 1;
        }
        public int GetAndSet(int index, int newValue)
        {
            return Interlocked.Exchange(ref this.array[index], newValue);
        }
        public int IncrementValueAndGet(int index)
        {
            return Interlocked.Increment(ref this.array[index]);
        }
        public bool WeakCompareAndSet(int index, int expect, int update)
        {
            return CompareAndSet(index, expect, update);
        }
    }
}