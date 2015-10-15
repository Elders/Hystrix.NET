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
using System.Threading;

namespace Java.Util.Concurrent.Locks
{
	/// <author>Doug Lea</author>
	/// <author>Griffin Caprio (.NET)</author>
    /// <author>Kenneth Xu</author>
    [Serializable]
    internal class ConditionVariable : ICondition // BACKPORT_3_1
    {
	    private const string _notSupportedMessage = "Use FAIR version";
	    protected internal IExclusiveLock _lock;

        /// <summary> 
        /// Create a new <see cref="ConditionVariable"/> that relies on the given mutual
        /// exclusion lock.
        /// </summary>
        /// <param name="lock">
        /// A non-reentrant mutual exclusion lock.
        /// </param>
        internal ConditionVariable(IExclusiveLock @lock)
        {
            _lock = @lock;
        }

        protected internal virtual IExclusiveLock Lock
        {
            get { return _lock; }
        }

        protected internal virtual int WaitQueueLength
        {
            get { throw new NotSupportedException(_notSupportedMessage); }
        }

        protected internal virtual ICollection<Thread> WaitingThreads
        {
            get { throw new NotSupportedException(_notSupportedMessage); }
        }

        protected internal virtual bool HasWaiters
        {
            get { throw new NotSupportedException(_notSupportedMessage); }
        }

        #region ICondition Members

        public virtual void AwaitUninterruptibly()
        {
            int holdCount = _lock.HoldCount;
            if (holdCount == 0)
            {
                throw new SynchronizationLockException();
            }
            bool wasInterrupted = false;
            try
            {
                lock (this)
                {
                    for (int i = holdCount; i > 0; i--) _lock.Unlock();
                    try
                    {
                        Monitor.Wait(this);
                    }
                    catch (ThreadInterruptedException)
                    {
                        wasInterrupted = true;
                        // may have masked the signal and there is no way
                        // to tell; we must wake up spuriously.
                    }
                }
            }
            finally
            {
                for (int i = holdCount; i > 0; i--) _lock.Lock();
                if (wasInterrupted)
                {
                    Thread.CurrentThread.Interrupt();
                }
            }
        }

        public virtual void Await()
        {
            int holdCount = _lock.HoldCount;
            if (holdCount == 0)
            {
                throw new SynchronizationLockException();
            }
            // This requires sleep(0) to implement in .Net, too expensive!
            // if (Thread.interrupted()) throw new InterruptedException();
            try
            {
                lock (this)
                {
                    for (int i = holdCount; i > 0; i--) _lock.Unlock();
                    try
                    {
                        Monitor.Wait(this);
                    }
                    catch (ThreadInterruptedException e)
                    {
                        Monitor.Pulse(this);
                        throw SystemExtensions.PreserveStackTrace(e);
                    }
                }
            }
            finally
            {
                for (int i = holdCount; i > 0; i--) _lock.Lock();
            }
        }

        public virtual bool Await(TimeSpan durationToWait)
        {
            int holdCount = _lock.HoldCount;
            if (holdCount == 0)
            {
                throw new SynchronizationLockException();
            }
            // This requires sleep(0) to implement in .Net, too expensive!
            // if (Thread.interrupted()) throw new InterruptedException();
            try
            {
                lock (this)
                {
                    for (int i = holdCount; i > 0; i--) _lock.Unlock();
                    try
                    {
                        // .Net implementation is a little different than backport 3.1 
                        // by taking advantage of the return value from Monitor.Wait.
                        return (durationToWait.Ticks > 0) && Monitor.Wait(this, durationToWait);
                    }
                    catch (ThreadInterruptedException e)
                    {
                        Monitor.Pulse(this);
                        throw SystemExtensions.PreserveStackTrace(e);
                    }
                }
            }
            finally
            {
                for (int i = holdCount; i > 0; i--) _lock.Lock();
            }
        }

        public virtual bool AwaitUntil(DateTime deadline)
        {
            int holdCount = _lock.HoldCount;
            if (holdCount == 0)
            {
                throw new SynchronizationLockException();
            }
            if ((deadline.Subtract(DateTime.UtcNow)).Ticks <= 0) return false;
            // This requires sleep(0) to implement in .Net, too expensive!
            // if (Thread.interrupted()) throw new InterruptedException();
            try
            {
                lock (this)
                {
                    for (int i = holdCount; i > 0; i--) _lock.Unlock();
                    try
                    {
                        // .Net has DateTime precision issue so we need to retry.
                        TimeSpan durationToWait;
                        while ((durationToWait = deadline.Subtract(DateTime.UtcNow)).Ticks > 0)
                        {
                            // .Net implementation is different than backport 3.1 
                            // by taking advantage of the return value from Monitor.Wait.
                            if (Monitor.Wait(this, durationToWait)) return true;
                        }
                    }
                    catch (ThreadInterruptedException e)
                    {
                        Monitor.Pulse(this);
                        throw SystemExtensions.PreserveStackTrace(e);
                    }
                }
            }
            finally
            {
                for (int i = holdCount; i > 0; i--) _lock.Lock();
            }
            return false;
        }

        public virtual void Signal()
        {
            lock (this)
            {
                AssertOwnership();
                Monitor.Pulse(this);
            }
        }


        public virtual void SignalAll()
        {
            lock (this)
            {
                AssertOwnership();
                Monitor.PulseAll(this);
            }
        }

        #endregion

        protected void AssertOwnership()
        {
            if (!_lock.IsHeldByCurrentThread)
            {
                throw new SynchronizationLockException();
            }
        }

        internal interface IExclusiveLock : ILock
        {
            bool IsHeldByCurrentThread { get; }
            int HoldCount { get; }
        }
    }
}