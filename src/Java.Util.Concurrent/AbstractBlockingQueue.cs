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
using System.Collections;
using System.Collections.Generic;
using System.Threading;
#endregion

namespace Java.Util.Concurrent
{
    /// <summary> 
    /// This class provides skeletal implementations of some
    /// <see cref="IBlockingQueue{T}"/> operations.
    /// </summary>
    /// <author>Kenneth Xu</author>
    [Serializable]
    public abstract class AbstractBlockingQueue<T> : AbstractQueue<T>, IBlockingQueue<T> // NET_ONLY
    {
        /// <summary> 
        /// Inserts the specified element into this queue, waiting if necessary
        /// for space to become available.
        /// </summary>
        /// <param name="element">the element to add</param>
        /// <exception cref="ThreadInterruptedException">
        /// if interrupted while waiting.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If some property of the supplied <paramref name="element"/> prevents
        /// it from being added to this queue.
        /// </exception>
        public abstract void Put(T element);

        /// <summary> 
        /// Inserts the specified element into this queue, waiting up to the
        /// specified wait time if necessary for space to become available.
        /// </summary>
        /// <param name="element">The element to add.</param>
        /// <param name="duration">How long to wait before giving up.</param>
        /// <returns>
        /// <see langword="true"/> if successful, or <see langword="false"/> if
        /// the specified waiting time elapses before space is available.
        /// </returns>
        /// <exception cref="ThreadInterruptedException">
        /// if interrupted while waiting.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If some property of the supplied <paramref name="element"/> prevents
        /// it from being added to this queue.
        /// </exception>
        public abstract bool Offer(T element, TimeSpan duration);

        /// <summary> 
        /// Retrieves and removes the head of this queue, waiting if necessary
        /// until an element becomes available.
        /// </summary>
        /// <returns> the head of this queue</returns>
        public abstract T Take();

        /// <summary> 
        /// Retrieves and removes the head of this queue, waiting up to the
        /// specified wait time if necessary for an element to become available.
        /// </summary>
        /// <param name="element">
        /// Set to the head of this queue. <c>default(T)</c> if queue is empty.
        /// </param>
        /// <param name="duration">How long to wait before giving up.</param>
        /// <returns> 
        /// <c>false</c> if the queue is still empty after waited for the time 
        /// specified by the <paramref name="duration"/>. Otherwise <c>true</c>.
        /// </returns>
        public abstract bool Poll(TimeSpan duration, out T element);

        /// <summary> 
        /// Removes all available elements from this queue and adds them to the 
        /// given collection.  
        /// </summary>
        /// <remarks>
        /// This operation may be more efficient than repeatedly polling this 
        /// queue.  A failure encountered while attempting to add elements to 
        /// collection <paramref name="collection"/> may result in elements 
        /// being in neither, either or both collections when the associated 
        /// exception is thrown.  Attempts to drain a queue to itself result in
        /// <see cref="System.ArgumentException"/>. Further, the behavior of
        /// this operation is undefined if the specified collection is
        /// modified while the operation is in progress.
        /// </remarks>
        /// <param name="collection">the collection to transfer elements into</param>
        /// <returns> the number of elements transferred</returns>
        /// <exception cref="System.InvalidOperationException">
        /// If the queue cannot be drained at this time.
        /// </exception>
        /// <exception cref="System.InvalidCastException">
        /// If the class of the supplied <paramref name="collection"/> prevents it
        /// from being used for the elemetns from the queue.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the specified collection is <see langword="null"/>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If <paramref name="collection"/> represents the queue itself.
        /// </exception>
        /// <seealso cref="AbstractQueue{T}.Drain(System.Action{T})"/>
        /// <seealso cref="DrainTo(ICollection{T},int)"/>
        /// <seealso cref="AbstractQueue{T}.Drain(System.Action{T},int)"/>
        public virtual int DrainTo(ICollection<T> collection)
        {
            return DrainTo(collection, null);
        }

        /// <summary> 
        /// Removes all available elements that meet the criteria defined by 
        /// <paramref name="predicate"/> from this queue and adds them to the 
        /// given collection.  
        /// </summary>
        /// <remarks>
        /// This operation may be more efficient than repeatedly polling this 
        /// queue.  A failure encountered while attempting to add elements to 
        /// collection <paramref name="collection"/> may result in elements 
        /// being in neither, either or both collections when the associated 
        /// exception is thrown.  Attempts to drain a queue to itself result in
        /// <see cref="System.ArgumentException"/>. Further, the behavior of
        /// this operation is undefined if the specified collection is
        /// modified while the operation is in progress.
        /// </remarks>
        /// <param name="collection">The collection to transfer elements into</param>
        /// <param name="predicate">The criteria to filter the elements</param>
        /// <returns> the number of elements transferred</returns>
        /// <exception cref="System.InvalidOperationException">
        /// If the queue cannot be drained at this time.
        /// </exception>
        /// <exception cref="System.InvalidCastException">
        /// If the class of the supplied <paramref name="collection"/> prevents it
        /// from being used for the elemetns from the queue.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the specified collection is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If <paramref name="collection"/> represents the queue itself.
        /// </exception>
        /// <seealso cref="IQueue{T}.Drain(System.Action{T})"/>
        /// <seealso cref="IBlockingQueue{T}.DrainTo(System.Collections.Generic.ICollection{T},int)"/>
        /// <seealso cref="IQueue{T}.Drain(System.Action{T},int)"/>
        public virtual int DrainTo(ICollection<T> collection, Predicate<T> predicate)
        {
            CheckCollection(collection);
            return DoDrain(collection.Add, predicate);
        }

        /// <summary> 
        /// Removes at most the given number of available elements from
        /// this queue and adds them to the given collection.  
        /// </summary>
        /// <remarks> 
        /// This operation may be more
        /// efficient than repeatedly polling this queue.  A failure
        /// encountered while attempting to add elements to
        /// collection <paramref name="collection"/> may result in elements being in neither,
        /// either or both collections when the associated exception is
        /// thrown.  Attempts to drain a queue to itself result in
        /// <see cref="System.ArgumentException"/>. Further, the behavior of
        /// this operation is undefined if the specified collection is
        /// modified while the operation is in progress.
        /// </remarks>
        /// <param name="collection">the collection to transfer elements into</param>
        /// <param name="maxElements">the maximum number of elements to transfer</param>
        /// <returns> the number of elements transferred</returns>
        /// <exception cref="System.InvalidOperationException">
        /// If the queue cannot be drained at this time.
        /// </exception>
        /// <exception cref="System.InvalidCastException">
        /// If the class of the supplied <paramref name="collection"/> prevents it
        /// from being used for the elemetns from the queue.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the specified collection is <see langword="null"/>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If <paramref name="collection"/> represents the queue itself.
        /// </exception>
        /// <seealso cref="DrainTo(ICollection{T})"/>
        /// <seealso cref="AbstractQueue{T}.Drain(System.Action{T})"/>
        /// <seealso cref="AbstractQueue{T}.Drain(System.Action{T},int)"/>
        public virtual int DrainTo(ICollection<T> collection, int maxElements)
        {
            return DrainTo(collection, maxElements, null);
        }

        /// <summary> 
        /// Removes at most the given number of available elements that meet 
        /// the criteria defined by <paramref name="predicate"/> from this 
        /// queue and adds them to the given collection.  
        /// </summary>
        /// <remarks> 
        /// This operation may be more
        /// efficient than repeatedly polling this queue.  A failure
        /// encountered while attempting to add elements to
        /// collection <paramref name="collection"/> may result in elements being in neither,
        /// either or both collections when the associated exception is
        /// thrown.  Attempts to drain a queue to itself result in
        /// <see cref="System.ArgumentException"/>. Further, the behavior of
        /// this operation is undefined if the specified collection is
        /// modified while the operation is in progress.
        /// </remarks>
        /// <param name="collection">the collection to transfer elements into</param>
        /// <param name="maxElements">the maximum number of elements to transfer</param>
        /// <param name="predicate">The criteria to filter the elements.</param>
        /// <returns> the number of elements transferred</returns>
        /// <exception cref="System.InvalidOperationException">
        /// If the queue cannot be drained at this time.
        /// </exception>
        /// <exception cref="System.InvalidCastException">
        /// If the class of the supplied <paramref name="collection"/> prevents it
        /// from being used for the elemetns from the queue.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the specified collection is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If <paramref name="collection"/> represents the queue itself.
        /// </exception>
        /// <seealso cref="IBlockingQueue{T}.DrainTo(System.Collections.Generic.ICollection{T})"/>
        /// <seealso cref="IQueue{T}.Drain(System.Action{T})"/>
        /// <seealso cref="IQueue{T}.Drain(System.Action{T},int)"/>
        public virtual int DrainTo(ICollection<T> collection, int maxElements, Predicate<T> predicate)
        {
            CheckCollection(collection);
            if (maxElements <= 0) return 0;
            return DoDrain(collection.Add, maxElements, predicate);
        }

        /// <summary>
        /// Gets a value indicating whether access to the <see cref="ICollection"/> 
        /// is synchronized (thread safe).
        /// </summary>
        /// <remarks>This implementaiton always return <see langword="true"/>.</remarks>
        /// <returns>
        /// true if access to the <see cref="ICollection"></see> 
        /// is synchronized (thread safe); otherwise, false.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        protected override bool IsSynchronized
        {
            get { return true; }
        }

        private void CheckCollection(ICollection<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");
            if (collection == this)
                throw new ArgumentException("Cannot drain queue to itself.", "collection");
        }
    }
}