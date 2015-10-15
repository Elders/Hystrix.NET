#region License

/*
 * Copyright (C) 2002-2010 the original author or authors.
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
using System.Threading;

namespace Java.Util.Concurrent.Locks
{
	/// <summary>
	/// <see cref="ICondition"/> factors out the <see cref="Monitor"/>
	/// methods <see cref="Monitor.Wait(object)"/>, <see cref="Monitor.Pulse(object)"/>
	/// and <see cref="Monitor.PulseAll(object)"/> into distinct objects to
	/// give the effect of having multiple wait-sets per object, by combining
	/// them with the use of arbitrary <see cref="ILock"/> implementations.
	/// Where a <see cref="ILock"/> replaces the use of <c>lock</c> statements
	/// or <see cref="Monitor.Enter"/> and <see cref="Monitor.Exit"/> methods,
	/// a <see cref="ICondition"/> replaces the use of the wait and pause
    /// methods of <see cref="Monitor"/>.
	/// </summary>
	/// <remarks>
    /// <para>
    /// Conditions (also known as <b>condition queues</b> or <b>condition
    /// variables</b>) provide a means for one thread to suspend execution (to
    /// 'wait') until notified by another thread that some state condition may
    /// now be true.  Because access to this shared state information occurs
    /// in different threads, it must be protected, so a lock of some form is
    /// associated with the condition. The key property that waiting for a
    /// condition provides is that it <b>atomically</b> releases the associated
    /// lock and suspends the current thread, just like
    /// <see cref="Monitor.Wait(object)"/>.
    /// </para>
    /// <para>
	/// An <see cref="ICondition"/> instance is intrinsically bound to a lock.
	/// To obtain a <see cref="ICondition"/> instance for a particular
	/// <see cref="ILock"/> instance use its <see cref="ILock.NewCondition()"/>
	/// method.
    /// </para>
    /// <example>
    /// As an example, suppose we have a bounded buffer which supports put and
    /// take methods.  If a take is attempted on an empty buffer, then the
    /// thread will block until an item becomes available; if a put is attempted
    /// on a full buffer, then the thread will block until a space becomes
    /// available.  We would like to keep waiting put threads and take threads
    /// in separate wait-sets so that we can use the optimization of only
    /// notifying a single thread at a time when items or spaces become available
    /// in the buffer. This can be achieved using two <see cref="ICondition"/>
    /// instances.
	/// <code language="c#">
	/// class BoundedBuffer {
	///		readonly ILock lock = new ReentrantLock();
    /// 	readonly ICondition notFull  = lock.NewCondition();
    /// 	readonly ICondition notEmpty = lock.NewCondition();
	/// 
    /// 	readonly object[] items = new object[100];
	/// 	int putptr, takeptr, count;
	/// 
	/// 	public void Put(object x) {
	///			using(lock.Lock()) {
	///				while (count == items.Length) {
	///					notFull.Await();
	///					items[putptr] = x;
	///	 				if (++putptr == items.Length) putptr = 0;
	///	 				++count;
	///	 				notEmpty.Signal();
	///	 			}
	///			}
	///	    }
	///	 
	///	    public object Take() {
	///			using(lock.Lock()) {
	///	 			while (count == 0) {
	///	 				notEmpty.Await();
	///	 				object x = items[takeptr];
	///	 				if (++takeptr == items.Length) takeptr = 0;
	///	 				--count;
	///	 				notFull.Signal();
	///	 			return x;
	///	 		}
	///	 	}
	/// }
	/// </code>
	/// 
	/// (The <see cref="ArrayBlockingQueue{T}"/> class provides this
	/// functionality, so there is no reason to implement this sample usage
	/// class.)
    /// </example>
    /// <para>
    /// An <see cref="ICondition"/> implementation can provide behavior and
    /// semantics that is different from that of the <see cref="Monitor"/>
    /// methods, such as guaranteed ordering for notifications, or not
    /// requiring a lock to be held when performing notifications. If an
    /// implementation provides such specialized semantics then the
	/// implementation must document those semantics.
    /// </para>
    /// <para>
    /// Note that <see cref="ICondition"/> instances are just normal objects
    /// and can themselves be used as the target in a synchronized statement,
	/// and can have <see cref="Monitor.Wait(object)"/> and
	/// <see cref="Monitor.Pulse(object)"/> methods invoked on them. Acquiring
	/// the monitor lock of a <see cref="ICondition"/> instance, or using it as
	/// a parameter to <see cref="Monitor"/> methods, has no specified
	/// relationship with acquiring the <see cref="ILock"/> associated with
	/// that <see cref="ICondition"/> or the use of its <see cref="Await()"/>
	/// and <see cref="Signal()"/> methods. It is recommended that to avoid
	/// confusion you never use <see cref="ICondition"/> instances in this way,
	/// except perhaps within their own implementation.
    /// </para>
    /// <para>
    /// Except where noted, passing a <c>null</c> value for any parameter
	/// will result in a <see cref="NullReferenceException"/> being thrown.
    /// </para>
    /// <para>
    /// <b>Implementation Considerations</b>
    /// </para>
    /// <para>
    /// When waiting upon a <see cref="ICondition"/>, a '<b>spurious wakeup</b>'
    /// is permitted to occur, in general, as a concession to the underlying
    /// platform semantics. This has little practical impact on most application
    /// programs as a <see cref="ICondition"/> should always be waited upon in
    /// a loop, testing the state predicate that is being waited for.  An
    /// implementation is free to remove the possibility of spurious wakeups
    /// but it is recommended that applications programmers always assume that
    /// they can occur and so always wait in a loop.
    /// </para>
    /// <para>
    /// The three forms of condition waiting (interruptible, non-interruptible,
    /// and timed) may differ in their ease of implementation on some platforms
    /// and in their performance characteristics. In particular, it may be
    /// difficult to provide these features and maintain specific semantics
    /// such as ordering guarantees.  Further, the ability to interrupt the
    /// actual suspension of the thread may not always be feasible to implement
    /// on all platforms.
    /// </para>
    /// <para>
    /// Consequently, an implementation is not required to define exactly the
	/// same guarantees or semantics for all three forms of waiting, nor is it
	/// required to support interruption of the actual suspension of the thread.
    /// </para>
    /// <para>
    /// An implementation is required to clearly document the semantics and
    /// guarantees provided by each of the waiting methods, and when an
    /// implementation does support interruption of thread suspension then it
    /// must obey the interruption semantics as defined in this interface.
    /// </para>
    /// <para>
    /// As interruption generally implies cancellation, and checks for
	/// interruption are often infrequent, an implementation can favor responding
	/// to an interrupt over normal method return. This is true even if it can be
	/// shown that the interrupt occurred after another action may have unblocked
	/// the thread. An implementation should document this behavior.
    /// </para>
    /// </remarks>
	/// <author>Doug Lea</author>
	/// <author>Griffin Caprio (.NET)</author>
	public interface ICondition // BACKPORT_3_1
	{
		/// <summary> 
		/// Causes the current thread to wait until it is signalled or
		/// <see cref="Thread.Interrupt()"/> is called.
		/// </summary>
		/// <remarks>
        /// <para>
        /// The lock associated with this <see cref="ICondition"/> is atomically
		/// released and the current thread becomes disabled for thread scheduling
		/// purposes and lies dormant until <b>one</b> of four things happens:
		/// <list type="bullet">
		/// <item>Some other thread invokes the <see cref="Signal()"/> method
		/// for this <see cref="ICondition"/> and the current thread happens to
		/// be chosen as the thread to be awakened.</item>
		/// <item>Some other thread invokes the <see cref="SignalAll()"/>}
		/// method for this <see cref="ICondition"/>.</item>
		/// <item>Some other thread <see cref="Thread.Interrupt()"/> is called
		/// the current thread, and interruption of thread suspension is
		/// supported.</item>
		/// <item>A '<b>spurious wakeup</b>' occurs.</item>
        /// </list>
        /// </para>
        /// <para>
        /// In all cases, before this method can return the current thread must
		/// re-acquire the lock associated with this condition. When the thread
		/// returns it is <b>guaranteed</b> to hold this lock.
        /// </para>
        /// <para>
        /// If the current thread:
		/// <list type="bullet">
		/// <item>has its interrupted status set on entry to this method</item>
		/// <item><see cref="Thread.Interrupt()"/> is called while waiting
		/// and interruption of thread suspension is supported</item>
		/// </list>
		/// then <see cref="ThreadInterruptedException"/> is thrown and the current thread's
		/// interrupted status is cleared. It is not specified, in the first
		/// case, whether or not the test for interruption occurs before the lock
		/// is released.
		/// </para>
        /// <para>
        /// <b>Implementation Considerations</b>
        /// </para>
        /// <para>
        /// The current thread is assumed to hold the lock associated with this
		/// <see cref="ICondition"/> when this method is called.
		/// It is up to the implementation to determine if this is
		/// the case and if not, how to respond. Typically, an exception will be
		/// thrown (such as <see cref="SynchronizationLockException"/>) and the
		/// implementation must document that fact.
        /// </para>
        /// <para>
        /// An implementation can favor responding to an interrupt over normal
		/// method return in response to a signal. In that case the implementation
		/// must ensure that the signal is redirected to another waiting thread, if
		/// there is one.
		/// </para>
        /// </remarks>
        /// <exception cref="ThreadInterruptedException">
		/// if the current threada is interrupted (and interruption of thread suspension is supported.
		/// </exception>
		void Await();

		/// <summary>
		/// Causes the current thread to wait until it is signalled.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The lock associated with this condition is atomically
		/// released and the current thread becomes disabled for thread scheduling
		/// purposes and lies dormant until <b>one</b> of three things happens:
		/// <list type="bullet">
		/// <item>Some other thread invokes the <see cref="Signal()"/> method
		/// for this <see cref="ICondition"/> and the current thread happens to
		/// be chosen as the thread to be awakened.</item>
		/// <item>Some other thread invokes the <see cref="SignalAll()"/>} method
		/// for this <see cref="ICondition"/>.</item>
		/// <item>A '<b>spurious wakeup</b>' occurs.</item>
		/// </list>
		/// </para>
		/// <para>
		/// In all cases, before this method can return the current thread must
		/// re-acquire the lock associated with this condition. When the
		/// thread returns it is <b>guaranteed</b> to hold this lock.
		/// </para>
		/// <para>
		/// If the current thread's interrupted status is set when it enters
		/// this method, or <see cref="Thread.Interrupt()"/> is called
		/// while waiting, it will continue to wait until signalled. When it finally
		/// returns from this method its interrupted status will still
		/// be set.
		/// </para>
		/// <para>
		/// <b>Implementation Considerations</b>
		/// <br/>
		/// The current thread is assumed to hold the lock associated with this
		/// <see cref="ICondition"/> when this method is called.
		/// It is up to the implementation to determine if this is
		/// the case and if not, how to respond. Typically, an exception will be
		/// thrown (such as <see cref="SynchronizationLockException"/>) and the
		/// implementation must document that fact.
		/// </para>
        /// </remarks>
        void AwaitUninterruptibly();

        /// <summary>
        /// Causes the current thread to wait until it is signalled or interrupted,
        /// or the specified waiting time elapses.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The lock associated with this condition is atomically
        /// released and the current thread becomes disabled for thread scheduling
        /// purposes and lies dormant until <i>one</i> of five things happens:
        /// <list type="bullet">
        /// <item>Some other thread invokes the <see cref="Signal"/> method for this
        /// <c>Condition</c> and the current thread happens to be chosen as the
        /// thread to be awakened; or</item>
        /// <item>Some other thread invokes the <see cref="SignalAll"/> method for this
        /// <c>Condition</c>; or</item>
        /// <item>Some other thread <see cref="Thread.Interrupt">interrupts</see> the
        /// current thread, and interruption of thread suspension is supported; or</item>
        /// <item>The specified waiting time elapses; or</item>
        /// <item>A &quot;<i>spurious wakeup</i>&quot; occurs.</item>
        /// </list>
        /// </para>
        /// <para>
        /// In all cases, before this method can return the current thread must
        /// re-acquire the lock associated with this condition. When the
        /// thread returns it is <i>guaranteed</i> to hold this lock.
        /// </para>
        /// <para>
        /// If the current thread:
        /// <list type="bullet">
        /// <item>has its interrupted status set on entry to this method; or</item>
        /// <item>is <see cref="Thread.Interrupt">interrupts</see> while waiting
        /// and interruption of thread suspension is supported,</item>
        /// </list>
        /// then <see cref="ThreadInterruptedException"/> is thrown and the current
        /// thread's interrupted status is cleared. It is not specified, in the first
        /// case, whether or not the test for interruption occurs before the lock
        /// is released.
        /// </para>
        /// <para>
        /// <b>Implementation Considerations</b>
        /// </para>
        /// <para>
        /// The current thread is assumed to hold the lock associated with this
        /// <c>Condition</c> when this method is called.
        /// It is up to the implementation to determine if this is
        /// the case and if not, how to respond. Typically, an exception will be
        /// thrown (such as <see cref="SynchronizationLockException"/>) and the
        /// implementation must document that fact.
        /// </para>
        /// <para>
        /// An implementation can favor responding to an interrupt over normal
        /// method return in response to a signal, or over indicating the elapse
        /// of the specified waiting time. In either case the implementation
        /// must ensure that the signal is redirected to another waiting thread, if
        /// there is one.
        /// </para>
        /// </remarks>
        /// <param name="timeSpan">The maximum time to wait.</param>
		/// <returns>
		/// <c>false</c> if the waiting time detectably elapsed
		/// before return from the method, else <c>true</c>.
		/// </returns>
		/// <exception cref="ThreadInterruptedException">
		/// If the current thread is interrupted ( and interruption of thread
		/// suspension is supported.
		/// </exception>
		bool Await(TimeSpan timeSpan);

		/// <summary>
		/// Causes the current thread to wait until it is signalled or interrupted,
		/// or the specified deadline elapses.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The lock associated with this condition is atomically
		/// released and the current thread becomes disabled for thread scheduling
		/// purposes and lies dormant until <b>one</b> of five things happens:
		/// <list type="bullet">
		/// <item>Some other thread invokes the <see cref="Signal()"/> method for this
		/// <see cref="ICondition"/> and the current thread happens to be chosen as the
		/// thread to be awakened.</item>
		/// <item>Some other thread invokes the <see cref="SignalAll()"/>} method for this
		/// <see cref="ICondition"/>.</item>
		/// <item>Some other thread <see cref="Thread.Interrupt()"/> is called the current
		/// thread, and interruption of thread suspension is supported.</item>
		/// <item>The specified deadline elapses.</item>	
		/// <item>A '<b>spurious wakeup</b>' occurs.</item>
		/// </list>
		/// </para>
		/// <para>
		/// In all cases, before this method can return the current thread must
		/// re-acquire the lock associated with this condition. When the
		/// thread returns it is <b>guaranteed</b> to hold this lock.
		/// </para>
		/// <para>
		/// If the current thread:
		/// <list type="bullet">
		/// <item>has its interrupted status set on entry to this method</item>
		/// <item><see cref="Thread.Interrupt()"/> is called while waiting
		/// and interruption of thread suspension is supported</item>
		/// </list>
		/// then <see cref="ThreadInterruptedException"/> is thrown and the current thread's
		/// interrupted status is cleared. It is not specified, in the first
		/// case, whether or not the test for interruption occurs before the lock
		/// is released.
		/// </para>
		/// <example>
		/// The return value indicates whether the deadline has elapsed,
		/// which can be used as follows:
		/// <code>
		///		bool AMethod(DateTime deadline) {
		///			bool stillWaiting = true;
		///			while (!conditionBeingWaitedFor) {
		/// 				if (stillWaiting)
		/// 						stillWaiting = theCondition.AwaitUntil(deadline);
		/// 				else
		/// 						return false;
		/// 				}
		/// 		// ...
		/// 		}
		/// 	}
		/// </code>
		/// </example>
		/// <para>
		/// <b>Implementation Considerations</b>
		/// </para>
		/// <para>
		/// The current thread is assumed to hold the lock associated with this
		/// <see cref="ICondition"/> when this method is called.
		/// It is up to the implementation to determine if this is
		/// the case and if not, how to respond. Typically, an exception will be
		/// thrown (such as <see cref="SynchronizationLockException"/>) and the
		/// implementation must document that fact.
		/// </para>
		/// <para>
		/// An implementation can favor responding to an interrupt over normal
		/// method return in response to a signal, or over indicating the passing
		/// of the specified deadline. In either case the implementation
		/// must ensure that the signal is redirected to another waiting thread, if
		/// there is one.
		/// </para>
        /// </remarks>
        /// <param name="deadline">The absolute UTC time to wait.</param>
		/// <returns> 
		/// <c>false</c> if the deadline has elapsed upon return, else <c>true</c>.
		/// </returns>
		/// <exception cref="ThreadInterruptedException">
		/// If the current thread is interrupted ( and interruption of thread
		/// suspension is supported.
		/// </exception>
		bool AwaitUntil(DateTime deadline);

		/// <summary> 
		/// Wakes up one waiting thread.
		/// If any threads are waiting on this condition then one
		/// is selected for waking up. That thread must then re-acquire the
		/// lock before returning from await.
		/// </summary>
		void Signal();

		/// <summary> 
		/// Wakes up all waiting threads.
		/// If any threads are waiting on this condition then they are
		/// all woken up. Each thread must re-acquire the lock before it can
		/// return from await.
		/// </summary>
		void SignalAll();
	}
}