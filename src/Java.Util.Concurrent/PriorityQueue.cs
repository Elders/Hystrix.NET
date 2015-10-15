#region License

/*
 * Copyright ?2002-2006 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using Java.Util.Concurrent;
using Java.Util.Concurrent.Helpers;

namespace Java.Util
{
    /// <summary> 
    /// An unbounded priority <see cref="IQueue{T}"/> based on a priority
    /// heap. The elements of priority queue are ordered according their
    /// <see cref="IComparable{T}">natural ordering</see>, or by an
    /// <see cref="IComparer"/> provided at queue construction time, 
    /// depending on which constructor is used. A priority queue does not 
    /// permit <c>null</c> elements. A priority queue relying on 
    /// natural ordering also does not permit insertion of non-comparable 
    /// objects (doing so will result in <see cref="InvalidCastException"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <i>head</i> of this queue is the <i>least</i> element
    /// with respect to the specified ordering.  If multiple elements are
    /// tied for least value, the head is one of those elements -- ties are
    /// broken arbitrarily. The queue retrieval operations <c>Poll</c>, 
    /// <c>Remove</c>, <c>Peek</c>, and <c>Element</c> access the element at 
    /// the head of the queue.
    /// </para>
    /// <para>
    /// A priority queue is unbounded, but has an internal
    /// <i>capacity</i> governing the size of an array used to store the
    /// elements on the queue.  It is always at least as large as the queue
    /// size.  As elements are added to a priority queue, its capacity
    /// grows automatically.  The details of the growth policy are not
    /// specified.
    /// </para>
    /// <para>
    /// This class and its enumerator implement all of the <i>optional</i> 
    /// methods of the <see cref="ICollection{T}"/> and 
    /// <see cref="IEnumerator{T}"/> interfaces. The enumerator provided in 
    /// method <see cref="IEnumerable{T}.GetEnumerator"/> is <b>not</b> 
    /// guaranteed to traverse the elements of the priority queue in any
    /// particular order. If you need ordered traversal, consider using 
    /// <c>Array.Sort(pq.ToArray())</c>.
    /// </para>
    /// <para>
    /// <b>Note that this implementation is NOT synchronized.</b> Multiple 
    /// threads should not access a <see cref="PriorityQueue{T}"/> instance 
    /// concurrently if any of the threads modifies the list structurally. 
    /// Instead, use the thread-safe <see cref="PriorityBlockingQueue{T}"/> .
    /// </para>
    /// <para>
    /// Implementation note: this implementation provides O(log(n)) time for 
    /// the enqueing and dequeing methods (<c>Offer</c>, <c>Poll</c>, 
    /// <c>Remove</c> and <c>Add</c>); linear time for the <c>Remove(T)</c> 
    /// and <c>Contains(T)</c> methods; and constant time for the retrieval 
    /// methods (<c>Peek</c>, <c>Element</c>, and <c>Count</c>).
    /// </para>
    /// </remarks>
    /// <author>Josh Bloch</author>
    /// <author>Griffin Caprio (.NET)</author>
    /// <author>Kenneth Xu</author>
    [Serializable]
    public class PriorityQueue<T> : AbstractQueue<T>, ISerializable  //JDK_1_6
    {

        #region Private Fields

        private static readonly bool _isValueType;
        private static readonly Func<T, T, bool> _areEqual;
        private static readonly IComparer<T> _defaultComparer;

        static PriorityQueue()
        {
            var isValueType = typeof(T).IsValueType;
            _isValueType = isValueType;
            if (isValueType)
                _areEqual = (x, y) => x.Equals(y);
            else
                _areEqual = (x, y) => ReferenceEquals(x, y);
            if (typeof (IComparable<T>).IsAssignableFrom(typeof (T)))
            {
                _defaultComparer = (IComparer<T>) typeof (ComparableComparer<>).MakeGenericType(typeof (T))
                    .GetField("Default").GetValue(null);
            }

        }

        private const int _defaultInitialCapacity = 11;

        /// <summary> 
        /// Priority queue represented as a balanced binary heap: the two children
        /// of queue[n] are queue[2*n+1] and queue[2*(n+1)].  The priority queue is
        /// ordered by comparator, or by the elements' natural ordering, if
        /// comparator is null:  For each node n in the heap and each descendant d
        /// of n, n &lt;= d.
        /// 
        /// The element with the lowest value is in queue[0], assuming the queue is
        /// nonempty.
        /// </summary>
        [NonSerialized]
        private T[] _queue;

        /// <summary> The number of elements in the priority queue.</summary>
        [NonSerialized]
        private int _size;

        /// <summary> 
        /// The comparator, or null if priority queue uses elements'
        /// natural ordering.
        /// </summary>
        [NonSerialized]
        private readonly IComparer<T> _comparer;

        /// <summary> 
        /// The number of times this priority queue has been
        /// <i>structurally modified</i>.
        /// </summary>
        [NonSerialized]
        private int _modCount;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a <see cref="PriorityQueue{T}"/> with the default initial 
        /// capacity (11) that orders its elements according to their natural
        /// ordering (using <see cref="IComparable"/>).
        /// </summary>
        public PriorityQueue()
            : this(_defaultInitialCapacity, _defaultComparer) {}

        /// <summary> 
        /// Creates a <see cref="PriorityQueue{T}"/> with the specified initial 
        /// capacity that orders its elements according to their natural ordering
        /// (using <see cref="IComparable"/>).
        /// </summary>
        /// <param name="initialCapacity">
        /// The initial capacity for this priority queue.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="initialCapacity"/> is less than 1.
        /// </exception>
        public PriorityQueue(int initialCapacity)
            : this(initialCapacity, _defaultComparer) { }

        /// <summary> 
        /// Creates a <see cref="PriorityQueue{T}"/> with the specified initial
        /// capacity that orders its elements according to the specified 
        /// <paramref name="comparison"/>.
        /// </summary>
        /// <param name="initialCapacity">
        /// The initial capacity for this priority queue.
        /// </param>
        /// <param name="comparison">
        /// The comparison delegate used to order this priority queue. If 
        /// <c>null</c> then the order depends on the elements' natural 
        /// ordering.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="initialCapacity"/> is less than 1.
        /// </exception>
        public PriorityQueue(int initialCapacity, Comparison<T> comparison)
            : this (initialCapacity, ComparisonComparer<T>.From(comparison)) {}

        /// <summary> 
        /// Creates a <see cref="PriorityQueue{T}"/> with the specified initial
        /// capacity that orders its elements according to the specified 
        /// <paramref name="comparer"/>.
        /// </summary>
        /// <param name="initialCapacity">
        /// The initial capacity for this priority queue.
        /// </param>
        /// <param name="comparer">
        /// The comparer used to order this priority queue. If <c>null</c>
        /// then the order depends on the elements' natural ordering.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="initialCapacity"/> is less than 1.
        /// </exception>
        public PriorityQueue(int initialCapacity, IComparer<T> comparer)
        {
            // Note: This restriction of at least one is not actually needed,
            // but for Java compatibility
            if (initialCapacity < 1) throw new ArgumentOutOfRangeException(
                "initialCapacity", initialCapacity, "Parameter value must be greater or equal to 1.");
            _queue = new T[initialCapacity];
            _comparer = comparer;
        }

        /// <summary> 
        /// Creates a <see cref="PriorityQueue{T}"/> containing the elements in
        /// the specified source.  If the specified source is another 
        /// <see cref="PriorityQueue{T}"/>, this priority queue will be ordered
        /// according to the same order.  Otherwise, the priority queue is ordered 
        /// according to its elements' <see cref="IComparable">natural order</see>.
        /// </summary>
        /// <param name="source">
        /// The collection whose elements are to be placed into this priority queue.
        /// </param>
        /// <exception cref="InvalidCastException">
        /// If elements of <paramref name="source"/> cannot be compared to 
        /// one another according to the priority queue's ordering.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="source"/> or any element with it is
        /// <c>null</c>.
        /// </exception>
        /// <exception cref="NullReferenceException">
        /// If there is <c>null</c> in source.
        /// </exception>
        public PriorityQueue(IEnumerable<T> source) {
            InitFromCollection(source);
            if(source is PriorityQueue<T>) 
            {
                _comparer = ((PriorityQueue<T>)source)._comparer;
            }
            // TODO: get comparator from any other known sorted collection.
            // else if (c instanceof SortedSet)
            // {
            //    comparator = (Comparator<? super E>) ((SortedSet<? extends E>)c).comparator();
            // }
            else
            {
                _comparer = _defaultComparer;
                Heapify();
            }
        }

        /// <summary> 
        /// Creates a <see cref="PriorityQueue{T}"/> containing the elements in
        /// the specified priority queue.  This priority queue will be ordered
        /// according to the same ordering as the given priority queue.
        /// </summary>
        /// <param name="soruce">
        /// The priority queue whose elements are to be placed into this 
        /// priority queue.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="soruce"/> is <c>null</c>.
        /// </exception>
        public PriorityQueue(PriorityQueue<T> soruce)
        {
            InitFromCollection(soruce);
            _comparer = soruce._comparer;
        }

        /// <summary>
        /// Initializes queue array with elements from the given
        /// <paramref name="source"/>.
        /// </summary>
        /// <param name="source">
        /// An <see cref="IEnumerable{T}"/> to create queue array from.
        /// </param>
        private void InitFromCollection(IEnumerable<T> source)
        {
            if (source == null) throw new ArgumentNullException("source");
            var a = source.ToArrayOptimized();
            _queue = a;
            _size = a.Length;
        }

        #endregion

        #region Private Helper Methods

        /// <summary> 
        /// Establishes the heap invariant assuming the heap
        /// satisfies the invariant except possibly for the leaf-node indexed by k
        /// (which may have a nextExecutionTime less than its parent's).
        /// </summary>
        /// <remarks>
        /// This method functions by "promoting" queue[k] up the hierarchy
        /// (by swapping it with its parent) repeatedly until queue[k]
        /// is greater than or equal to its parent.
        /// </remarks>
        private void SiftUp(int k, T element)
        {
            if (_comparer == null)
                SiftUpComparable(k, element);
            else
                SiftUpUsingComparator(k, element);
        }

        private void SiftUpComparable(int k, T x)
        {
            IComparable key = (IComparable) x;
            while (k > 0)
            {
                int parent = (k - 1) >> 1;
                T e = _queue[parent];
                if (key.CompareTo(e) >= 0)
                    break;
                _queue[k] = e;
                k = parent;
            }
            _queue[k] = x;
        }

        private void SiftUpUsingComparator(int k, T x)
        {
            while (k > 0)
            {
                int parent = (k - 1) >> 1;
                T e = _queue[parent];
                if (_comparer.Compare(x, e) >= 0)
                    break;
                _queue[k] = e;
                k = parent;
            }
            _queue[k] = x;
        }


        /// <summary> 
        /// Establishes the heap invariant (described above) in the subtree
        /// rooted at k, which is assumed to satisfy the heap invariant except
        /// possibly for node k itself (which may be greater than its children).
        /// </summary>
        /// <remarks>
        /// This method functions by "demoting" queue[k] down the hierarchy
        /// (by swapping it with its smaller child) repeatedly until queue[k]
        /// is less than or equal to its children.
        /// </remarks>
        private void SiftDown(int k, T x)
        {
            if (_comparer != null)
                SiftDownUsingComparator(k, x);
            else
                SiftDownComparable(k, x);
        }

        private void SiftDownComparable(int k, T x)
        {
            IComparable key = (IComparable) x;
            int half = _size >> 1; // loop while a non-leaf
            while (k < half)
            {
                int child = (k << 1) + 1; // assume left child is least
                T c = _queue[child];
                int right = child + 1;
                if (right < _size &&
                    ((IComparable) c).CompareTo(_queue[right]) > 0)
                    c = _queue[child = right];
                if (key.CompareTo(c) <= 0)
                    break;
                _queue[k] = c;
                k = child;
            }
            _queue[k] = x;
        }

        private void SiftDownUsingComparator(int k, T x)
        {
            int half = _size >> 1;
            while (k < half)
            {
                int child = (k << 1) + 1;
                T c = _queue[child];
                int right = child + 1;
                if (right < _size &&
                    _comparer.Compare(c, _queue[right]) > 0)
                    c = _queue[child = right];
                if (_comparer.Compare(x, c) <= 0)
                    break;
                _queue[k] = c;
                k = child;
            }
            _queue[k] = x;
        }

        /// <summary> 
        /// Establishes the heap invariant in the entire tree,
        /// assuming nothing about the order of the elements prior to the call.
        /// </summary>
        private void Heapify()
        {
            for (int i = (_size >> 1) - 1; i >= 0; i--)
                SiftDown(i, _queue[i]);
        }

        /// <summary> 
        /// Removes and returns element located at <paramref name="i"/> from queue.  (Recall that the queue
        /// is one-based, so 1 &lt;= i &lt;= size.)
        /// </summary>
        /// <remarks>
        /// Normally this method leaves the elements at positions from 1 up to i-1,
        /// inclusive, untouched.  Under these circumstances, it returns <c>null</c>.
        /// Occasionally, in order to maintain the heap invariant, it must move
        /// the last element of the list to some index in the range [2, i-1],
        /// and move the element previously at position (i/2) to position i.
        /// Under these circumstances, this method returns the element that was
        /// previously at the end of the list and is now at some position between
        /// 2 and i-1 inclusive.
        /// </remarks>
        private void RemoveAt(int i)
        {
            Debug.Assert(i >= 0 && i < _size);
            _modCount++;

            int s = --_size;

            if (s == i) // remove last element
            {
                _queue[i] = default(T);
            }
            else
            {
                var moved = _queue[s];
                _queue[s] = default(T);
                SiftDown(i, moved);
                if (_areEqual(_queue[i],moved))
                {
                    SiftUp(i, moved);
                }
            }
        }

        /// <summary>Increases the capacity of the array.</summary>
        private void Grow(int minCapacity)
        {
            if (minCapacity < 0) // overflow
                throw new OutOfMemoryException(
                    "Cannot grow queue to accomdate more then int.MaxValue elements.");

            int oldCapacity = _queue.Length;
            // Double size if small; else grow by 50%
            int newCapacity = ((oldCapacity < 64) ?
                               ((oldCapacity + 1) * 2) :
                               ((oldCapacity / 2) * 3));
            if (newCapacity < 0) // overflow
                newCapacity = Int32.MaxValue;
            if (newCapacity < minCapacity)
                newCapacity = minCapacity;

            T[] newQueue = new T[newCapacity];
            Array.Copy(_queue, 0, newQueue, 0, _queue.Length);
            _queue = newQueue;
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Return <see cref="int.MaxValue"/> as <see cref="PriorityQueue{T}"/>
        /// is unbounded.
        /// </summary>
        public override int RemainingCapacity
        {
            get { return Int32.MaxValue; }
        }

        /// <summary>
        /// Returns <see cref="int.MaxValue"/> as <see cref="PriorityQueue{T}"/>
        /// is unbounded.
        /// </summary>
        public override int Capacity
        {
            get { return int.MaxValue; }
        }

        /// <summary>
        /// Returns the queue count.
        /// </summary>
        public override int Count 
        {
            get { return _size; }
        }

        /// <summary> 
        /// Inserts the specified element into this queue if it is possible to do
        /// so immediately without violating capacity restrictions.
        /// </summary>
        /// <remarks>
        /// <p>
        /// When using a capacity-restricted queue, this method is generally
        /// preferable to <see cref="AbstractQueue{T}.Add"/>,
        /// which can fail to insert an element only by throwing an exception.
        /// </p>
        /// </remarks>
        /// <param name="element">
        /// The element to add.
        /// </param>
        /// <returns>
        /// <c>true</c> if the element was added to this queue.
        /// </returns>
        /// <exception cref="System.InvalidCastException">
        /// if the specified element cannot be compared
        /// with elements currently in the priority queue according
        /// to the priority queue's ordering.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// If the element cannot be added at this time due to capacity restrictions.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="element"/> is
        /// <c>null</c> and this queue does not permit <c>null</c>
        /// elements.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If some property of the supplied <paramref name="element"/> prevents
        /// it from being added to this queue.
        /// </exception>
        public override bool Offer(T element)
        {
            // ReSharper disable CompareNonConstrainedGenericWithNull
            if(!_isValueType && element == null) 
                throw new ArgumentNullException("element");
            // ReSharper restore CompareNonConstrainedGenericWithNull

            _modCount++;

            var i = _size;
            if (i >= _queue.Length) Grow(i + 1);
            _size = i+1;

            if (i == 0) 
                _queue[0] = element;
            else 
                SiftUp(i, element);

            return true;
        }

        /// <summary> 
        /// Retrieves, but does not remove, the head of this queue into the
        /// out parameter <paramref name="element"/>, or returns <c>false</c>
        /// if this queue is empty.
        /// </summary>
        /// <returns> 
        /// <c>true</c> when the the head of this queue is return. <c>false</c>
        /// when the queue is empty.
        /// </returns>
        public override bool Peek(out T element ) {
            if(_size == 0)
            {
                element = default(T);
                return false;
            }
            element = _queue[0];
            return true; 
        }

        private int IndexOf(T e)
        {
// ReSharper disable CompareNonConstrainedGenericWithNull
            if (_isValueType || e != null)
// ReSharper restore CompareNonConstrainedGenericWithNull
            {
                for (int i = 0; i < _size; i++)
                    if (e.Equals(_queue[i]))
                        return i;
            }
            return -1;
        }


        /// <summary> 
        /// Removes a single instance of the specified element from this
        /// queue, if it is present.
        /// </summary>
        public override bool Remove(T item)
        {
            var i = IndexOf(item);
            if (i == -1) return false;
            RemoveAt(i);
            return true;
        }

        /// <summary> 
        /// Returns an <see cref="System.Collections.IEnumerator"/> over the elements in this queue. 
        /// The enumeratoar does not return the elements in any particular order.
        /// </summary>
        /// <returns> an enumerator over the elements in this queue.</returns>
        public override IEnumerator<T> GetEnumerator()
        {
            return new PriorityQueueEnumerator(this);
        }


        /// <summary> 
        /// Removes all elements from the priority queue.
        /// The queue will be empty after this call returns.
        /// </summary>
        public override void Clear()
        {
            _modCount++;

            for(int i = 0; i < _size; i++)
                _queue[i] = default(T);

            _size = 0;
        }

        /// <summary> 
        /// Retrieves and removes the head of this queue into the out parameter
        /// <paramref name="element"/>, or returns <c>false</c> if this queue
        /// is empty.
        /// </summary>
        /// <returns> 
        /// <c>true</c> when the the head of this queue is return. <c>false</c>
        /// when the queue is empty.
        /// </returns>
        public override bool Poll(out T element) 
        {
            if(_size == 0)
            {
                element = default(T);
                return false;
            }
            int s = --_size;

            _modCount++;

            T result = _queue[0];
            var x = _queue[s];
            _queue[s] = default(T);
            if(s != 0)
                SiftDown(0, x);
            element = result;
            return true;
        }
        /// <summary>
        /// Queries the queue to see if it contains the specified <paramref name="item"/>
        /// </summary>
        /// <param name="item">element to look for.</param>
        /// <returns><c>true</c> if the queue contains the <paramref name="item"/>, 
        /// <c>false</c> otherwise.</returns>
        public override bool Contains(T item)
        {
            return IndexOf(item) != -1;
        }

        /// <summary> 
        /// Does the real work for all drain methods. Caller must
        /// guarantee the <paramref name="action"/> is not <c>null</c> and
        /// <paramref name="maxElements"/> is greater then zero (0).
        /// </summary>
        /// <seealso cref="IQueue{T}.Drain(System.Action{T})"/>
        /// <seealso cref="IQueue{T}.Drain(System.Action{T}, int)"/>
        /// <seealso cref="IQueue{T}.Drain(System.Action{T}, Predicate{T})"/>
        /// <seealso cref="IQueue{T}.Drain(System.Action{T}, int, Predicate{T})"/>
        internal protected override int DoDrain(Action<T> action, int maxElements, Predicate<T> criteria)
        {
            return Drain(action, maxElements, criteria, null);
        }

        /// <summary> 
        /// Removes at most the given number of available elements that meet 
        /// the criteria defined by <paramref name="selectCriteria"/> from this 
        /// queue and invoke the given <paramref name="action"/> on each 
        /// element in order until the <paramref name="stopCriteria"/> is met.
        /// </summary>
        /// <remarks>
        /// This operation may be more efficient than repeatedly polling this 
        /// queue.  A failure encountered while attempting to invoke the 
        /// <paramref name="action"/> on the elements may result in elements 
        /// being neither, either or both in the queue or processed when the 
        /// associated exception is thrown.
        /// </remarks>
        /// <param name="action">The action to performe on each element.</param>
        /// <param name="maxElements">the maximum number of elements to transfer</param>
        /// <param name="selectCriteria">The criteria to filter the elements.</param>
        /// <param name="stopCriteria">The criteria to stop drain</param>
        /// <returns>The number of elements processed.</returns>
        /// <exception cref="System.InvalidOperationException">
        /// If the queue cannot be drained at this time.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the specified action is <see langword="null"/>.
        /// </exception>
        public virtual int Drain(Action<T> action, int maxElements, Predicate<T> selectCriteria, Predicate<T> stopCriteria)
        {
            if (_comparer==null)
                Array.Sort(_queue, 0, _size);
            else
                Array.Sort(_queue, 0, _size, _comparer);

            int n = 0;
            int i;
            for (i = 0; n < maxElements && i < _size; i++ )
            {
                T element = _queue[i];
                if (stopCriteria != null && stopCriteria(element)) break;

                if (selectCriteria == null || selectCriteria(element))
                {
                    action(element);
                    n++;
                }
                else if (n > 0)
                {
                    _queue[i - n] = element;
                }
            }
            if (n>0)
            {
                for (int j = i; j < _size; j++)
                {
                    _queue[j - n] = _queue[j];
                }
                _size -= n;
            }
            return n;

        }


        /// <summary> Returns the comparator used to order this collection, or <c>null</c>
        /// if this collection is sorted according to its elements natural ordering
        /// (using <see cref="System.IComparable"/>).
        /// 
        /// </summary>
        /// <returns> the comparator used to order this collection, or <c>null</c>
        /// if this collection is sorted according to its elements natural ordering.
        /// </returns>
        public virtual IComparer<T> Comparer
        {
            get { return _comparer == _defaultComparer ? null : _comparer; }
        }

        #endregion

        #region ISerializable Implementation
        /// <summary> 
        /// Save the state of the instance to a stream (that
        /// is, serialize it).
        /// </summary>
        /// <serialData> The length of the array backing the instance is
        /// emitted (int), followed by all of its elements (each an
        /// <see cref="System.Object"/>) in the proper order.
        /// </serialData>
        /// <param name="serializationInfo">the stream</param>
        /// <param name="context">the context</param>
        public virtual void GetObjectData(SerializationInfo serializationInfo, StreamingContext context) {
            SerializationUtilities.DefaultWriteObject(serializationInfo, context, this);

            // Write out array length
            serializationInfo.AddValue("Length", _queue.Length);

            // Write out all elements in the proper order.
            serializationInfo.AddValue("Data", ToArray());

            // Writer out the comparer if not the _defaultComparer.
            serializationInfo.AddValue("Comparer", Comparer);
        }

        /// <summary> 
        /// Reconstitute the <see cref="PriorityQueue{T}"/> instance from a stream (that is,
        /// deserialize it).
        /// </summary>
        /// <param name="serializationInfo">the stream</param>
        /// <param name="context">the context</param>
        protected PriorityQueue(SerializationInfo serializationInfo, StreamingContext context) {
            SerializationUtilities.DefaultReadObject(serializationInfo, context, this);

            int arrayLength = serializationInfo.GetInt32("Length");
            _queue = new T[arrayLength];

            var array = (T[]) serializationInfo.GetValue("Data", typeof(T[]));
            Array.Copy(array, 0, _queue, 0, array.Length);
            _size = array.Length;

            var comparer = (IComparer<T>) serializationInfo.GetValue("Comparer", typeof (IComparer<T>));
            _comparer = comparer?? _defaultComparer;
        }
        #endregion

        #region ICollection Implementation

        /// <summary>
        /// Does the actual work of copying to array.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the 
        /// destination of the elements copied from <see cref="ICollection{T}"/>. 
        /// The <see cref="Array"/> must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">
        /// The zero-based index in array at which copying begins.
        /// </param>
        /// <param name="ensureCapacity">
        /// If is <c>true</c>, calls <see cref="AbstractCollection{T}.EnsureCapacity"/>
        /// </param>
        /// <returns>
        /// A new array of same runtime type as <paramref name="array"/> if 
        /// <paramref name="array"/> is too small to hold all elements and 
        /// <paramref name="ensureCapacity"/> is <c>false</c>. Otherwise
        /// the <paramref name="array"/> instance itself.
        /// </returns>
        protected override T[] DoCopyTo(T[] array, int arrayIndex, bool ensureCapacity)
        {
            if (ensureCapacity) array = EnsureCapacity(array, Count);
            Array.Copy(_queue, 0, array, arrayIndex, _size);
            return array;
        }

        #endregion

        private class PriorityQueueEnumerator : AbstractEnumerator<T>
        {
            private readonly PriorityQueue<T> _parent;
            private int _expectedModCount;
            private int _cursor = -1;
            private T _current;

            public PriorityQueueEnumerator(PriorityQueue<T> parent)
            {
                _parent = parent;
                _expectedModCount = parent._modCount;
                _cursor = -1;
            }

            protected override bool GoNext()
            {
                CheckChanges();
                if (++_cursor >= _parent._size) return false;
                _current = _parent._queue[_cursor];
                return true;
            }

            protected override void DoReset()
            {
                CheckChanges();
                _cursor = -1;
            }

            private void CheckChanges()
            {
                if (_expectedModCount != _parent._modCount)
                    throw new InvalidOperationException("PriorityQueue is modified.");
            }

            protected override T FetchCurrent()
            {
                return _current;
            }
        }
    }
}
