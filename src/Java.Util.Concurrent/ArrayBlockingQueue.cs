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

namespace Java.Util.Concurrent
{
	/// <summary> 
	/// A bounded <see cref="IBlockingQueue{T}"/> backed by an array.  This queue 
	/// orders elements FIFO (first-in-first-out).  The <i>head</i> of the queue 
	/// is that element that has been on the queue the longest time.  The 
	/// <i>tail</i> of the queue is that element that has been on the queue the 
	/// shortest time. New elements are inserted at the tail of the queue, and 
	/// the queue retrieval operations obtain elements at the head of the queue.
	/// <p/>
	/// This is a classic &quot;bounded buffer&quot;, in which a fixed-sized 
	/// array holds elements inserted by producers and extracted by consumers.  
	/// Once created, the capacity cannot be increased.  Attempts to 
	/// <see cref="IBlockingQueue{T}.Put(T)"/> an element into a full queue will 
	/// result in the operation blocking; attempts to <see cref="IBlockingQueue{T}.Take()"/>
	/// an element from an empty queue will similarly block.
	/// 
	/// <p/> 
	/// This class supports an optional fairness policy for ordering waiting 
	/// producer and consumer threads.  By default, this ordering is not 
	/// guaranteed. However, a queue constructed with fairness set to 
	/// <see langword="true"/> grants threads access in FIFO order. Fairness
	/// generally decreases throughput but reduces variability and avoids
	/// starvation.
	/// </summary>
	/// <author>Doug Lea</author>
	/// <author>Griffin Caprio (.NET)</author>
	/// <author>Kenneth Xu</author>
	[Serializable]
	public class ArrayBlockingQueue<T> : BlockingQueueWrapper<T> //BACKPORT_2_2
	{
		#region Constructors

		/// <summary> 
		/// Creates an <see cref="ArrayBlockingQueue{T}"/> with the given (fixed)
		/// <paramref name="capacity"/> and the specified <paramref name="isFair"/> 
		/// fairness access policy.
		/// </summary>
		/// <param name="capacity">The capacity of this queue.</param>
		/// <param name="isFair">
		/// If <see langword="true"/> then queue accesses for threads blocked
		/// on insertion or removal, are processed in FIFO order; if 
		/// <see langword="false"/> the access order is unspecified.
		/// </param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// If <paramref name="capacity"/> is less than 1.
		/// </exception>
		public ArrayBlockingQueue(int capacity, bool isFair) : 
            base(new ArrayQueue<T>(capacity), capacity, isFair)
		{
		}

		/// <summary> 
		/// Creates an <see cref="ArrayBlockingQueue{T}"/> with the given (fixed)
		/// <paramref name="capacity"/> and the specified <paramref name="isFair"/> 
		/// fairness access policy and initially containing the  elements of the 
		/// given collection, added in traversal order of the collection's iterator.
		/// </summary>
		/// <param name="capacity">The capacity of this queue.</param>
        /// <param name="isFair">
        /// If <see langword="true"/> then queue accesses for threads blocked
        /// on insertion or removal, are processed in FIFO order; if 
        /// <see langword="false"/> the access order is unspecified.
        /// </param>
        /// <param name="collection">
        /// The collection of elements to initially contain.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="capacity"/> is less than 1.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="collection"/> is <see langword="null"/>.
		/// </exception> 
		public ArrayBlockingQueue(int capacity, bool isFair, IEnumerable<T> collection) : 
            base(new ArrayQueue<T>(capacity, collection), capacity, isFair)
		{
		}

		/// <summary> 
		/// Creates an <see cref="ArrayBlockingQueue{T}"/> with the given (fixed)
		/// <paramref name="capacity"/> and default fairness access policy.
		/// </summary>
        /// <param name="capacity">The capacity of this queue.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="capacity"/> is less than 1.
        /// </exception>
        public ArrayBlockingQueue(int capacity)
            : this(capacity, false)
		{
		}

		#endregion
        
	}
}