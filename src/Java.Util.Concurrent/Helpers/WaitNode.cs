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

namespace Java.Util.Concurrent.Helpers
{
	/// <summary>
	/// The wait node used by implementations of <see cref="IWaitQueue"/>.
	/// NOTE: this class is NOT present in java.util.concurrent.
	/// </summary>
	/// <author>Doug Lea</author>
	/// <author>Griffin Caprio (.NET)</author>
    /// <author>Kenneth Xu</author>
	internal class WaitNode // was WaitQueue.WaitNode in BACKPORT_3_1
	{
		internal Thread _owner;
		internal bool _waiting = true;
		internal WaitNode _nextWaitNode;

        public WaitNode()
		{
			_owner = Thread.CurrentThread;
		}

        internal virtual Thread Owner
		{
			get { return _owner; }

		}

		internal virtual bool IsWaiting
		{
			get
			{
				return _waiting;
			}
		}

		internal virtual WaitNode NextWaitNode
		{
			get { return _nextWaitNode; }
			set { _nextWaitNode = value; }
		}

		public virtual bool Signal(IQueuedSync sync)
		{
			lock (this)
			{
				bool signalled = _waiting;
				if (signalled)
				{
					_waiting = false;
					Monitor.Pulse(this);
					sync.TakeOver(this);
				}
				return signalled;
			}
		}

        public virtual bool DoTimedWait( IQueuedSync sync, TimeSpan duration)
		{
			lock (this)
			{
			    if (sync.Recheck(this) || !_waiting)
				{
					return true;
				}
			    if (duration.Ticks <= 0)
			    {
			        _waiting = false;
			        return false;
			    }
			    DateTime deadline = DateTime.UtcNow.Add(duration);
			    try
			    {
			        for (;; )
			        {
			            Monitor.Wait(this, duration);
			            if (!_waiting) // definitely signalled
			                return true;
			            duration = deadline.Subtract(DateTime.UtcNow);
			            if (duration.Ticks <= 0) // time out
			            {
			                _waiting = false;
			                return false;
			            }
			        }
			    }
			    catch (ThreadInterruptedException ex)
			    {
			        if (_waiting) // no notification
			        {
			            _waiting = false; // invalidate for the signaller
			            throw SystemExtensions.PreserveStackTrace(ex);
			        }
                    // thread was interrupted after it was notified
			        Thread.CurrentThread.Interrupt();
			        return true;
			    }
			}
		}

        public virtual void DoWait(IQueuedSync sync)
		{
			lock (this)
			{
				if (!sync.Recheck(this))
				{
					try
					{
						while (_waiting) Monitor.Wait(this);
					}
					catch (ThreadInterruptedException ex)
					{
						if (_waiting)
						{
							// no notification
							_waiting = false; // invalidate for the signaller
							throw SystemExtensions.PreserveStackTrace(ex);
						}
					    // thread was interrupted after it was notified
					    Thread.CurrentThread.Interrupt();
					    return;
					}
				}
			}
		}

		public virtual void DoWaitUninterruptibly(IQueuedSync sync)
		{
			lock (this)
			{
				if (!sync.Recheck(this))
				{
					bool wasInterrupted = false;
					while (_waiting)
					{
						try
						{
							Monitor.Wait(this);
						}
						catch (ThreadInterruptedException)
						{
							wasInterrupted = true;
                            // no need to notify; if we were signalled, we
                            // must be not waiting, and we'll act like signalled
                        }
					}
					if (wasInterrupted)
					{
						Thread.CurrentThread.Interrupt();
					}
				}
			}
		}
	}
}
