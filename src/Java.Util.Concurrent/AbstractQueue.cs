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
	/// This class provides skeletal implementations for some of <see cref="IQueue{T}"/>
	/// operations.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The methods <see cref="Add"/>, <see cref="Remove"/>, and
	/// <see cref="Element()"/> are based on the <see cref="Offer"/>,
	/// <see cref="Poll"/>, and <see cref="Peek"/> methods respectively but 
	/// throw exceptions instead of indicating failure via
	/// <see langword="false"/> returns.
	/// </para>
	/// </remarks>
	/// <author>Doug Lea</author>
	/// <author>Griffin Caprio (.NET)</author>
	/// <author>Kenneth Xu</author>
	[Serializable]
    public abstract class AbstractQueue<T> : AbstractCollection<T>, IQueue<T> //JDK_1_6
	{
	    /// <summary>
        /// Returns the remaining capacity of this queue.
        /// </summary>
        public abstract int RemainingCapacity { get; }

        #region IQueue<T> Members

	    /// <summary>
	    /// Inserts the specified element into this queue if it is possible to 
	    /// do so immediately without violating capacity restrictions. 
	    /// </summary>
	    /// <remarks>
	    /// When using a capacity-restricted queue, this method is generally 
	    /// preferable to <see cref="Add"/>, which can fail to 
	    /// insert an element only by throwing an exception. 
	    /// </remarks>
	    /// <param name="element">The element to add.</param>
	    /// <returns>
	    /// <c>true</c> if the element was added to this queue. Otherwise 
	    /// <c>false</c>.
	    /// </returns>
	    /// <exception cref="ArgumentNullException">
	    /// If the <paramref name="element"/> is <c>null</c> and the queue 
	    /// implementation doesn't allow <c>null</c>.
	    /// </exception>
	    /// <exception cref="ArgumentException">
	    /// If some property of the supplied <paramref name="element"/> 
	    /// prevents it from being added to this queue. 
	    /// </exception>
	    public abstract bool Offer(T element);

	    /// <summary>
	    /// Retrieves, but does not remove, the head of this queue. 
	    /// </summary>
	    /// <remarks>
	    /// <para>
	    /// This method differs from <see cref="Peek(out T)"/> in that it throws an 
	    /// exception if this queue is empty. 
	    /// </para>
	    /// <para>
        /// this implementation returns the result of <see cref="Peek"/> 
        /// unless the queue is empty.
	    /// </para>
	    /// </remarks>
	    /// <returns>The head of this queue.</returns>
	    /// <exception cref="InvalidOperationException">
	    /// If this queue is empty.
	    /// </exception>
	    public virtual T Element()
        {
            T element;
            if (Peek(out element))
            {
                return element;
            }
            else
            {
                throw new InvalidOperationException("Queue is empty.");
            }
        }

	    /// <summary>
	    /// Retrieves, but does not remove, the head of this queue into out
	    /// parameter <paramref name="element"/>.
	    /// </summary>
	    /// <param name="element">
	    /// The head of this queue. <c>default(T)</c> if queue is empty.
	    /// </param>
	    /// <returns>
	    /// <c>false</c> is the queue is empty. Otherwise <c>true</c>.
	    /// </returns>
	    public abstract bool Peek(out T element);

	    /// <summary>
	    /// Retrieves and removes the head of this queue. 
	    /// </summary>
	    /// <returns>The head of this queue</returns>
	    /// <exception cref="InvalidOperationException">
	    /// If this queue is empty.
	    /// </exception>
	    public virtual T Remove()
        {
            T element;
            if (Poll(out element))
            {
                return element;
            }
            else
            {
                throw new InvalidOperationException("Queue is empty.");
            }
        }

	    /// <summary>
	    /// Retrieves and removes the head of this queue into out parameter
	    /// <paramref name="element"/>. 
	    /// </summary>
	    /// <param name="element">
	    /// Set to the head of this queue. <c>default(T)</c> if queue is empty.
	    /// </param>
	    /// <returns>
	    /// <c>false</c> if the queue is empty. Otherwise <c>true</c>.
	    /// </returns>
	    public abstract bool Poll(out T element);

        #endregion

        #region ICollection<T> Members

	    /// <summary>
        /// Inserts the specified <paramref name="element"/> into this queue 
        /// if it is possible to do so immediately without violating capacity 
        /// restrictions. Throws an <see cref="InvalidOperationException"/> 
        /// if no space is currently available.
	    /// </summary>
        /// <param name="element">The element to add.</param>
        /// <exception cref="InvalidOperationException">
        /// If the <paramref name="element"/> cannot be added at this time due 
        /// to capacity restrictions. 
        /// </exception>
	    public override void Add(T element)
        {
            if (!Offer(element))
            {
                throw new InvalidOperationException("Queue full.");
            }
        }

	    /// <summary>
	    /// Removes all items from the queue.
	    /// </summary>
	    /// <remarks>
	    /// This implementation repeatedly calls the <see cref="Poll"/> moethod
	    /// until it returns <c>false</c>.
	    /// </remarks>
	    public override void Clear()
        {
            T element;
            while (Poll(out element)) {}
        }

        #endregion

        #region IQueue Members

        /// <summary>
        /// Returns <see langword="true"/> if there are no elements in the 
        /// <see cref="IQueue{T}"/>, <see langword="false"/> otherwise.
        /// </summary>
        public virtual bool IsEmpty
        {
            get { return Count==0; }
        }

        /// <summary>
        /// Returns the current capacity of this queue.
        /// </summary>
        public abstract int Capacity { get; }

	    /// <summary>
	    /// Gets a value indicating whether the <see cref="ICollection{T}"/> is read-only.
	    /// This implementation always return true;
	    /// </summary>
	    /// 
	    /// <returns>
	    /// true if the <see cref="ICollection{T}"/> is read-only; otherwise, false.
	    /// This implementation always return false as typically a queue should not
	    /// be read only.
	    /// </returns>
	    /// 
	    public override bool IsReadOnly
	    {
	        get
	        {
	            return false;
	        }
	    }

        #endregion

	    /// <summary> 
	    /// Removes all available elements from this queue and invoke the given
	    /// <paramref name="action"/> on each element in order.
	    /// </summary>
	    /// <remarks>
	    /// This operation may be more efficient than repeatedly polling this 
	    /// queue.  A failure encountered while attempting to invoke the 
	    /// <paramref name="action"/> on the elements may result in elements 
	    /// being neither, either or both in the queue or processed when the 
	    /// associated exception is thrown.
	    /// <example> Drain to a non-generic list.
	    /// <code language="c#">
	    /// IList c = ...;
	    /// int count = Drain(delegate(T e) {c.Add(e);});
	    /// </code>
	    /// </example>
	    /// </remarks>
	    /// <param name="action">The action to performe on each element.</param>
	    /// <returns>The number of elements processed.</returns>
	    /// <exception cref="System.InvalidOperationException">
	    /// If the queue cannot be drained at this time.
	    /// </exception>
	    /// <exception cref="System.ArgumentNullException">
	    /// If the specified action is <see langword="null"/>.
	    /// </exception>
	    /// <seealso cref="IQueue{T}.Drain(Action{T}, int)"/>
	    public virtual int Drain(Action<T> action)
	    {
	        return Drain(action, null);
	    }

        /// <summary> 
        /// Removes all elements that pass the given <paramref name="criteria"/> 
        /// from this queue and invoke the given <paramref name="action"/> on 
        /// each element in order.
        /// </summary>
        /// <remarks>
        /// This operation may be more efficient than repeatedly polling this 
        /// queue.  A failure encountered while attempting to invoke the 
        /// <paramref name="action"/> on the elements may result in elements 
        /// being neither, either or both in the queue or processed when the 
        /// associated exception is thrown.
        /// <example> Drain to a non-generic list.
        /// <code language="c#">
        /// IList c = ...;
        /// int count = Drain(delegate(T e) {c.Add(e);});
        /// </code>
        /// </example>
        /// </remarks>
        /// <param name="action">The action to performe on each element.</param>
        /// <param name="criteria">
        /// The criteria to select the elements. <c>null</c> selects any element.
        /// </param>
        /// <returns>The number of elements processed.</returns>
        /// <exception cref="System.InvalidOperationException">
        /// If the queue cannot be drained at this time.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the specified action is <see langword="null"/>.
        /// </exception>
        /// <seealso cref="IQueue{T}.Drain(Action{T}, int)"/>
        public virtual int Drain(Action<T> action, Predicate<T> criteria)
	    {
	        if (action == null) throw new ArgumentNullException("action");
	        return DoDrain(action, criteria);
	    }

	    /// <summary> 
	    /// Removes at most the given number of available elements from this 
	    /// queue and invoke the given <paramref name="action"/> on each 
	    /// element in order.
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
	    /// <returns>The number of elements processed.</returns>
	    /// <exception cref="System.InvalidOperationException">
	    /// If the queue cannot be drained at this time.
	    /// </exception>
	    /// <exception cref="System.ArgumentNullException">
	    /// If the specified action is <see langword="null"/>.
	    /// </exception>
	    /// <seealso cref="IQueue{T}.Drain(System.Action{T})"/>
	    public virtual int Drain(Action<T> action, int maxElements)
	    {
	        return Drain(action, maxElements, null);
	    }

        /// <summary> 
        /// Removes at most the given number of elements that pass the given 
        /// <paramref name="criteria"/>from this queue and invoke the given 
        /// <paramref name="action"/> on each element in order.
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
        /// <param name="criteria">
        /// The criteria to select the elements. <c>null</c> selects any element.
        /// </param>
        /// <returns>The number of elements processed.</returns>
        /// <exception cref="System.InvalidOperationException">
        /// If the queue cannot be drained at this time.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the specified action is <see langword="null"/>.
        /// </exception>
        /// <seealso cref="IQueue{T}.Drain(System.Action{T})"/>
        public virtual int Drain(Action<T> action, int maxElements, Predicate<T> criteria)
	    {
	        if (action == null) throw new ArgumentNullException("action");
	        if (maxElements <= 0) return 0;
	        return DoDrain(action, maxElements, criteria);
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
	    internal protected abstract int DoDrain(Action<T> action, int maxElements, Predicate<T> criteria);

	    /// <summary>
	    /// Does the real work for the <see cref="AbstractQueue{T}.Drain(System.Action{T})"/>
        /// and <see cref="AbstractQueue{T}.Drain(System.Action{T},Predicate{T})"/>.
	    /// </summary>
	    internal protected virtual int DoDrain(Action<T> action, Predicate<T> criteria)
	    {
	        return DoDrain(action, int.MaxValue, criteria);
	    }
	}
}