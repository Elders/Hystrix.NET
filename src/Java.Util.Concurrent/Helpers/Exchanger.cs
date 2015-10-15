using System;
using System.Threading;

namespace Spring.Threading.Helpers
{
	/// <summary> 
	/// A synchronization point at which two threads can exchange objects.
    /// Each thread presents some object on entry to the <see cref="Exchanger.Exchange(object)"/>
	/// method, and receives the object presented by the other
	/// thread on return.
	/// </summary>
	/// <author>Doug Lea</author>
	/// <author>Bill Scherer</author>
	/// <author>Michael Scott</author>
	/// <author>Griffin Caprio (.NET)</author>
	public class Exchanger
	{
		private readonly object _lock = new Object();

		/// <summary>Holder for the item being exchanged </summary>
		private object _itemToBeExchanged;

		/// <summary> 
		/// Arrival count transitions from 0 to 1 to 2 then back to 0
		/// during an exchange.
		/// </summary>
		private int _arrivalCount;

		/// <summary> Main exchange function, handling the different policy variants.</summary>
		private object doExchange(Object objectToExchange, bool timed, TimeSpan duration)
		{
			lock (_lock)
			{
				TimeSpan durationToWait = duration;
				object other;
				DateTime deadline = timed ? DateTime.Now.Add(duration): new DateTime(0);
				while (_arrivalCount == 2)
				{
					if (!timed)
						Monitor.Wait(_lock);
					else if (durationToWait.Ticks > 0)
					{
						Monitor.Wait(_lock, durationToWait);
						durationToWait = deadline.Subtract(DateTime.Now);
					}
					else
						throw new TimeoutException("Timeout waiting for other thread.");
				}

				int count = ++_arrivalCount;

				// If item is already waiting, replace it and signal other thread
				if (count == 2)
				{
					other = _itemToBeExchanged;
					_itemToBeExchanged = objectToExchange;
					Monitor.Pulse(_lock);
					return other;
				}

				// Otherwise, set item and wait for another thread to
				// replace it and signal us.

				_itemToBeExchanged = objectToExchange;
				ThreadInterruptedException interrupted = null;
				try
				{
					while (_arrivalCount != 2)
					{
						if (!timed)
							Monitor.Wait(_lock);
						else if (durationToWait.Ticks > 0)
						{
							Monitor.Wait(_lock, durationToWait);

							durationToWait = deadline.Subtract(DateTime.Now);
						}
						else
							break; // timed out
					}
				}
				catch (ThreadInterruptedException ie)
				{
					interrupted = ie;
				}

				// Get and reset item and count after the wait.
				// (We need to do this even if wait was aborted.)
				other = _itemToBeExchanged;
				_itemToBeExchanged = null;
				count = _arrivalCount;
				_arrivalCount = 0;
				Monitor.Pulse(_lock);

				// If the other thread replaced item, then we must
				// continue even if cancelled.
				if (count == 2)
				{
					if (interrupted != null)
						Thread.CurrentThread.Interrupt();
					return other;
				}

				// If no one is waiting for us, we can back out
				if (interrupted != null)
					throw interrupted;
					// must be timeout
			    throw new TimeoutException();
			}
		}

		

		/// <summary> 
		/// Waits for another thread to arrive at this exchange point (unless
		/// <see cref="System.Threading.Thread.Interrupt()"/> is called,
		/// and then transfers the given object to it, receiving its object
		/// in return.
		/// </summary>
		/// <remarks> 
		/// If another thread is already waiting at the exchange point then
		/// it is resumed for thread scheduling purposes and receives the object
		/// passed in by the current thread. The current thread returns immediately,
		/// receiving the object passed to the exchange by that other thread.
		/// 
		/// <p/>
		/// If no other thread is already waiting at the exchange then the
		/// current thread is disabled for thread scheduling purposes and lies
		/// dormant until one of two things happens:
		/// <ul>
		/// <li>Some other thread enters the exchange</li>
		/// <li><see cref="System.Threading.Thread.Interrupt"/> is called on the current thread.</li>
		/// </ul>
		/// <p/>
		/// If <see cref="System.Threading.Thread.Interrupt()"/> is called on the current thread,
		/// then a <see cref="System.Threading.ThreadInterruptedException"/> is thrown
		/// </remarks>
		/// <param name="objectToExchange">the object to exchange</param>
		/// <returns> the object provided by the other thread.</returns>
		/// <exception cref="System.Threading.ThreadInterruptedException">if the current thread was interrupted</exception>
		public virtual object Exchange(object objectToExchange)
		{
			return doExchange(objectToExchange, false, new TimeSpan(0));
		}

		/// <summary> 
		/// Waits for another thread to arrive at this exchange point (unless
		/// <see cref="System.Threading.Thread.Interrupt()"/> is called, or the specified 
		/// <paramref name="duration"/> elapses, and then transfers the given object to it, receiving its object
		/// in return.
		/// </summary>
		/// <remarks> 
		/// If another thread is already waiting at the exchange point then
		/// it is resumed for thread scheduling purposes and receives the object
		/// passed in by the current thread. The current thread returns immediately,
		/// receiving the object passed to the exchange by that other thread.
		/// 
		/// <p/>
		/// If no other thread is already waiting at the exchange then the
		/// current thread is disabled for thread scheduling purposes and lies
		/// dormant until one of two things happens:
		/// <ul>
		/// <li>Some other thread enters the exchange</li>
		/// <li><see cref="System.Threading.Thread.Interrupt"/> is called on the current thread.</li>
		/// </ul>
		/// <p/>
		/// If <see cref="System.Threading.Thread.Interrupt()"/> is called on the current thread,
		/// then a <see cref="System.Threading.ThreadInterruptedException"/> is thrown
		/// </remarks>
		/// <param name="objectToExchange">the object to exchange</param>
		/// <param name="duration">Duration to wait for another thread to enter.</param>
		/// <returns> the object provided by the other thread.</returns>
		/// <exception cref="System.Threading.ThreadInterruptedException">if the current thread was interrupted</exception>
		public virtual object Exchange(object objectToExchange, TimeSpan duration)
		{
			return doExchange(objectToExchange, true, duration);
		}
	}
}