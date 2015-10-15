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
    /// A collection designed for holding elements prior to processing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Besides basic <see cref="ICollection{T}"/> operations, queues provide 
    /// additional insertion, extraction, and inspection operations. 
    /// </para>
    /// <para>
    /// Each of these methods exists in two forms: one throws an exception if 
    /// the operation fails, the other returns a special value. The latter 
    /// form of the insert operation is designed specifically for use with 
    /// capacity-restricted <see cref="IQueue{T}"/> implementations; in most 
    /// implementations, insert operations cannot fail. 
    /// </para>
    /// <para>
    /// Queues typically, but do not necessarily, order elements in a FIFO 
    /// (first-in-first-out) manner. Among the exceptions are priority queues, 
    /// which order elements according to a supplied comparator, or the 
    /// elements' natural ordering, and LIFO queues (or stacks) which order 
    /// the elements LIFO (last-in-first-out). Whatever the ordering used,
    ///  the head of the queue is that element which would be removed by a call 
    /// to <see cref="Remove"/> or <see cref="Poll"/>. In a FIFO queue, all new 
    /// elements are inserted at the tail of the queue. Other kinds of queues 
    /// may use different placement rules. Every <see cref="IQueue{T}"/> 
    /// implementation must specify its ordering properties. 
    /// </para>
    /// <para>
    /// The <see cref="Offer"/> method inserts an element if possible, 
    /// otherwise returning <c>false</c>. This differs from the 
    /// <see cref="ICollection{T}.Add"/> method, which can fail to add an 
    /// element only by throwing an exception. The <see cref="Offer"/> method 
    /// is designed for use when failure is a normal, rather than exceptional 
    /// occurrence, for example, in fixed-capacity (or "bounded") queues. 
    /// </para>
    /// <para>
    /// The <see cref="Remove"/> and <see cref="Poll"/> methods remove and 
    /// return the head of the queue. Exactly which element is removed from 
    /// the queue is a function of the queue's ordering policy, which differs 
    /// from implementation to implementation. The <see cref="Remove"/> and Poll
    /// <see cref="Poll"/> methods differ only in their behavior when the queue 
    /// is empty: the <see cref="Remove"/> method throws an exception, while 
    /// the <see cref="Poll"/> method returns <c>false</c>. 
    /// </para>
    /// <para>
    /// The <see cref="Element"/> and <see cref="Peek"/> methods return, but do 
    /// not remove, the head of the queue. 
    /// </para>
    /// <para>
    /// The <see cref="IQueue{T}"/> interface does not define the blocking queue 
    /// methods, which are common in concurrent programming. 
    /// </para>
    /// <para>
    /// <see cref="IQueue{T}"/> implementations generally do not define 
    /// element-based versions of methods <see cref="object.Equals(object)"/> 
    /// and  <see cref="object.GetHashCode"/>, but instead inherit the identity 
    /// based versions from the class object, because element-based equality is 
    /// not always well-defined for queues with the same elements but different 
    /// ordering properties. 
    /// </para>
    /// <para>
    /// Based on the back port of JCP JSR-166.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">The type of the elements in the queue.</typeparam>
    /// <author>Doug Lea</author>
    /// <author>Griffin Caprio (.NET)</author>
    /// <author>Kenneth Xu</author>
    public interface IQueue<T> : ICollection<T> // JDK_1_6
    {
        /// <summary>
        /// Gets the remaining capacity of a bounded queue or
        /// <see cref="int.MaxValue"/> if the queue is un-bounded.
        /// </summary>
        int RemainingCapacity { get; }

        /// <summary>
        /// Retrieves, but does not remove, the head of this queue. 
        /// </summary>
        /// <remarks>
        /// This method differs from <see cref="Peek(out T)"/> in that it throws an 
        /// exception if this queue is empty. 
        /// </remarks>
        /// <returns>The head of this queue.</returns>
        /// <exception cref="InvalidOperationException">
        /// If this queue is empty.
        /// </exception>
        T Element();

        /// <summary>
        /// Inserts the specified element into this queue if it is possible to 
        /// do so immediately without violating capacity restrictions. 
        /// </summary>
        /// <remarks>
        /// When using a capacity-restricted queue, this method is generally 
        /// preferable to <see cref="ICollection{T}.Add"/>, which can fail to 
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
        bool Offer(T element);

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
        bool Peek(out T element);

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
        bool Poll(out T element);

        /// <summary>
        /// Retrieves and removes the head of this queue. 
        /// </summary>
        /// <returns>The head of this queue</returns>
        /// <exception cref="InvalidOperationException">
        /// If this queue is empty.
        /// </exception>
        T Remove();

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
        /// <seealso cref="IBlockingQueue{T}.DrainTo(System.Collections.Generic.ICollection{T})"/>
        /// <seealso cref="IBlockingQueue{T}.DrainTo(System.Collections.Generic.ICollection{T},int)"/>
        /// <seealso cref="Drain(System.Action{T},int)"/>
        int Drain(Action<T> action);

        /// <summary> 
        /// Removes all available elements that meet the criteria defined by 
        /// <paramref name="criteria"/> from this queue and invoke the given
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
        /// <param name="criteria">The criteria to filter the elements.</param>
        /// <returns>The number of elements processed.</returns>
        /// <exception cref="System.InvalidOperationException">
        /// If the queue cannot be drained at this time.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the specified action is <see langword="null"/>.
        /// </exception>
        /// <seealso cref="Drain(System.Action{T},int)"/>
        int Drain(Action<T> action, Predicate<T> criteria);

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
        /// <seealso cref="Drain(System.Action{T})"/>
        int Drain(Action<T> action, int maxElements);

        /// <summary> 
        /// Removes at most the given number of available elements that meet 
        /// the criteria defined by <paramref name="criteria"/> from this 
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
        /// <param name="criteria">The criteria to filter the elements.</param>
        /// <returns>The number of elements processed.</returns>
        /// <exception cref="System.InvalidOperationException">
        /// If the queue cannot be drained at this time.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the specified action is <see langword="null"/>.
        /// </exception>
        /// <seealso cref="Drain(System.Action{T})"/>
        int Drain(Action<T> action, int maxElements, Predicate<T> criteria);
    }
}