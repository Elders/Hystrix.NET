using System;
using System.Collections.Generic;

namespace Java.Util.Concurrent
{
    internal struct EnumerableToArrayBuffer<T>
    {
        private readonly T[] _items;
        private readonly int _count;
        private readonly ICollection<T> _collection;
            
        internal int Count
        {
            get { return _collection == null ? _count : _collection.Count; }
        }

        internal EnumerableToArrayBuffer(IEnumerable<T> source)
        {
            T[] array = null;
            int length = 0;
            _collection = source as ICollection<T>;
            if (_collection != null)
            {
                _items = null;
                _count = 0;
                return;
            }
            foreach (T local in source)
            {
                if (array == null)
                {
                    array = new T[4];
                }
                else if (array.Length == length)
                {
                    T[] destinationArray = new T[length * 2];
                    Array.Copy(array, 0, destinationArray, 0, length);
                    array = destinationArray;
                }
                array[length] = local;
                length++;
            }
            _items = array;
            _count = length;
        }

        internal T[] ToArray()
        {
            var count = Count;
            if (count == 0)
            {
                return new T[0];
            }
            T[] destinationArray;
            if (_collection == null)
            {
                if (_items.Length == _count)
                {
                    return _items;
                }
                destinationArray = new T[_count];
                Array.Copy(_items, 0, destinationArray, 0, _count);
                return destinationArray;
            }
            var list = _collection as List<T>;
            if (list != null) return list.ToArray();

            var ac = _collection as AbstractCollection<T>;
            if (ac != null) return ac.ToArray();

            destinationArray = new T[count];
            _collection.CopyTo(destinationArray, 0);
            return destinationArray;
        }

        /// <summary>
        /// Caller to guarantee items.Length > index >= 0
        /// </summary>
        internal void CopyTo(T[] items, int index)
        {
            if (_collection != null && _collection.Count > 0)
                _collection.CopyTo(items, index);
            else if(_count > 0)
                Array.Copy(_items, 0, items, index, _count);
        }
    }
}