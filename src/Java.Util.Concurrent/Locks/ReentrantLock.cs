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
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;
using Java.Util.Concurrent.Helpers;

namespace Java.Util.Concurrent.Locks
{
	/// <summary> 
	/// A reentrant mutual exclusion <see cref="ILock"/> with the same basic
	/// behavior and semantics as the implicit monitor lock accessed using
	/// <c>lock</c> statements, but with extended capabilities.
    /// </summary>
    /// <remarks>
	/// <para>
	/// A <see cref="ReentrantLock"/> is <b>owned</b> by the thread last
	/// successfully locking, but not yet unlocking it. A thread invoking
	/// <see cref="Lock()"/> will return, successfully acquiring the lock, when
	/// the lock is not owned by another thread. The method will return
	/// immediately if the current thread already owns the lock. This can
	/// be checked using methods <see cref="IsHeldByCurrentThread"/>, and 
	/// <see cref="HoldCount"/>.
    /// </para>
    /// <para>
	/// The constructor for this class accepts an optional
	/// <b>fairness</b> parameter.  When set <c>true</c>, under
	/// contention, locks favor granting access to the longest-waiting
	/// thread.  Otherwise this lock does not guarantee any particular
	/// access order.  Programs using fair locks accessed by many threads
	/// may display lower overall throughput (i.e., are slower; often much
	/// slower) than those using the default setting, but have smaller
	/// variances in times to obtain locks and guarantee lack of
	/// starvation. Note however, that fairness of locks does not guarantee
	/// fairness of thread scheduling. Thus, one of many threads using a
	/// fair lock may obtain it multiple times in succession while other
	/// active threads are not progressing and not currently holding the
	/// lock.
    /// Also note that the untimed <see cref="TryLock()"/> method does not
	/// honor the fairness setting. It will succeed if the lock
	/// is available even if other threads are waiting.
    /// </para>
    /// <para>
    /// It is recommended practice to <b>always</b> immediately follow a call 
    /// to <see cref="Lock()"/> with a <c>try</c> block or make user of
    /// the <c>using</c> keyward in C#, most typically in a before/after 
    /// construction such as:
	/// 
	/// <code>
	/// class X {
	///		private ReentrantLock lock = new ReentrantLock();
	///		// ...
	/// 
	/// 	public void m() {
	/// 		lock.Lock();  // block until condition holds
	/// 		try {
	/// 			// ... method body
	/// 		} finally {
	/// 			lock.Unlock()
	/// 		}
	/// 	}
	/// }
	/// </code>
    /// </para>
    /// <para>
    /// In addition to implementing the <see cref="ILock"/> interface, this
	/// class defines methods <see cref="IsLocked"/> and
	/// <see cref="GetWaitQueueLength"/> , as well as some associated
	/// <c>protected</c> access methods that may be useful for
	/// instrumentation and monitoring.
    /// </para>
    /// <para>
    /// Serialization of this class behaves in the same way as built-in
	/// locks: a deserialized lock is in the unlocked state, regardless of
	/// its state when serialized.
    /// </para>
    /// <para>
    /// This lock supports a maximum of 2147483648 recursive locks by
	/// the same thread.
    /// </para>
    /// </remarks>
    /// <author>Doug Lea</author>
	/// <author>Dawid Kurzyniec</author>
	/// <author>Griffin Caprio (.NET)</author>
    /// <author>Kenneth Xu (.NET)</author>
	[Serializable]
	public class ReentrantLock : ILock, IDisposable, ConditionVariable.IExclusiveLock
	{
		#region Internal Helper Classes

        /// <summary>
        /// Base of synchronization control for this lock. Subclassed
        /// into fair and nonfair versions below.
        /// </summary>
		[Serializable]
		private abstract class Sync
		{
			[NonSerialized] protected Thread _owner;
			[NonSerialized] protected int _holds;

			internal Thread Owner
			{
				get { lock (this) return _owner; }
			}

			public int HoldCount
			{
				get { lock (this) return IsHeldByCurrentThread ? _holds : 0; }
			}

			public bool IsHeldByCurrentThread
			{
				get { lock (this) return Thread.CurrentThread == _owner; }
			}

			public bool IsLocked
			{
				get { lock (this) return _owner != null; }
			}

            public bool TryLock()
			{
				Thread caller = Thread.CurrentThread;
                lock (this) return GetHold(caller);
			}

			public virtual bool HasQueuedThreads
			{
				get { throw new NotSupportedException("Use FAIR version"); }
			}

			public virtual int QueueLength
			{
				get { throw new NotSupportedException("Use FAIR version"); }
			}

			public virtual ICollection<Thread> QueuedThreads
			{
				get { throw new NotSupportedException("Use FAIR version"); }
			}

			public virtual bool IsQueued(Thread thread)
			{
				throw new NotSupportedException("Use FAIR version");
			}

            public abstract void Lock();
			public abstract bool IsFair { get; }
			public abstract void LockInterruptibly();
			public abstract bool TryLock(TimeSpan timespan);
			public abstract void Unlock();

            protected bool GetHold(Thread caller)
            {
                if (_owner == null)
                {
                    _owner = caller;
                    _holds = 1;
                    return true;
                }
                if (caller == _owner)
                {
                    int holds = _holds;
                    if (++holds < 0)
                        throw new SystemException("Maximum lock count exceeded");
                    _holds = holds;
                    return true;
                }
                return false;
            }
		}

		[Serializable]
		private sealed class NonfairSync : Sync
		{
		    public override bool IsFair
			{
				get { return false; }
			}

            /// <summary>
            /// Performs lock.  Try immediate barge, backing up to normal acquire on failure.
            /// </summary>
            public override void Lock()
            {
                Thread caller = Thread.CurrentThread;
                lock (this)
                {
                    if (GetHold(caller)) return;
                    bool wasInterrupted = false;
                    try
                    {
                        while (true)
                        {
                            try
                            {
                                Monitor.Wait(this);
                            }
                            catch (ThreadInterruptedException)
                            {
                                wasInterrupted = true;
                                // no need to notify; if we were signalled, we
                                // will act as signalled, ignoring the
                                // interruption
                            }
                            if (_owner == null)
                            {
                                _owner = caller;
                                _holds = 1;
                                return;
                            }
                        }
                    }
                    finally
                    {
                        if (wasInterrupted) Thread.CurrentThread.Interrupt();
                    }
                }
            }


		    public override void LockInterruptibly()
			{
				Thread caller = Thread.CurrentThread;
				lock (this)
				{
                    if (GetHold(caller)) return;
                    try
				    {
				        do
				        {
				            Monitor.Wait(this);
				        } while (_owner != null);
				        _owner = caller;
				        _holds = 1;
				        return;
				    }
				    catch (ThreadInterruptedException e)
				    {
                        if (_owner == null) Monitor.Pulse(this);
				        throw SystemExtensions.PreserveStackTrace(e);
				    }
				}
			}

			public override bool TryLock(TimeSpan durationToWait)
			{
				Thread caller = Thread.CurrentThread;

				lock (this)
				{
				    if (GetHold(caller)) return true;
				    if (durationToWait.Ticks <= 0)
				        return false;
				    DateTime deadline = DateTime.UtcNow.Add(durationToWait);
				    try
				    {
				        for (;;)
				        {
				            Monitor.Wait(this, durationToWait);
                            if (_owner == null)
                            {
                                _owner = caller;
                                _holds = 1;
                                return true;
                            }
                            durationToWait = deadline.Subtract(DateTime.UtcNow);
				            if ( durationToWait.Ticks <= 0)
				                return false;
				        }
				    }
				    catch (ThreadInterruptedException e)
				    {
				        if (_owner == null ) Monitor.Pulse(this);
				        throw SystemExtensions.PreserveStackTrace(e);
				    }
				}
			}

		    public override void Unlock()
			{
				lock (this)
				{
					if (Thread.CurrentThread != _owner)
					{
						throw new SynchronizationLockException("Not owner");
					}

					if (--_holds == 0)
					{
						_owner = null;
						Monitor.Pulse(this);
					}
				}
			}
		}

		[Serializable]
        private sealed class FairSync : Sync, IQueuedSync, IDeserializationCallback 
		{
			[NonSerialized] private IWaitQueue _wq = new FIFOWaitQueue();

			public override bool IsFair
			{
				get { return true; }
			}

			public bool Recheck(WaitNode node)
			{
				Thread caller = Thread.CurrentThread;
				lock (this)
				{
                    if(GetHold(caller)) return true;
				    _wq.Enqueue(node);
					return false;
				}
			}

			public void TakeOver(WaitNode node)
			{
				lock (this)
				{
					Debug.Assert(_holds == 1 && _owner == Thread.CurrentThread);
					_owner = node.Owner;
				}
			}

            public override void Lock()
            {
                Thread caller = Thread.CurrentThread;
                lock (this)
                {
                    if(GetHold(caller)) return;
                }
                WaitNode n = new WaitNode();
                n.DoWaitUninterruptibly(this);
            }


		    public override void LockInterruptibly()
			{
				Thread caller = Thread.CurrentThread;
				lock (this)
				{
                    if (GetHold(caller)) return;
                }
				WaitNode n = new WaitNode();
				n.DoWait(this);
			}

			public override bool TryLock(TimeSpan timespan)
			{
				Thread caller = Thread.CurrentThread;
				lock (this)
				{
                    if (GetHold(caller)) return true;
                }
			    WaitNode n = new WaitNode();
				return n.DoTimedWait(this, timespan);
			}

		    private WaitNode GetSignallee(Thread caller)
			{
				lock (this)
				{
					if (caller != _owner)
					{
						throw new SynchronizationLockException("Not owner");
					}
                    if (_holds >= 2) // current thread will keep the lock
					{
						--_holds;
						return null;
					}
					WaitNode w = _wq.Dequeue();
                    if (w == null) // if none, clear for new arrivals
					{
						_owner = null;
						_holds = 0;
					}
					return w;
				}
			}

			public override void Unlock()
			{
				Thread caller = Thread.CurrentThread;
				for (;; )
				{
					WaitNode w = GetSignallee(caller);
                    if (w == null) return; // no one to signal
                    if (w.Signal(this)) return; // notify if still waiting, else skip
				}
			}

			public override bool HasQueuedThreads
			{
				get
				{
					lock (this)
					{
						return _wq.HasNodes;
					}
				}
			}

			public override int QueueLength
			{
				get
				{
					lock (this)
					{
						return _wq.Length;
					}
				}
			}

			public override ICollection<Thread> QueuedThreads
			{
				get
				{
					lock (this)
					{
						return _wq.WaitingThreads;
					}
				}
			}

			public override bool IsQueued(Thread thread)
			{
				lock (this)
				{
					return _wq.IsWaiting(thread);
				}
			}

            #region IDeserializationCallback Members

		    void IDeserializationCallback.OnDeserialization(object sender)
            {
                lock (this)
                {
                    _wq = new FIFOWaitQueue();
                }
            }

            #endregion
        }

		#endregion

		private readonly Sync sync;

		/// <summary> 
		/// Queries the number of holds on this lock by the current thread.
		/// 
		/// <p/>
		/// A thread has a hold on a lock for each lock action that is not
		/// matched by an unlock action.
		/// 
		/// <p/>
		/// The hold count information is typically only used for testing and
		/// debugging purposes. For example, if a certain section of code should
		/// not be entered with the lock already held then we can assert that
		/// fact:
		/// 
		/// <code>
		///	class X {
		/// 	ReentrantLock lock = new ReentrantLock();
		/// 	// ...
		/// 	public void m() {
		/// 		Debug.Assert( lock.HoldCount() == 0 );
		/// 		lock.Lock();
		/// 		try {
		/// 			// ... method body
		/// 		} finally {
		/// 			lock.Unlock();
		/// 		}
		/// 	}
		/// }
		/// </code>
		/// </summary>
		/// <returns> 
		/// The number of holds on this lock by the current thread,
		/// or zero if this lock is not held by the current thread.
		/// </returns>
		public virtual int HoldCount
		{
			get { return sync.HoldCount; }

		}

		/// <summary> 
		/// Queries if this lock is held by the current thread.
		/// 
		/// <p/>
		/// This method is typically used for debugging and
		/// testing. For example, a method that should only be called while
		/// a lock is held can assert that this is the case:
		/// 
		/// <code>
		/// class X {
		///		ReentrantLock lock = new ReentrantLock();
		/// 	// ...
		/// 
		/// 	public void m() {
		/// 		Debug.Assert( lock.IsHeldByCurrentThread );
		/// 		// ... method body
		/// 	}
		/// }
		/// </code>
		/// 
		/// <p/>
		/// It can also be used to ensure that a reentrant lock is used
		/// in a non-reentrant manner, for example:
		/// 
		/// <code>
		/// class X {
		///		ReentrantLock lock = new ReentrantLock();
		/// 	// ...
		/// 
		/// 	public void m() {
		/// 		Debug.Assert( !lock.IsHeldByCurrentThread );
		/// 		lock.Lock();
		/// 		try {
		/// 			// ... method body
		/// 		} finally {
		/// 			lock.Unlock();
		/// 		}
		/// 	}
		/// }
		/// </code>
		/// </summary>
		/// <returns> <c>true</c> if current thread holds this lock and
		/// <c>false</c> otherwise.
		/// </returns>
		public virtual bool IsHeldByCurrentThread
		{
			get { return sync.IsHeldByCurrentThread; }

		}

		/// <summary> 
		/// Queries if this lock is held by any thread. This method is
		/// designed for use in monitoring of the system state,
		/// not for synchronization control.
		/// </summary>
		/// <returns> <c>true</c> if any thread holds this lock and
		/// <c>false</c> otherwise.
		/// </returns>
		public virtual bool IsLocked
		{
			get { return sync.IsLocked; }

		}

		/// <summary> Returns <c>true</c> if this lock has fairness set true.</summary>
		/// <returns> <c>true</c> if this lock has fairness set true.
		/// </returns>
		public virtual bool IsFair
		{
			get { return sync.IsFair; }

		}

		/// <summary> 
		/// Returns the thread that currently owns this lock, or
		/// <c>null</c> if not owned. Note that the owner may be
		/// momentarily <c>null</c> even if there are threads trying to
		/// acquire the lock but have not yet done so.  This method is
		/// designed to facilitate construction of subclasses that provide
		/// more extensive lock monitoring facilities.
		/// </summary>
		/// <returns> the owner, or <c>null</c> if not owned.
		/// </returns>
		protected internal virtual Thread Owner
		{
			get { return sync.Owner; }

		}

		/// <summary> 
		/// Returns an estimate of the number of threads waiting to
		/// acquire this lock.  The value is only an estimate because the number of
		/// threads may change dynamically while this method traverses
		/// internal data structures.  This method is designed for use in
		/// monitoring of the system state, not for synchronization
		/// control.
		/// </summary>
		/// <returns> the estimated number of threads waiting for this lock
		/// </returns>
		public virtual int QueueLength
		{
			get { return sync.QueueLength; }

		}


		/// <summary> Creates an instance of <see cref="Spring.Threading.Locks.ReentrantLock"/>.
		/// This is equivalent to using <tt>ReentrantLock(false)</tt>.
		/// </summary>
		public ReentrantLock() : this(false)
		{
		}

		/// <summary> 
		///	Creates an instance of <see cref="Spring.Threading.Locks.ReentrantLock"/> with the
		/// given fairness policy.
		/// </summary>
		/// <param name="fair"><c>true</c> if this lock will be fair, else <c>false</c>
		/// </param>
		public ReentrantLock(bool fair)
		{
			sync = fair ? (Sync) new FairSync() : new NonfairSync();
		}

        /// <summary>
        /// Acquires the lock and returns an <see cref="IDisposable"/> that
        /// can be used to unlock when disposed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Acquires the lock if it is not held by another thread and returns
        /// immediately, setting the lock hold count to one.
        /// </para>
        /// <para>
        /// If the <see cref="Thread.CurrentThread">current thread</see>
        /// already holds the lock then the hold count is incremented by one and
        /// the method returns immediately.
        /// </para>
        /// <para>
        /// If the lock is held by another thread then the
        /// current thread becomes disabled for thread scheduling
        /// purposes and lies dormant until the lock has been acquired,
        /// at which time the lock hold count is set to one.
        /// </para>
        /// <example>
        /// Below is a typical use of <see cref="Lock"/>
        /// <code language="c#">
        /// ReentrantLock reentrantLock = ...;
        /// 
        /// using(reentrantLock.Lock())
        /// {
        ///    // locked
        /// }
        /// // unlocked.
        /// </code>
        /// it is equvilant to
        /// <code language="c#">
        /// ReentrantLock reentrantLock = ...;
        /// 
        /// reentrantLock.Lock();
        /// try {
        ///     // locked
        /// }
        /// finally
        /// {
        ///     reentrantLock.Unlock();
        /// }
        /// // unlocked
        /// </code>
        /// </example>
        /// </remarks>
        /// <returns>
        /// An <see cref="IDisposable"/> object that unlocks current 
        /// <see cref="ReentrantLock"/> when it is disposed.
        /// </returns>
        /// <seealso cref="LockInterruptibly"/>
        public virtual IDisposable Lock()
		{
			sync.Lock();
            return this;
		}

		/// <summary> 
		/// Acquires the lock unless <see cref="System.Threading.Thread.Interrupt()"/> is called on the current thread
		/// 
		/// <p/>
		/// Acquires the lock if it is not held by another thread and returns
		/// immediately, setting the lock hold count to one.
		/// 
		/// <p/>
		/// If the current thread already holds this lock then the hold count
		/// is incremented by one and the method returns immediately.
		/// 
		/// <p/>
		/// If the lock is held by another thread then the
		/// current thread becomes disabled for thread scheduling
		/// purposes and lies dormant until one of two things happens:
		/// 
		/// <list type="bullet">
		/// <item>The lock is acquired by the current thread</item>
		/// <item>Some other thread calls <see cref="System.Threading.Thread.Interrupt()"/> on the current thread.</item>
		/// </list>
		/// 
		/// <p/>If the lock is acquired by the current thread then the lock hold
		/// count is set to one.
		/// 
		/// <p/>If the current thread:
		/// 
		/// <list type="bullet">
		/// <item>has its interrupted status set on entry to this method</item> 
		/// <item><see cref="System.Threading.Thread.Interrupt()"/> is called while acquiring the lock</item>
		/// </list>
		/// 
		/// then <see cref="System.Threading.ThreadInterruptedException"/> is thrown and the current thread's
		/// interrupted status is cleared.
		/// 
		/// <p/>
		/// In this implementation, as this method is an explicit interruption
		/// point, preference is given to responding to the interrupt over normal or reentrant
		/// acquisition of the lock.
		/// </summary>
		/// <exception cref="System.Threading.ThreadInterruptedException">if the current thread is interrupted</exception>
		/// 
        /// <summary>
        /// Acquires the lock unless <see cref="Thread.Interrupt()"/> is called 
        /// on the current thread. Returns an <see cref="IDisposable"/> that
        /// can be used to unlock if the lock is sucessfully obtained.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Acquires the lock if it is not held by another thread and returns
        /// immediately, setting the lock hold count to one.
        /// </para>
        /// <para>
        /// If the current thread already holds this lock then the hold count
        /// is incremented by one and the method returns immediately.
        /// </para>
        /// <para>
        /// If the lock is held by another thread then the
        /// current thread becomes disabled for thread scheduling
        /// purposes and lies dormant until one of two things happens:
        /// </para>
        /// <list type="bullet">
        /// <item>
        /// The lock is acquired by the current thread.
        /// </item>
        /// <item>
        /// Some other thread calls <see cref="Thread.Interrupt()"/> on the current thread.
        /// </item>
        /// </list>
        /// <para>
        /// If the lock is acquired by the current thread then the lock hold
        /// count is set to one.
        /// </para>
        /// <para>
        /// If the current thread:
        /// </para>
        /// <list type="bullet">
        /// <item>has its interrupted status set on entry to this method</item> 
        /// <item><see cref="Thread.Interrupt()"/> is called while acquiring the lock</item>
        /// </list>
        /// <para>
        /// then <see cref="ThreadInterruptedException"/> is thrown and the current thread's
        /// interrupted status is cleared.
        /// </para>
        /// <para>
        /// In this implementation, as this method is an explicit interruption
        /// point, preference is given to responding to the interrupt over normal or reentrant
        /// acquisition of the lock.
        /// </para>
        /// <example>
        /// Below is a typical use of <see cref="LockInterruptibly"/>
        /// <code language="c#">
        /// ReentrantLock reentrantLock = ...;
        /// 
        /// using(reentrantLock.LockInterruptibly())
        /// {
        ///    // locked
        /// }
        /// // unlocked.
        /// </code>
        /// it is equvilant to
        /// <code language="c#">
        /// ReentrantLock reentrantLock = ...;
        /// 
        /// reentrantLock.LockInterruptibly();
        /// try {
        ///     // locked
        /// }
        /// finally
        /// {
        ///     reentrantLock.Unlock();
        /// }
        /// // unlocked
        /// </code>
        /// </example>
        /// </remarks>
        /// <returns>
        /// An <see cref="IDisposable"/> object that unlocks current 
        /// <see cref="ReentrantLock"/> when it is disposed.
        /// </returns>
        /// <exception cref="ThreadInterruptedException">
        /// If the current thread is interrupted.
        /// </exception>
        /// <seealso cref="Lock"/>
        public virtual IDisposable LockInterruptibly()
		{
			sync.LockInterruptibly();
		    return this;
		}

		/// <summary> 
		/// Acquires the lock only if it is not held by another thread at the time
		/// of invocation.
		/// 
		/// <p/>
		/// Acquires the lock if it is not held by another thread and
		/// returns immediately with the value <c>true</c>, setting the
		/// lock hold count to one. Even when this lock has been set to use a
		/// fair ordering policy, a call to <see cref="Spring.Threading.Locks.ReentrantLock.TryLock()"/> <b>will</b>
		/// immediately acquire the lock if it is available, whether or not
		/// other threads are currently waiting for the lock.
		/// This &quot;barging&quot; behavior can be useful in certain
		/// circumstances, even though it breaks fairness. If you want to honor
		/// the fairness setting for this lock, then use
		/// <see cref="Spring.Threading.Locks.ReentrantLock.TryLock(TimeSpan)"/>
		/// which is almost equivalent (it also detects interruption).
		/// 
		/// <p/> 
		/// If the current thread
		/// already holds this lock then the hold count is incremented by one and
		/// the method returns <c>true</c>.
		/// 
		/// <p/>
		/// If the lock is held by another thread then this method will return
		/// immediately with the value <c>false</c>.
		/// 
		/// </summary>
		/// <returns> <c>true</c> if the lock was free and was acquired by the
		/// current thread, or the lock was already held by the current thread,
		/// <c>false</c> otherwise.
		/// </returns>
		public virtual bool TryLock()
		{
			return sync.TryLock();
		}

		/// <summary> 
		/// Acquires the lock if it is not held by another thread within the given
		/// waiting time and <see cref="System.Threading.Thread.Interrupt()"/> hasn't been called on the current thread
		/// 
		/// <p/>
		/// Acquires the lock if it is not held by another thread and returns
		/// immediately with the value <c>true</c>, setting the lock hold count
		/// to one. If this lock has been set to use a fair ordering policy then
		/// an available lock <b>will not</b> be acquired if any other threads
		/// are waiting for the lock. This is in contrast to the <see cref="Spring.Threading.Locks.ReentrantLock.TryLock()"/> 
		/// method. If you want a timed <see cref="Spring.Threading.Locks.ReentrantLock.TryLock()"/> that does permit barging on
		/// a fair lock then combine the timed and un-timed forms together:
		/// 
		/// <code>
		/// if (lock.TryLock() || lock.TryLock(timeSpan) ) { ... }
		/// </code>
		/// 
		/// <p/>
		/// If the current thread
		/// already holds this lock then the hold count is incremented by one and
		/// the method returns <c>true</c>.
		/// 
		/// <p/>
		/// If the lock is held by another thread then the
		/// current thread becomes disabled for thread scheduling
		/// purposes and lies dormant until one of three things happens:
		/// 
		/// <list type="bullet">
		/// <item>The lock is acquired by the current thread</item>	
		/// <item>Some other thread calls <see cref="System.Threading.Thread.Interrupt()"/> the current thread</item>
		/// <item>The specified waiting time elapses</item>
		/// </list>
		/// 
		/// <p/>
		/// If the lock is acquired then the value <c>true</c> is returned and
		/// the lock hold count is set to one.
		/// 
		/// <p/>If the current thread:
		/// <list type="bullet">
		/// <item>has its interrupted status set on entry to this method</item>	
		/// <item>has <see cref="System.Threading.Thread.Interrupt()"/> called on it while acquiring the lock</item>	
		/// </list>
		/// 
		/// then <see cref="System.Threading.ThreadInterruptedException"/> is thrown and the current thread's
		/// interrupted status is cleared.
		/// 
		/// <p/>
		/// If the specified waiting time elapses then the value <c>false</c>
		/// is returned.  If the time is less than or equal to zero, the method will not wait at all.
		/// 
		/// <p/>
		/// In this implementation, as this method is an explicit interruption
		/// point, preference is given to responding to the interrupt over normal or reentrant
		/// acquisition of the lock, and over reporting the elapse of the waiting
		/// time.
		/// 
		/// </summary>
		/// <param name="timeSpan">the <see cref="System.TimeSpan"/> to wait for the lock</param>
		/// <returns> <c>true</c> if the lock was free and was acquired by the
		/// current thread, or the lock was already held by the current thread; and
		/// <c>false</c> if the waiting time elapsed before the lock could be
		/// acquired.
		/// </returns>
		/// <throws>  InterruptedException if the current thread is interrupted </throws>
		/// <throws>  NullPointerException if unit is null </throws>
		/// <exception cref="System.NullReferenceException">If <paramref name="timeSpan"/> is null</exception>
		/// <exception cref="System.Threading.ThreadInterruptedException">If the current thread is interrupted</exception>
		public virtual bool TryLock(TimeSpan timeSpan)
		{
			return sync.TryLock(timeSpan);
		}

		/// <summary> 
		/// Attempts to release this lock.
		/// <p/>
		/// If the current thread is the
		/// holder of this lock then the hold count is decremented. If the
		/// hold count is now zero then the lock is released.  If the
		/// current thread is not the holder of this lock then <see cref="System.InvalidOperationException"/> is thrown.
		/// </summary>
		/// <exception cref="System.Threading.SynchronizationLockException">if the current thread does not hold this lock.</exception>
		public virtual void Unlock()
		{
			sync.Unlock();
		}

		/// <summary> 
		/// Returns a <see cref="Spring.Threading.Locks.ICondition"/> instance for use with this
		/// <see cref="Spring.Threading.Locks.ILock"/> instance.
		/// 
		/// <p/>
		/// The returned <see cref="Spring.Threading.Locks.ICondition"/> instance supports the same
		/// usages as do the <see cref="System.Threading.Monitor"/> methods <see cref="System.Threading.Monitor.Wait(object)"/>,
		/// <see cref="System.Threading.Monitor.Pulse(object)"/>, and <see cref="System.Threading.Monitor.PulseAll(object)"/>) when used with the built-in
		/// monitor lock.
		/// <list type="bullet">
		/// <item>
		/// If this lock is not held when either 
		/// <see cref="Spring.Threading.Locks.ICondition.Await()"/> or <see cref="Spring.Threading.Locks.ICondition.Signal()"/>
		/// methods are called, then an <see cref="System.InvalidOperationException"/>  is thrown.</item>
		/// <item>When the condition <see cref="Spring.Threading.Locks.ICondition"/>await() waiting}
		/// methods are called the lock is released and, before they
		/// return, the lock is reacquired and the lock hold count restored
		/// to what it was when the method was called.</item>
		/// <item>If <see cref="System.Threading.Thread.Interrupt()"/> is called while
		/// waiting then the wait will terminate, an <see cref="System.Threading.ThreadInterruptedException"/>
		/// and the thread's interrupted status will be cleared.</item>
		/// <item> Waiting threads are signalled in FIFO order</item>
		/// <item>
		/// The ordering of lock reacquisition for threads returning
		/// from waiting methods is the same as for threads initially
		/// acquiring the lock, which is in the default case not specified,
		/// but for <b>fair</b> locks favors those threads that have been
		/// waiting the longest.</item>
		/// </list>
		/// </summary>
		/// <returns> the ICondition object
		/// </returns>
		public virtual ICondition NewCondition()
		{
			return IsFair ? new FIFOConditionVariable(this) : new ConditionVariable(this);
		}

		/// <summary> 
		/// Queries whether any threads are waiting to acquire this lock. Note that
		/// because cancellations may occur at any time, a <c>true</c>
		/// return does not guarantee that any other thread will ever
		/// acquire this lock.  This method is designed primarily for use in
		/// monitoring of the system state.
		/// </summary>
		/// <returns> <c>true</c>if there may be other threads waiting to acquire
		/// the lock, <c>false</c> otherwise.
		/// </returns>
		public bool HasQueuedThreads
		{
			get { return sync.HasQueuedThreads; }
		}


		/// <summary> 
		/// Queries whether the <paramref name="thread"/> is waiting to acquire this
		/// lock. Note that because cancellations may occur at any time, a
		/// <c>true</c> return does not guarantee that this thread
		/// will ever acquire this lock.  This method is designed primarily for use
		/// in monitoring of the system state.
		/// </summary>
		/// <param name="thread">the <see cref="System.Threading.Thread"/> instance.
		/// </param>
		/// <returns> <c>true</c> if the given thread is queued waiting for this lock, <c>false</c> otherwise.
		/// </returns>
		/// <throws>  NullPointerException if thread is null </throws>
		/// <exception cref="System.NullReferenceException">if <paramref name="thread"/> is null.</exception>
		public bool IsQueuedThread(Thread thread)
		{
			return sync.IsQueued(thread);
		}

		/// <summary> 
		///	Returns a collection containing threads that may be waiting to
		/// acquire this lock.  Since the actual set of threads may change
		/// dynamically while constructing this result, the returned
		/// collection is only a best-effort estimate.  The elements of the
		/// returned collection are in no particular order.  This method is
		/// designed to facilitate construction of subclasses that provide
		/// more extensive monitoring facilities.
		/// </summary>
		/// <returns> collection of threads
		/// </returns>
		public virtual ICollection<Thread> QueuedThreads
		{
			get { return sync.QueuedThreads; }
		}

		/// <summary> 
		/// Queries whether any threads are waiting on the <paramref name="condition"/>
		/// associated with this lock. Note that because timeouts and
		/// interrupts may occur at any time, a <c>true</c> return does
		/// not guarantee that a future <tt>signal</tt> will awaken any
		/// threads.  This method is designed primarily for use in
		/// monitoring of the system state.
		/// </summary>
		/// <param name="condition">the condition</param>
		/// <returns> <c>true</c> if there are any waiting threads.</returns>
		/// <exception cref="System.NullReferenceException">if the <paramref name="condition"/> is null</exception>
		/// <exception cref="System.ArgumentException">if the <paramref name="condition"/> is not associated with this lock</exception>
		public virtual bool HasWaiters(ICondition condition)
		{
			return AsConditionVariable(condition).HasWaiters;
		}

		/// <summary> 
		/// Returns an estimate of the number of threads waiting on the
		/// <paramref name="condition"/> associated with this lock. Note that because
		/// timeouts and interrupts may occur at any time, the estimate
		/// serves only as an upper bound on the actual number of waiters.
		/// This method is designed for use in monitoring of the system
		/// state, not for synchronization control.
		/// </summary>
		/// <param name="condition">the condition</param>
		/// <returns> the estimated number of waiting threads.</returns>
		/// <exception cref="System.NullReferenceException">if the <paramref name="condition"/> is null</exception>
		/// <exception cref="System.ArgumentException">if the <paramref name="condition"/> is not associated with this lock</exception>
		public virtual int GetWaitQueueLength(ICondition condition)
		{
			return AsConditionVariable(condition).WaitQueueLength;
		}

		/// <summary> 
		/// Returns a collection containing those threads that may be
		/// waiting on the <paramref name="condition"/> associated with this lock.
		/// Because the actual set of threads may change dynamically while
		/// constructing this result, the returned collection is only a
		/// best-effort estimate. The elements of the returned collection
		/// are in no particular order.  This method is designed to
		/// facilitate construction of subclasses that provide more
		/// extensive condition monitoring facilities.
		/// </summary>
		/// <param name="condition">the condition</param>
		/// <returns> the collection of threads waiting on <paramref name="condition"/></returns>
		/// <exception cref="System.NullReferenceException">if the <paramref name="condition"/> is null</exception>
		/// <exception cref="System.ArgumentException">if the <paramref name="condition"/> is not associated with this lock</exception>
		protected virtual ICollection<Thread> GetWaitingThreads(ICondition condition)
		{
			return AsConditionVariable(condition).WaitingThreads;
		}

		/// <summary> 
		/// Returns a string identifying this lock, as well as its lock
		/// state.  The state, in brackets, includes either the string
		/// 'Unlocked' or the string 'Locked by'
		/// followed by the <see cref="System.Threading.Thread.Name"/>  of the owning thread.
		/// </summary>
		/// <returns> a string identifying this lock, as well as its lock state.</returns>
		public override string ToString()
		{
			Thread o = Owner;
			return base.ToString() + ((o == null) ? "[Unlocked]" : "[Locked by thread " + o.Name + "]");
		}

		private ConditionVariable AsConditionVariable(ICondition condition)
		{
			if (condition == null)
				throw new ArgumentNullException("condition");
            ConditionVariable condVar = condition as ConditionVariable;
            if (condVar == null)
				throw new ArgumentException("not owner");
			if (condVar.Lock != this)
				throw new ArgumentException("not owner");
			return condVar;
		}

        #region IDisposable Members

	    void IDisposable.Dispose()
        {
            sync.Unlock();
        }

        #endregion
    }
}