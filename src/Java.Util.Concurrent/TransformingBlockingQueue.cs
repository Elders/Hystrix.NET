#region License

/*
 * Copyright (C) 2009-2010 the original author or authors.
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
using System.Collections.Generic;
using System.Threading;

namespace Java.Util.Concurrent
{
    /// <summary>
    /// Class TransformingBlockingQueue
    /// </summary>
    /// <author>Kenneth Xu</author>
    public class TransformingBlockingQueue<TFrom, TTo> : AbstractBlockingQueue<TTo>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="tranformer"></param>
        /// <param name="reverser"></param>
        public TransformingBlockingQueue(IBlockingQueue<TFrom> source, Converter<TFrom, TTo> tranformer, Converter<TTo, TFrom> reverser)
        {
            
        }
        #region Overrides of AbstractCollection<TTo>

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <remarks>
        /// Subclass must implement this method.
        /// </remarks>
        /// <returns>
        /// A <see cref="IEnumerator{T}"/> that can be used to iterate 
        /// through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override IEnumerator<TTo> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Overrides of AbstractQueue<TTo>

        /// <summary>
        /// Returns the remaining capacity of this queue.
        /// </summary>
        public override int RemainingCapacity
        {
            get { throw new NotImplementedException(); }
        }

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
        public override bool Offer(TTo element)
        {
            throw new NotImplementedException();
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
        public override bool Peek(out TTo element)
        {
            throw new NotImplementedException();
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
        public override bool Poll(out TTo element)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the current capacity of this queue.
        /// </summary>
        public override int Capacity
        {
            get { throw new NotImplementedException(); }
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
        protected internal override int DoDrain(Action<TTo> action, int maxElements, Predicate<TTo> criteria)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Overrides of AbstractBlockingQueue<TTo>

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
        public override void Put(TTo element)
        {
            throw new NotImplementedException();
        }

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
        public override bool Offer(TTo element, TimeSpan duration)
        {
            throw new NotImplementedException();
        }

        /// <summary> 
        /// Retrieves and removes the head of this queue, waiting if necessary
        /// until an element becomes available.
        /// </summary>
        /// <returns> the head of this queue</returns>
        public override TTo Take()
        {
            throw new NotImplementedException();
        }

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
        public override bool Poll(TimeSpan duration, out TTo element)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}