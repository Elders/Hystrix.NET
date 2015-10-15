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
	/// <see cref="ILock"/> implementations provide more extensive locking
	/// operations than can be obtained using <see cref="Monitor"/> class and 
	/// <c>lock</c> statements.  They allow more flexible structuring, may have
	/// quite different properties, and may support multiple associated
	/// <see cref="ICondition"/> objects.
	/// </summary>
	/// <remarks>
    /// <para>
    /// A lock is a tool for controlling access to a shared resource by
	/// multiple threads. Commonly, a lock provides exclusive access to a
	/// shared resource: only one thread at a time can acquire the lock and
	/// all access to the shared resource requires that the lock be
	/// acquired first. However, some locks may allow concurrent access to
	/// a shared resource, such as the read lock of a <see cref="IReadWriteLock"/> 
    /// </para>
    /// <para>
    /// The use of <see cref="Monitor"/> class or <c>lock</c> statement provides
	/// access to the implicit monitor lock associated with every object, but
	/// forces all lock acquisition and release to occur in a block-structured way:
	/// when multiple locks are acquired they must be released in the opposite
	/// order, and all locks must be released in the same lexical scope in which
	/// they were acquired.
    /// </para>
    /// <para>
    /// While the scoping mechanism for <see cref="Monitor"/> or <c>lock</c>
	/// statements makes it much easier to program with monitor locks,
	/// and helps avoid many common programming errors involving locks,
	/// there are occasions where you need to work with locks in a more
	/// flexible way. For example, some algorithms for traversing
	/// concurrently accessed data structures require the use of
	/// 'hand-over-hand' or 'chain locking': you
	/// acquire the lock of node A, then node B, then release A and acquire
	/// C, then release B and acquire D and so on.  Implementations of the
	/// <see cref="ILock"/> interface enable the use of such techniques by
	/// allowing a lock to be acquired and released in different scopes,
	/// and allowing multiple locks to be acquired and released in any
	/// order.
    /// </para>
    /// <para>
    /// With this increased flexibility comes additional responsibility. The
    /// absence of block-structured locking removes the automatic release of
    /// locks that occurs with <c>lock</c> statements. In most cases, the
    /// following idiom should be used:
	/// 
	/// <code>
	/// ILock l = ...;
	/// l.Lock();
	/// try {
	///     // access the resource protected by this lock
	/// } finally {
	///		l.Unlock();
	/// }
	/// </code> 
	/// -or-
    /// <code>
    /// ILock l = ...;
    /// using(l.Lock())
    /// {
    /// // access the resource protected by this lock
    /// }
    /// </code> 
    /// </para>
    /// <para>
    /// When locking and unlocking occur in different scopes, care must be
	/// taken to ensure that all code that is executed while the lock is
	/// held is protected by try-finally or try-catch to ensure that the
	/// lock is released when necessary.
    /// </para>
    /// <para>
    /// <see cref="ILock"/> implementations provide additional functionality
	/// over the use of <see cref="Monitor"/> class and <c>lock</c> statement
	/// by providing a non-blocking attempt to acquire a lock 
	/// (<see cref="TryLock()"/>), an attempt to acquire the lock that can be
	/// interrupted <see cref="LockInterruptibly()"/>}, and an attempt to acquire
	/// the lock that can timeout (<see cref="TryLock(TimeSpan)"/>).
    /// </para>
    /// <para>
    /// An <see cref="ILock"/> class can also provide behavior and semantics
	/// that is quite different from that of the implicit monitor lock,
	/// such as guaranteed ordering, non-reentrant usage, or deadlock
	/// detection. If an implementation provides such specialized semantics
	/// then the implementation must document those semantics.
    /// </para>
    /// <para>
    /// Note that <see cref="ILock"/> instances are just normal objects and can
	/// themselves be used as the target in a <c>lock</c> statement. Acquiring
	/// the monitor lock of a <see cref="ILock"/> instance has no specified
	/// relationship with invoking any of the <see cref="Lock()"/> methods of
	/// that instance. It is recommended that to avoid confusion you never use
	/// <see cref="ILock"/> instances in this way, except within their own
	/// implementation.
    /// </para>
    /// <para>
    /// Except where noted, passing a <c>null</c> value for any parameter will
    /// result in a <see cref="NullReferenceException"/> being thrown.
    /// </para>
    /// <para>
    /// <b>Memory Synchronization</b>
    /// <br/>
	/// All <see cref="ILock"/> implementations <b>must</b> enforce the same
	/// memory synchronization semantics as provided by the built-in monitor
	/// lock. 
	/// <list type="bullet">
	/// <item>A successful <see cref="Lock()"/> operation has the same memory
	/// synchronization effects as a successful <see cref="Monitor.Enter(object)"/>
	/// action.</item>
	/// <item>A successful <see cref="Unlock()"/> operation has the same memory
	/// synchronization effects as a successful <see cref="Monitor.Exit(object)"/>
	/// action.</item>
	/// </list>
	/// Unsuccessful locking and unlocking operations, and reentrant
	/// locking/unlocking operations, do not require any memory synchronization
	/// effects.
    /// </para>
    /// <para>
    /// <b>Implementation Considerations</b>
	/// <list type="bullet">
	/// <item>
	/// The three forms of lock acquisition (interruptible, non-interruptible, 
	/// and timed) may differ in their performance characteristics, ordering
	/// guarantees, or other implementation qualities.  Further, the ability
	/// to interrupt the <b>ongoing</b> acquisition of a lock may not be
	/// available in a given <see cref="ILock"/> class.  Consequently, an
	/// implementation is not required to define exactly the same guarantees
	/// or semantics for all three forms of lock acquisition, nor is it
	/// required to support interruption of an ongoing lock acquisition.  An
	/// implementation is required to clearly document the semantics and
	/// guarantees provided by each of the locking methods. It must also obey
	/// the interruption semantics as defined in this interface, to the extent
	/// that interruption of lock acquisition is supported: which is either
	/// totally, or only on method entry.
    /// </item>
    /// <item>
    /// As interruption generally implies cancellation, and checks for
	/// interruption are often infrequent, an implementation can favor responding
	/// to an interrupt over normal method return. This is true even if it can be
	/// shown that the interrupt occurred after another action may have unblocked
	/// the thread. An implementation should document this behavior.
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
	/// <seealso cref="ReentrantLock"/>
	/// <seealso cref="ICondition"/>
	/// <seealso cref="T:Spring.Threading.Locks.IReadWriteLock"/>
	/// <author>Doug Lea</author>
	/// <author>Griffin Caprio (.NET)</author>
	/// <author>Kenneth Xu</author>
	public interface ILock // JDK_1_6
    {
		/// <summary> 
		/// Acquires the lock. If the lock is not available then the current
		/// thread becomes disabled for thread scheduling purposes and lies
		/// dormant until the lock has been acquired.
        /// </summary>
        /// <remarks>
        /// <para>
		/// <b>Implementation Considerations</b>
		/// <br/>
		/// An <see cref="ILock"/> implementation may be able to detect
		/// erroneous use of the lock, such as an invocation that would cause
		/// deadlock, and may throw an exception in such circumstances. The
		/// circumstances and the exception type must be documented by that
		/// <see cref="ILock"/> implementation.
        /// </para>
        /// </remarks>
        /// <returns>
        /// An <see cref="IDisposable"/> that <see cref="Unlock"/>s the lock
        /// upon <see cref="IDisposable.Dispose"/>.
        /// </returns>
        IDisposable Lock();

		/// <summary> 
		/// Acquires the lock if it is available and returns immediately, or
		/// wait for the lock to become available unless the current thread is
		/// interrupted by a call to <see cref="Thread.Interrupt"/>.
		/// </summary>
		/// <remarks>
		/// <para>
		/// If the lock is not available then the current thread becomes
		/// disabled for thread scheduling purposes and lies dormant until one
		/// of two things happens:
        /// <list type="bullet">
		/// <item>The lock is acquired by the current thread.</item>
		/// <item>Some other thread interrupts the current thread by calling
		/// <see cref="Thread.Interrupt()"/>, and interruption of lock
		/// acquisition is supported.</item>
        /// </list>
        /// </para>
		/// <para>
		/// If the current thread:
		/// <list type="bullet">
		/// <item>has its interrupted status set on entry to this method</item>
		/// <item>is interrupted while acquiring the lock, and interruption of
		/// lock acquisition is supported</item>
		/// </list>
		/// then <see cref="ThreadInterruptedException"/> is thrown and the
		/// current thread's interrupted status is cleared.
		/// </para>
		/// <para>
		/// <b>Implementation Considerations</b>
		/// <br/>
		/// The ability to interrupt a lock acquisition in some implementations
		/// may not be possible, and if possible may be an expensive operation.
		/// The programmer should be aware that this may be the case. An
		/// implementation should document when this is the case.
		/// </para>
		/// <para>
		/// An implementation can favor responding to an interrupt over
		/// normal method return.
		/// </para>
		/// <para>
		/// A <see cref="ILock"/> implementation may be able to detect
		/// erroneous use of the lock, such as an invocation that would cause
		/// deadlock, and may throw an exception in such circumstances.  The
		/// circumstances and the exception type must be documented by that
		/// <see cref="ILock"/> implementation.
		/// </para>
		/// </remarks>
        /// <returns>
        /// An <see cref="IDisposable"/> that <see cref="Unlock"/>s the lock
        /// open <see cref="IDisposable.Dispose"/>.
        /// </returns>
		/// <exception cref="ThreadInterruptedException">
		/// If the current thread is interrupted while acquiring the lock (and
		/// interruption of lock acquisition is supported).
		/// </exception>
		/// <seealso cref="Thread.Interrupt()"/>
		IDisposable LockInterruptibly();

		/// <summary> 
		/// Acquires the lock only if it is free at the time of invocation.
        /// </summary>
        /// <remarks>
        /// <para>
		/// Acquires the lock if it is available and returns immediately with
		/// the value <c>true</c>.  If the lock is not available then this
		/// method will return immediately with the value <c>false</c>.
		/// </para>
		/// <para>
		/// A typical usage idiom for this method would be:
		/// <code> 
		/// ILock lock = ...;
		/// if (lock.TryLock()) {
		///		try {
		///		// manipulate protected state
		///		} finally {
		///			lock.Unlock();
		///		}
		/// } else {
		///		// perform alternative actions
		/// }
		/// </code>
        /// </para>
        /// <para>
        /// This usage ensures that the lock is unlocked if it was acquired,
        /// and doesn't try to unlock if the lock was not acquired.
        /// </para>
        /// </remarks>
		/// <returns>
		/// <c>true</c> if the lock was acquired and <c>false</c> otherwise.
		/// </returns>
		bool TryLock();

		/// <summary> 
		/// Acquires the lock if it is free within the specified
		/// <paramref name="timeSpan"/> time and the current thread has not
		/// been interrupted by calling <see cref="Thread.Interrupt()"/>.
        /// </summary>
        /// <remark>
        /// <para>
        /// If the lock is available this method returns immediately with the
        /// value <c>true</c>. If the lock is not available then the current
        /// thread becomes disabled for thread scheduling purposes and lies
        /// dormant until one of three things happens:
		/// <list type="bullet">
		/// <item>The lock is acquired by the current thread.</item>
		/// <item>Some other thread interrupts the current thread, and
		/// interruption of lock acquisition is supported.</item>
		/// <item>The specified <paramref name="timeSpan"/> elapses.</item>
		/// </list>
        /// </para>
        /// <para>
        /// If the lock is acquired then the value <c>true</c> is returned.
        /// </para>
        /// <para>
        /// If the current thread:
        /// <list type="bullet">
        /// <item>has its interrupted status set on entry to this method;</item>
		/// <item>is interrupted while acquiring the lock, and interruption of
		/// lock acquisition is supported.</item>
		/// </list>
		/// then <see cref="ThreadInterruptedException"/> is thrown and the
		/// current thread's interrupted status is cleared.
        /// </para>
        /// <para>
        /// If the specified <paramref name="timeSpan"/> elapses then the value
        /// <c>false</c> is returned.  If the <paramref name="timeSpan"/> is
        /// less than or equal to zero, the method will not wait at all.
        /// </para>
        /// <para>
        /// <b>Implementation Considerations</b>
		/// <br/>
		/// The ability to interrupt a lock acquisition in some implementations
		/// may not be possible, and if possible may be an expensive operation.
		/// The programmer should be aware that this may be the case. An
		/// implementation should document when this is the case.
        /// </para>
        /// <para>
        /// An implementation can favor responding to an interrupt over normal
		/// method return, or reporting a timeout.
        /// </para>
        /// <para>
        /// An <see cref="ILock"/> implementation may be able to detect
		/// erroneous use of the lock, such as an invocation that would cause
		/// deadlock, and may throw an exception in suchcircumstances. The
		/// circumstances and the exception type must be documented by that
		/// <see cref="ILock"/> implementation.
        /// </para>
        /// </remark>
		/// <param name="timeSpan">
		/// The specificed <see cref="System.TimeSpan"/> to wait to aquire lock.
		/// </param>
		/// <returns>
		/// <c>true</c> if the lock was acquired and <c>false</c> if the
		/// waiting time elapsed before the lock was acquired.
		/// </returns>
		/// <exception cref="ThreadInterruptedException">
		/// If the current thread is interrupted while aquirign the lock (and
		/// interruption of lock acquisition is supported).
		/// </exception>
		/// <seealso cref="Thread.Interrupt()"/>
		bool TryLock( TimeSpan timeSpan );

		/// <summary> 
		/// Releases the lock.
        /// </summary>
        /// <remarks>
		/// <b>Implementation Considerations</b>
		/// <br/>
		/// An <see cref="ILock"/> implementation will usually impose
		/// restrictions on which thread can release a lock (typically only the
		/// holder of the lock can release it) and may throw an exception if
		/// the restriction is violated. Any restrictions and the exception
		/// type must be documented by that <see cref="ILock"/> implementation.
		/// </remarks>
		void Unlock();

		/// <summary> 
		/// Returns a new <see cref="ICondition"/> instance that is bound to
		/// this <see cref="ILock"/> instance.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Before waiting on the condition the lock must be held by the
		/// current thread.  A call to <see cref="ICondition.Await()"/> will
		/// atomically release the lock before waiting and re-acquire the lock
		/// before the wait returns.
        /// </para>
        /// <para>
        /// <b>Implementation Considerations</b>
		/// <br/>
		/// The exact operation of the <see cref="ICondition"/> instance
		/// depends on the <see cref="ILock"/> implementation and must be
		/// documented by that implementation.
        /// </para>
        /// </remarks>
		/// <returns>
		/// A new <see cref="ICondition"/> instance for this 
		/// <see cref="ILock"/> instance.
		/// </returns>
		/// <exception cref="NotSupportedException">
		/// If this <see cref="ILock"/> implementation does not support
		/// conditions.
		/// </exception>
		ICondition NewCondition();
	}
}