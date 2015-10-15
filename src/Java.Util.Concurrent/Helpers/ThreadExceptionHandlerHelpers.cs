using System;
using System.Threading;

namespace Spring.Threading.Helpers
{
	/// <summary> 
	/// Notification of the uncaught exception that occurred within specified
	/// thread.
	/// </summary>
	/// <param name="thread">the thread where the exception occurred</param>
	/// <param name="exception">the exception</param>
	public delegate void UncaughtExceptionHandlerDelegate(Thread thread, Exception exception);


	/// <summary>
	/// Helper class for providing unhandled exception handlers for <see cref="Spring.Threading.IRunnable"/> instances.
	/// </summary>
	/// <author>Griffin Caprio (.NET)</author>
	public class ThreadExceptionHandlerHelpers
	{
		private class AnonymousClassRunnable : IRunnable
		{
			private IRunnable _runnable;
			private UncaughtExceptionHandlerDelegate _handler;

			public AnonymousClassRunnable(IRunnable runnable, UncaughtExceptionHandlerDelegate handler)
			{
				_runnable = runnable;
				_handler = handler;
			}

			public virtual void Run()
			{
				try
				{
					_runnable.Run();
				}
				catch (Exception error)
				{
					try
					{
						_handler(Thread.CurrentThread, error);
					}
					catch (Exception)
					{
					}
				}
			}
		}

		/// <summary> Returns wrapped runnable that ensures that if an exception occurs
		/// during the execution, the specified exception handler is invoked.
		/// </summary>
		/// <param name="runnable">runnable for which exceptions are to be intercepted
		/// </param>
		/// <param name="handler">the exception handler to call when exception occurs
		/// during execution of the given runnable
		/// </param>
		/// <returns> wrapped runnable
		/// </returns>
		/// <exception cref="System.ArgumentNullException">If either parameter is <c>null</c></exception>
		public static IRunnable AssignExceptionHandler(IRunnable runnable, UncaughtExceptionHandlerDelegate handler)
		{
			if ( runnable == null )
				throw new ArgumentNullException("runnable", "Runnable cannot be null");
			if ( handler == null )
				throw new ArgumentNullException("handler", "Handler cannot be null");
			return new AnonymousClassRunnable(runnable, handler);
		}
	}
}