using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;
using Java.Util.Concurrent.Helpers;

namespace Java.Util.Concurrent
{
	/// <summary>
	/// An unbounded <see cref="IBlockingQueue{T}"/> of <see cref="IDelayed"/>
	/// elements, in which an element can only be taken when its delay has expired.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The <b>head</b> of the queue is that <see cref="IDelayed"/> element whose
	/// delay expired furthest in the past.  If no delay has expired there is no
	/// head and <see cref="Poll(out T)"/> will return <c>false</c>. Expiration
	/// occurs when an element's <see cref="IDelayed.GetRemainingDelay"/> method
	/// returns a value less then or equals to zero.  Even though unexpired elements
	/// cannot be removed using <see cref="Take"/> or <see cref="Poll(out T)"/>,
    /// they are otherwise treated as normal elements. For example, the 
    /// <see cref="Count"/> property returns the count of both expired and unexpired
    /// elements. This queue does not permit <c>null</c> elements.
	/// </para>
	/// <para>
	/// This class implement all of the <i>optional</i> methods of the 
	/// <see cref="ICollection{T}"/> interfaces.
	/// </para>
	/// </remarks>
	/// <typeparam name="T">
	/// The type of elements that implements <see cref="IDelayed"/>.
	/// </typeparam>
	/// <author>Doug Lea</author>
	/// <author>Griffin Caprio (.NET)</author>
	/// <author>Kenneth Xu</author>
	[Serializable]
    public class DelayQueue<T> : AbstractBlockingQueue<T>, IDeserializationCallback // BACKPORT_3_1
        where T : IDelayed
    {
        [NonSerialized] private object _lock = new object();

        private readonly PriorityQueue<T> _queue;

        /// <summary>
        /// Creates a new, empty <see cref="DelayQueue{T}"/>.
        /// </summary>
        public DelayQueue()
        {
            _queue = new PriorityQueue<T>();
        }

	    /// <summary>
        /// Creates a <see cref="DelayQueue{T}"/> initially containing the
        /// elements of the given collection of <see cref="IDelayed"/>
        /// instances specified by parameter <paramref name="source"/>.
        /// </summary>
        /// <param name="source">
        /// Collection of elements to populate queue with.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If the collection is <c>null</c>.
        /// </exception>
        /// <exception cref="NullReferenceException">
        /// If any of the elements of the collection are <c>null</c>.
        /// </exception>
        public DelayQueue(IEnumerable<T> source)
        {
            _queue = new PriorityQueue<T>(source);
        }

	    /// <summary>
	    /// Returns the capacity of this queue. Since this is a unbounded queue, <see cref="int.MaxValue"/> is returned.
	    /// </summary>
	    public override int Capacity
	    {
	        get { return Int32.MaxValue; }
	    }

	    /// <summary> 
	    /// <see cref="DelayQueue{T}"/> is unbounded so this always
	    /// return <see cref="int.MaxValue"/>.
	    /// </summary>
	    /// <returns><see cref="int.MaxValue"/></returns>
	    public override int RemainingCapacity
	    {
	        get { return Int32.MaxValue; }
	    }

	    /// <summary>
        /// Inserts the specified element into this delay queue.
        /// </summary>
        /// <param name="element">The element to add.</param>
        /// <returns>Always <c>true</c></returns>
        /// <exception cref="NullReferenceException">
        /// If the specified element is <c>null</c>.
        /// </exception>
        public override bool Offer(T element)
        {
            lock (_lock)
            {
                T first;
                bool emptyBeforeOffer = !_queue.Peek(out first);
                _queue.Offer(element);
                if (emptyBeforeOffer || element.CompareTo(first) < 0)
                {
                    Monitor.PulseAll(_lock);
                }
                return true;
            }
        }

	    /// <summary>
	    ///	Inserts the specified element into this delay queue. As the queue is
	    ///	unbounded this method will never block.
	    /// </summary>
	    /// <param name="element">Element to add.</param>
	    /// <exception cref="NullReferenceException">
	    /// If the element is <c>null</c>.
	    /// </exception>
	    public override void Put(T element)
	    {
	        Offer(element);
	    }

	    /// <summary> 
        /// Inserts the specified element into this delay queue. As the queue
        /// is unbounded this method will never block.
        /// </summary>
        /// <param name="element">The element to add.</param>
        /// <param name="duration">
        /// This parameter is ignored as this method never blocks.
        /// </param>
        /// <returns>Always <c>true</c>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If the specified element is <c>null</c>.
        /// </exception>
        public override bool Offer(T element, TimeSpan duration)
        {
            return Offer(element);
        }

	    /// <summary> 
        /// Retrieves and removes the head of this queue, or returns 
        /// <c>false</c> if this has queue no elements with an expired delay.
        /// </summary>
        /// <param name="element">
        /// Set to the elemented retrieved from the queue if the return value
        /// is <c>true</c>. Otherwise, set to <c>default(T)</c>.
        /// </param>
        /// <returns> 
        /// <c>false</c> if this queue has no elements with an expired delay.
        /// Otherwise <c>true</c>.
        /// </returns>
        public override bool Poll(out T element)
        {
            lock (_lock)
            {
                T first;
                if (!_queue.Peek(out first) || first.GetRemainingDelay().Ticks > 0)
                {
                    element = default(T);
                    return false;
                }
                T x;
                bool hasOne = _queue.Poll(out x);
                Debug.Assert(hasOne);
                if (_queue.Count != 0)
                {
                    Monitor.PulseAll(_lock);
                }
                element = x;
                return true;
            }
        }

	    /// <summary> 
	    /// Retrieves and removes the head of this queue, waiting if necessary
	    /// until an element with an expired delay is available on this queue.
	    /// </summary>
	    /// <returns>The head of this queue.</returns>
	    /// <exception cref="ThreadInterruptedException">
	    /// If thread is interruped when waiting.
	    /// </exception>
	    public override T Take()
	    {
	        lock (_lock)
	        {
	            for (; ; )
	            {
	                T first;
	                if (!_queue.Peek(out first))
	                {
	                    Monitor.Wait(_lock);
	                }
	                else
	                {
	                    TimeSpan delay = first.GetRemainingDelay();
	                    if (delay.Ticks > 0)
	                    {
	                        Monitor.Wait(_lock, delay);
	                    }
	                    else
	                    {
	                        T x;
	                        bool hasOne = _queue.Poll(out x);
	                        Debug.Assert(hasOne);
	                        if (_queue.Count != 0)
	                        {
	                            Monitor.PulseAll(_lock);
	                        }
	                        return x;
	                    }
	                }
	            }
	        }
	    }

	    /// <summary> 
        /// Retrieves and removes the head of this queue, waiting if necessary
        /// until an element with an expired delay is available on this queue,
        /// or the specified wait time expires.
        /// </summary>
        /// <param name="duration">How long to wait before giving up.</param>
        /// <param name="element">
        /// Set to the head of this queue, or <c>default(T)</c> if the specified
        /// waiting time elapses before an element with an expired delay becomes
        /// available
        /// </param>
        /// <returns> 
        /// <c>false</c> if the specified waiting time elapses before an element
        /// is available. Otherwise <c>true</c>.
        /// </returns>
        public override bool Poll(TimeSpan duration, out T element)
        {
            lock (_lock)
            {
                DateTime deadline = WaitTime.Deadline(duration);
                for (; ; )
                {
                    T first;
                    if (!_queue.Peek(out first))
                    {
                        if (duration.Ticks <= 0)
                        {
                            element = default(T);
                            return false;
                        }
                        Monitor.Wait(_lock, WaitTime.Cap(duration));
                        duration = deadline.Subtract(DateTime.UtcNow);
                    }
                    else
                    {
                        TimeSpan delay = first.GetRemainingDelay();
                        if (delay.Ticks > 0)
                        {
                            if (duration.Ticks <= 0)
                            {
                                element = default(T);
                                return false;
                            }
                            if (delay > duration)
                            {
                                delay = duration;
                            }
                            Monitor.Wait(_lock, WaitTime.Cap(delay));
                            duration = deadline.Subtract(DateTime.UtcNow);
                        }
                        else
                        {
                            T x;
                            bool hasOne = _queue.Poll(out x);
                            Debug.Assert(hasOne);
                            if (_queue.Count != 0)
                            {
                                Monitor.PulseAll(_lock);
                            }
                            element = x;
                            return true;
                        }
                    }
                }
            }
        }

	    /// <summary>
	    /// Retrieves, but does not remove, the head of this queue into out
	    /// parameter <paramref name="element"/>. Unlike <see cref="Poll(out T)"/>,
	    /// if no expired elements are available in the queue, this method returns
	    /// the element that will expire next, if one exists.
	    /// </summary>
	    /// <param name="element">
	    /// The head of this queue. <c>default(T)</c> if queue is empty.
	    /// </param>
	    /// <returns>
	    /// <c>false</c> is the queue is empty. Otherwise <c>true</c>.
	    /// </returns>
	    public override bool Peek(out T element)
	    {
	        lock (_lock)
	        {
	            return _queue.Peek(out element);
	        }
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
            lock (_lock)
            {
                int n = _queue.Drain(action, maxElements, criteria, (e => e.GetRemainingDelay().Ticks > 0) );
                if (n > 0)
                {
                    Monitor.PulseAll(_lock);
                }
                return n;
            }
        }

        #region ICollection Members

        /// <summary>
        /// Returns the current number of elements in this queue.
        /// </summary>
        public override int Count
        {
            get
            {
                lock (_lock)
                {
                    return _queue.Count;
                }
            }
        }

        /// <summary> 
        /// Returns an enumerator over all the elements (both expired and
        /// unexpired) in this queue. The enumerator does not return the
        /// elements in any particular order.
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
        /// An enumerator over the elements in this queue.
        /// </returns>
        public override IEnumerator<T> GetEnumerator()
	    {
	        return new ToArrayEnumerator<T>(_queue);
	    }

	    /// <summary>
        /// When implemented by a class, copies the elements of the ICollection to an Array, starting at a particular Array index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied from ICollection. The Array must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins. </param>
        protected override void CopyTo(Array array, int index)
        {
            lock (_lock)
            {
                ((ICollection)_queue).CopyTo(array, index);
            }
        }

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
            lock (_lock)
            {
                if (ensureCapacity) array = EnsureCapacity(array, Count);
                _queue.CopyTo(array, arrayIndex);
                return array;
            }
        }

        /// <summary> 
        /// Removes all of the elements from this queue.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The queue will be empty after this call returns.
        /// </para>
        /// </remarks>
        public override void Clear()
        {
            lock (_lock)
            {
                _queue.Clear();
            }
        }

        /// <summary>
        /// Removes a single instance of the specified element from this
        /// queue, if it is present, whether or not it has expired.
        /// </summary>
        /// <param name="element">element to remove</param>
        /// <returns><c>true</c> if element was remove, <c>false</c> if not.</returns>
        public override bool Remove(T element) {
            lock (_lock)
            {
                return _queue.Remove(element);
            }
        }

        #endregion

        #region IDeserializationCallback Members

        void IDeserializationCallback.OnDeserialization(object sender)
        {
            _lock = new object();
        }

        #endregion
    }
}