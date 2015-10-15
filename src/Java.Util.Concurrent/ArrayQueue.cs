#region License

/*
 * Copyright (C) 2002-2008 the original author or authors.
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

#region Imports
using System;
using System.Collections.Generic;
#endregion

namespace Java.Util
{
	/// <summary> 
	/// A bounded <see cref="IQueue{T}"/> backed by an array.  This queue orders 
	/// elements FIFO (first-in-first-out).  The <i>head</i> of the queue is 
	/// that element that has been on the queue the longest time.  The <i>tail</i> 
	/// of the queue is that element that has been on the queue the shortest time. 
	/// New elements are inserted at the tail of the queue, and the queue retrieval
	/// operations obtain elements at the head of the queue.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This is a classic &quot;bounded buffer&quot;, in which a fixed-sized array 
	/// holds elements inserted by producers and extracted by consumers.  Once 
	/// created, the capacity cannot be increased.
	/// </para>
    /// </remarks>
	/// <author>Doug Lea</author>
	/// <author>Griffin Caprio (.NET)</author>
	/// <author>Kenneth Xu</author>
	[Serializable]
	public class ArrayQueue<T> : AbstractQueue<T> //BACKPORT_2_2
	{
		/// <summary>The intial capacity of this queue.</summary>
		private readonly int _capacity;

		/// <summary>Number of items in the queue </summary>
		private int _count;

		/// <summary>The queued items  </summary>
		private readonly T[] _items;

		/// <summary>items index for next take, poll or remove </summary>
		[NonSerialized] private int _takeIndex;

		/// <summary>items index for next put, offer, or add. </summary>
		[NonSerialized] private int _putIndex;

		#region Private Methods

		/// <summary> 
		/// Utility for remove: Delete item at position <paramref name="index"/>.
		/// and return the next item position. Call only when holding lock.
		/// </summary>
		private int RemoveAt(int index)
		{
			T[] items = _items;
			if (index == _takeIndex)
			{
				items[_takeIndex] = default(T);
				_takeIndex = Increment(_takeIndex);
			    index = _takeIndex;
			}
			else
			{
                int i = index;
                for (; ; )
				{
					int nextIndex = Increment(i);
					if (nextIndex != _putIndex)
					{
						items[i] = items[nextIndex];
						i = nextIndex;
					}
					else
					{
						items[i] = default(T);
						_putIndex = i;
						break;
					}
				}
			}
			--_count;
		    return index;
		}

		/// <summary> Circularly increment i.</summary>
		private int Increment(int index)
		{
			return (++index == _items.Length) ? 0 : index;
		}

		/// <summary> 
		/// Inserts element at current put position, advances, and signals.
		/// Call only when holding lock.
		/// </summary>
		private void Insert(T x)
		{
			_items[_putIndex] = x;
			_putIndex = Increment(_putIndex);
			++_count;
		}

		/// <summary> 
		/// Extracts element at current take position, advances, and signals.
		/// Call only when holding lock.
		/// </summary>
		private T Extract()
		{
			T[] items = _items;
			T x = items[_takeIndex];
			items[_takeIndex] = default(T);
			_takeIndex = Increment(_takeIndex);
			--_count;
			return x;
		}

		#endregion

		#region Constructors

		/// <summary> 
		/// Creates an <see cref="ArrayQueue{T}"/> with the given (fixed)
		/// <paramref name="capacity"/> and initially containing the elements 
		/// of the given collection, added in traversal order of the 
		/// collection's iterator.
		/// </summary>
		/// <param name="capacity">
		/// The capacity of this queue.
		/// </param>
		/// <param name="collection">
		/// The collection of elements to initially contain.
		/// </param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// If <paramref name="capacity"/> is less than 1 or is less than the 
		/// size of <pararef name="collection"/>.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="collection"/> is <see langword="null"/>.
		/// </exception> 
		public ArrayQueue(int capacity, IEnumerable<T> collection) : this(capacity)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");
			foreach (T currentObject in collection)
			{
                if(_count >= capacity)
                {
				    throw new ArgumentOutOfRangeException(
                        "collection", collection, "Collection size greater than queue capacity");
                }
				Insert(currentObject);
			}
		}

		/// <summary> 
		/// Creates an <see cref="ArrayQueue{T}"/> with the given (fixed)
		/// <paramref name="capacity"/> and default fairness access policy.
		/// </summary>
		/// <param name="capacity">
		/// The capacity of this queue.
		/// </param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// If <paramref name="capacity"/> is less than 1.
		/// </exception>
		public ArrayQueue(int capacity)
		{
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(
                    "capacity", capacity, "Must not be negative");
            _capacity = capacity;
            _items = new T[capacity];
        }

		#endregion

        /// <summary>
        /// Clear the queue without locking, this is for subclass to provide
        /// the clear implementation without worrying about concurrency.
        /// </summary>
	    public override void Clear()
	    {
	        T[] items = _items;
	        int i = _takeIndex;
	        int k = _count;
	        while (k-- > 0)
	        {
	            items[i] = default(T);
	            i = Increment(i);
	        }
	        _count = 0;
	        _putIndex = 0;
	        _takeIndex = 0;
	    }
        /// <summary>
        /// 
        /// </summary>
	    public override int Capacity
	    {
            get { return _capacity; }
	    }

	    /// <summary> 
		/// Returns <see langword="true"/> if this queue contains the specified element.
		/// </summary>
		/// <remarks>
		/// More formally, returns <see langword="true"/> if and only if this queue contains
		/// at least one element <i>element</i> such that <i>elementToSearchFor.equals(element)</i>.
		/// </remarks>
		/// <param name="elementToSearchFor">object to be checked for containment in this queue</param>
		/// <returns> <see langword="true"/> if this queue contains the specified element</returns>
        public override bool Contains(T elementToSearchFor)
		{
		    T[] items = _items;
		    int i = _takeIndex;
		    int k = 0;
		    while (k++ < _count)
		    {
		        if (elementToSearchFor.Equals(items[i]))
		            return true;
		        i = Increment(i);
		    }
		    return false;
		}

	    /// <summary> 
		/// Removes a single instance of the specified element from this queue,
		/// if it is present.  More formally, removes an <i>element</i> such
		/// that <i>elementToRemove.Equals(element)</i>, if this queue contains one or more such
		/// elements.
		/// </summary>
		/// <param name="elementToRemove">element to be removed from this queue, if present
		/// </param>
		/// <returns> <see langword="true"/> if this queue contained the specified element or
		///  if this queue changed as a result of the call, <see langword="false"/> otherwise
		/// </returns>
        public override bool Remove(T elementToRemove)
	    {
	        T[] items = _items;

	        int currentIndex = _takeIndex;
	        int currentStep = 0;
	        for (;;)
	        {
	            if (currentStep++ >= _count)
	                return false;
	            if (elementToRemove.Equals(items[currentIndex]))
	            {
	                RemoveAt(currentIndex);
	                return true;
	            }
	            currentIndex = Increment(currentIndex);
	        }
	    }

		/// <summary>
		/// Gets the capacity of the queue.
		/// </summary>
		public override int RemainingCapacity
		{
			get { return _capacity - _count; }
		}

		/// <summary> 
		/// Returns the number of elements in this queue.
		/// </summary>
		/// <returns> the number of elements in this queue</returns>
		public override int Count
		{
            get { return _count; }
		}

		/// <summary> 
		/// Inserts the specified element into this queue if it is possible to do
		/// so immediately without violating capacity restrictions.
		/// </summary>
		/// <remarks>
		/// <p/>
		/// When using a capacity-restricted queue, this method is generally
		/// preferable to <see cref="ArgumentException"/>,
		/// which can fail to insert an element only by throwing an exception.
		/// </remarks>
		/// <param name="element">
		/// The element to add.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if the element was added to this queue.
		/// </returns>
		/// <exception cref="object">
		/// If the element cannot be added at this time due to capacity restrictions.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// If the supplied <paramref name="element"/> is <see langword="null"/> 
		/// and this queue does not permit <see langword="null"/> elements.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// If some property of the supplied <paramref name="element"/> prevents
		/// it from being added to this queue.
		/// </exception>
        public override bool Offer(T element)
		{
		    if (_count == _items.Length) return false;
		    Insert(element);
		    return true;
		}

	    /// <summary> 
		/// Retrieves and removes the head of this queue.
		/// </summary>
		/// <remarks>
		/// <p/>
		/// This method differs from <see cref="IQueue{T}.Poll"/>
		/// only in that it throws an exception if this queue is empty.
		/// </remarks>
		/// <returns> 
		/// The head of this queue
		/// </returns>
		/// <exception cref="InvalidOperationException">if this queue is empty</exception>
        public override T Remove()
	    {
	        if (_count == 0)
	            throw new InvalidOperationException("Queue is empty.");
	        T x = Extract();
	        return x;
	    }

	    /// <summary> 
		/// Retrieves and removes the head of this queue,
		/// or returns <see langword="null"/> if this queue is empty.
		/// </summary>
		/// <returns> 
		/// The head of this queue, or <see langword="null"/> if this queue is empty.
		/// </returns>
        public override bool Poll(out T element)
	    {
	        bool notEmpty = _count > 0;
	        element = notEmpty ? Extract() : default(T);
	        return notEmpty;
	    }

		/// <summary> 
		/// Retrieves, but does not remove, the head of this queue,
		/// or returns <see langword="null"/> if this queue is empty.
		/// </summary>
		/// <returns> 
		/// The head of this queue, or <see langword="null"/> if this queue is empty.
		/// </returns>
        public override bool Peek(out T element)
		{
		    bool notEmpty = (_count > 0);
		    element = notEmpty ? _items[_takeIndex] : default(T);
		    return notEmpty;
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
            T[] items = _items;

            int n = 0;
	        int reject = 0;
            int currentIndex = _takeIndex; 
            while(n < maxElements && _count > reject)
            {
                T element = items[currentIndex];
                if (criteria == null || criteria(element))
                {
                    action(element);
                    n++;
                    currentIndex = RemoveAt(currentIndex);
                }
                else
                {
                    reject++;
                    currentIndex = Increment(currentIndex);
                }
                
            }
            return n;
        }


	    /// <summary> 
		/// Returns an array of <typeparamref name="T"/> containing all of the 
		/// elements in this queue, in proper sequence.
		/// </summary>
		/// <remarks>
		/// The returned array will be "safe" in that no references to it are
		/// maintained by this queue.  (In other words, this method must allocate
		/// a new array).  The caller is thus free to modify the returned array.
		/// </remarks>
		/// <returns> an <c>T[]</c> containing all of the elements in this queue</returns>
        public override T[] ToArray()
	    {
	        T[] items = _items;
	        T[] a = new T[_count];
	        int k = 0;
	        int i = _takeIndex;
	        while (k < _count)
	        {
	            a[k++] = items[i];
	            i = Increment(i);
	        }
	        return a;
	    }

	    /// <summary> 
		/// Returns an array containing all of the elements in this queue, in
		/// proper sequence; the runtime type of the returned array is that of
		/// the specified array.  
		/// </summary>
		/// <remarks>
		/// If the queue fits in the specified array, it
		/// is returned therein.  Otherwise, a new array is allocated with the
		/// runtime type of the specified array and the size of this queue.
		/// 
		/// <p/>
		/// If this queue fits in the specified array with room to spare
		/// (i.e., the array has more elements than this queue), the element in
		/// the array immediately following the end of the queue is set to
		/// <see langword="null"/>.
		/// 
		/// <p/>
		/// Like the <see cref="ToArray()"/> method, 
		/// this method acts as bridge between
		/// array-based and collection-based APIs.  Further, this method allows
		/// precise control over the runtime type of the output array, and may,
		/// under certain circumstances, be used to save allocation costs.
		/// 
		/// <p/>
		/// Suppose <i>x</i> is a queue known to contain only strings.
		/// The following code can be used to dump the queue into a newly
		/// allocated array of <see cref="System.String"/> objects:
		/// 
		/// <code>
		///		string[] y = x.ToArray(new string[0]);
		/// </code>
		/// 
		/// Note that <see cref="ToArray(T[])"/> with an empty
		/// arry is identical in function to
		/// <see cref="ToArray()"/>.
		/// </remarks>
		/// <param name="targetArray">
		/// the array into which the elements of the queue are to
		/// be stored, if it is big enough; otherwise, a new array of the
		/// same runtime type is allocated for this purpose
		/// </param>
		/// <returns> an array containing all of the elements in this queue</returns>
		/// <exception cref="ArrayTypeMismatchException">if the runtime type of the <pararef name="targetArray"/> 
		/// is not a super tyoe of the runtime tye of every element in this queue.
		/// </exception>
		/// <exception cref="System.ArgumentNullException">If the <paramref name="targetArray"/> is <see langword="null"/>
		/// </exception>
		public override T[] ToArray(T[] targetArray)
	    {
	        if (targetArray == null)
	            throw new ArgumentNullException("targetArray");
	        T[] items = _items;

	        if (targetArray.Length < _count)
	            targetArray = (T[]) Array.CreateInstance(targetArray.GetType().GetElementType(), _count);

	        int k = 0;
	        int i = _takeIndex;
	        while (k < _count)
	        {
	            targetArray[k++] = items[i];
	            i = Increment(i);
	        }
	        if (targetArray.Length > _count)
	            targetArray[_count] = default(T);
	        return targetArray;
	    }

        /// <summary> 
        /// Returns an <see cref="IEnumerator{T}"/> over the elements in this 
        /// queue in proper sequence.
        /// </summary>
        /// <remarks>
        /// The returned <see cref="IEnumerator{T}"/> is a "weakly consistent" 
        /// enumerator that will not throw <see cref="InvalidOperationException"/> 
        /// when the queue is concurrently modified, and guarantees to traverse
        /// elements as they existed upon construction of the enumerator, and
        /// may (but is not guaranteed to) reflect any modifications subsequent
        /// to construction.
        /// </remarks>
        /// <returns>
        /// An enumerator over the elements in this queue in proper sequence.
        /// </returns>
        public override IEnumerator<T> GetEnumerator()
		{
		    return new ArrayQueueEnumerator(this);
		}

	    private class ArrayQueueEnumerator : AbstractEnumerator<T>
		{
			/// <summary> 
			/// Index of element to be returned by next,
			/// or a negative number if no such element.
			/// </summary>
			private int _nextIndex;
			/// <summary>
			/// Parent <see cref="ArrayQueue{T}"/> 
			/// for this <see cref="IEnumerator{T}"/>
			/// </summary>
			private readonly ArrayQueue<T> _queue;
			/// <summary> 
			/// nextItem holds on to item fields because once we claim
			/// that an element exists in hasNext(), we must return it in
			/// the following next() call even if it was in the process of
			/// being removed when hasNext() was called.
			/// </summary>
			private T _nextItem;

	        protected override T FetchCurrent()
			{
	            return _nextItem;
			}
				
			internal ArrayQueueEnumerator(ArrayQueue<T> queue)
			{
				_queue = queue;
				SetInitialState();
			}
			
			protected override bool GoNext()
			{
			    var index = _nextIndex;
			    if (index == -1) return false;
                _nextItem = _queue._items[index];
                index = _queue.Increment(index);
			    _nextIndex = index == _queue._putIndex ? -1 : index;
			    return true;
			}

			protected override void DoReset()
			{
				SetInitialState();
			}

			private void SetInitialState()
			{
			    _nextIndex = _queue.Count == 0 ? - 1 : _queue._takeIndex;
			}
		}
	}
}