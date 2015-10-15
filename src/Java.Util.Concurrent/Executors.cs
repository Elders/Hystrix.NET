#region License
/*
* Copyright (C) 2002-2009 the original author or authors.
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
using Java.Util.Concurrent.Atomic;

namespace Java.Util.Concurrent
{
    //TODO PHASED correct the link to Spring.Threading.Execution.IScheduledExecutorService
    /// <summary> 
	/// Factory and utility methods for <see cref="IExecutor"/>, 
	/// <see cref="IExecutorService"/>,
	/// <see cref="T:Spring.Threading.Execution.IScheduledExecutorService"/>,
	/// <see cref="IThreadFactory"/>, and <see cref="ICallable{T}"/> classes
	/// defined in this package.
	/// </summary>
	/// <remarks>
	/// This class supports the following kinds of
	/// methods:
	/// <list type="bullet">
	/// <item>Methods that create and return an <see cref="IExecutorService"/>
	/// set up with commonly useful configuration settings.</item>
	/// <item>Methods that create and return a <see cref="T:Spring.Threading.Execution.IScheduledExecutorService"/>
	/// set up with commonly useful configuration settings.</item>
	/// <item>Methods that create and return a "wrapped"
	/// <see cref="IExecutorService"/>, that disables reconfiguration by
	/// making implementation-specific methods inaccessible.</item>
	/// <item>Methods that create and return a <see cref="IThreadFactory"/>
	/// that sets newly created threads to a known state.</item>
	/// <item>Methods that create and return a <see cref="ICallable{T}"/>
	/// out of other closure-like forms, so they can be used
	/// in execution methods requiring <see cref="ICallable{T}"/>.</item>
	/// </list>
	/// </remarks>
	/// <author>Doug Lea</author>
	/// <author>Griffin Caprio (.NET)</author>
	/// <author>Kenneth Xu</author>
    public static class Executors //JDK_1_6
    {
		#region Public Static Methods
		/// <summary> 
		/// Creates a thread pool that reuses a fixed number of threads
		/// operating off a shared unbounded queue. 
		/// </summary>
		/// <remarks>
		/// At any point, at most
		/// <paramref name="threadPoolSize"/> threads will be active processing tasks. If
		/// additional tasks are submitted when all threads are active,
		/// they will wait in the queue until a thread is available.  If
		/// any thread terminates due to a failure during execution prior
		/// to shutdown, a new one will take its place if needed to execute
		/// subsequent tasks.
		/// </remarks>
		/// <param name="threadPoolSize">the number of threads in the pool</param>
		/// <returns> the newly created thread pool</returns>
		public static IExecutorService NewFixedThreadPool(int threadPoolSize)
		{
            CheckPoolSize(threadPoolSize);
            return new ThreadPoolExecutor(threadPoolSize, threadPoolSize, TimeSpan.Zero, 
                new LinkedBlockingQueue<IRunnable>());
		}

		/// <summary> 
		/// Creates a thread pool that reuses a fixed number of threads
		/// operating off a shared unbounded queue, using the provided
		/// <see cref="Spring.Threading.IThreadFactory"/> to create new threads when needed.  
		/// </summary>
		/// <remarks>
		/// At any point, at most <paramref name="threadPoolSize"/> threads will be active processing
		/// tasks. If additional tasks are submitted when all threads are
		/// active, they will wait in the queue until a thread is
		/// available. If any thread terminates due to a failure during
		/// execution prior to shutdown, a new one will take its place if
		/// needed to execute subsequent tasks.
		/// </remarks>
		/// <param name="threadPoolSize">the number of threads in the pool</param>
		/// <param name="threadFactory">the factory to use when creating new threads</param>
		/// <returns> the newly created thread pool
		/// </returns>
		public static IExecutorService NewFixedThreadPool(int threadPoolSize, IThreadFactory threadFactory)
		{
		    CheckPoolSize(threadPoolSize);
		    return new ThreadPoolExecutor(threadPoolSize, threadPoolSize, TimeSpan.Zero, 
                new LinkedBlockingQueue<IRunnable>(), threadFactory);
		}

	    private static void CheckPoolSize(int threadPoolSize)
	    {
	        if(threadPoolSize <= 0)
	        {
	            throw new ArgumentOutOfRangeException(
                    "threadPoolSize", threadPoolSize, "thread pool size must be greater than zero");
	        }
	    }

	    /// <summary> 
		/// Creates an <see cref="Spring.Threading.IExecutor"/> that uses a single worker thread operating
		/// off an unbounded queue.
		/// </summary>
		/// <remarks>
		/// <b>Note:</b> however that if this single
		/// thread terminates due to a failure during execution prior to
		/// shutdown, a new one will take its place if needed to execute
		/// subsequent tasks.  Tasks are guaranteed to execute
		/// sequentially, and no more than one task will be active at any
		/// given time. Unlike the otherwise equivalent <see cref="Spring.Threading.Execution.Executors.NewFixedThreadPool(int)"/>,
		/// the returned executor is guaranteed not to be reconfigurable to use additional threads.
		/// </remarks>
		/// <returns> the newly created single-threaded <see cref="Spring.Threading.IExecutor"/> </returns>
		public static IExecutorService NewSingleThreadExecutor()
		{
			return new FinalizableDelegatedExecutorService(
                new ThreadPoolExecutor(1, 1, TimeSpan.Zero, new LinkedBlockingQueue<IRunnable>()));
		}

		/// <summary> 
		/// Creates an <see cref="Spring.Threading.IExecutor"/> that uses a single worker thread operating
		/// off an unbounded queue, and uses the provided <see cref="Spring.Threading.IThreadFactory"/> to
		/// create a new thread when needed. 
		/// </summary>
		/// <remarks>
		/// Unlike the otherwise equivalent <see cref="Spring.Threading.Execution.Executors.NewFixedThreadPool(int, IThreadFactory)"/>, the
		/// returned executor is guaranteed not to be reconfigurable to use
		/// additional threads.
		/// </remarks>
		/// <param name="threadFactory">the factory to use when creating new threads</param>
		/// <returns> the newly created single-threaded <see cref="Spring.Threading.IExecutor"/></returns>
		public static IExecutorService NewSingleThreadExecutor(IThreadFactory threadFactory)
		{
			return new FinalizableDelegatedExecutorService(
                new ThreadPoolExecutor(1, 1, TimeSpan.Zero, new LinkedBlockingQueue<IRunnable>(), threadFactory));
		}

		/// <summary> 
		/// Creates a thread pool that creates new threads as needed, but
		/// will reuse previously constructed threads when they are
		/// available.  
		/// </summary>
		/// <remarks>
		/// These pools will typically improve the performance
		/// of programs that execute many short-lived asynchronous tasks.
		/// Calls to <see cref="Spring.Threading.IExecutor.Execute(IRunnable)"/> will reuse previously constructed
		/// threads if available. If no existing thread is available, a new
		/// thread will be created and added to the pool. Threads that have
		/// not been used for sixty seconds are terminated and removed from`
		/// the cache. Thus, a pool that remains idle for long enough will
		/// not consume any resources. <b>Note:</b> pools with similar
		/// properties but different details (for example, timeout parameters)
		/// may be created using <see cref="Spring.Threading.Execution.ThreadPoolExecutor"/> constructors.
		/// </remarks>
		/// <returns>the newly created thread pool</returns>
		public static IExecutorService NewCachedThreadPool()
		{
			return new ThreadPoolExecutor(0, Int32.MaxValue, TimeSpan.FromMinutes(1), 
                new SynchronousQueue<IRunnable>());
		}

		/// <summary> 
		/// Creates a thread pool that creates new threads as needed, but
		/// will reuse previously constructed threads when they are
		/// available, and uses the provided <see cref="Spring.Threading.IThreadFactory"/>  to create new threads when needed.
		/// </summary>
		/// <param name="threadFactory">the factory to use when creating new threads</param>
		/// <returns> the newly created thread pool</returns>
		public static IExecutorService NewCachedThreadPool(IThreadFactory threadFactory)
		{
            return new ThreadPoolExecutor(0, Int32.MaxValue, TimeSpan.FromMinutes(1), 
                new SynchronousQueue<IRunnable>(), threadFactory);
		}

		/// <summary> 
		/// Returns an object that delegates all defined 
		/// <see cref="Spring.Threading.Execution.IExecutorService"/> 
		/// methods to the given executor, but not any
		/// other methods that might otherwise be accessible using
		/// casts. 
		/// </summary>
		/// <remarks>
		/// This provides a way to safely "freeze" configuration and
		/// disallow tuning of a given concrete implementation.
		/// </remarks>
		/// <param name="executor">the underlying implementation</param>
		/// <returns> an <see cref="Spring.Threading.Execution.IExecutorService"/> instance</returns>
		/// <exception cref="System.ArgumentNullException">if <paramref name="executor"/> is null</exception>
		public static IExecutorService UnconfigurableExecutorService(IExecutorService executor)
		{
			if (executor == null)
				throw new ArgumentNullException("executor");
			return new DelegatedExecutorService(executor);
		}

#if !PHASED
		/// <summary> 
		/// Creates a single-threaded executor that can schedule commands
		/// to run after a given delay, or to execute periodically.
		/// </summary>
		/// <remarks>
		/// <b>Note:</b> if this single thread terminates due to a failure during execution prior to
		/// shutdown, a new one will take its place if needed to execute
		/// subsequent tasks.  Tasks are guaranteed to execute
		/// sequentially, and no more than one task will be active at any
		/// given time. Unlike the otherwise equivalent <see cref="Spring.Threading.Execution.Executors.NewScheduledThreadPool(int)"/>
		/// the returned executor is guaranteed not to be reconfigurable to use additional threads.
		/// </remarks>
		/// <returns> the newly created scheduled executor</returns>
		public static IScheduledExecutorService NewSingleThreadScheduledExecutor<T>()
		{
			return new DelegatedScheduledExecutorService(new ScheduledThreadPoolExecutor(1));
		}

		/// <summary> 
		/// Creates a single-threaded executor that can schedule commands
		/// to run after a given delay, or to execute periodically.
		/// </summary>
		/// <remarks>
		/// Note however that if this single thread terminates due to a failure
		/// during execution prior to shutdown, a new one will take its
		/// place if needed to execute subsequent tasks.)  Tasks are
		/// guaranteed to execute sequentially, and no more than one task
		/// will be active at any given time. Unlike the otherwise
		/// equivalent <see cref="Spring.Threading.Execution.Executors.NewScheduledThreadPool(int, IThreadFactory)"/>
		/// the returned executor is guaranteed not to be reconfigurable to
		/// use additional threads.
		/// </remarks>
		/// <param name="threadFactory">the factory to use when creating new threads</param>
		/// <returns> a newly created scheduled executor</returns>
		public static IScheduledExecutorService NewSingleThreadScheduledExecutor<T>(IThreadFactory threadFactory)
		{
			return new DelegatedScheduledExecutorService(new ScheduledThreadPoolExecutor(1, threadFactory));
		}

		/// <summary> 
		/// Creates a thread pool that can schedule commands to run after a
		/// given delay, or to execute periodically.
		/// </summary>
		/// <param name="corePoolSize">the number of threads to keep in the pool, even if they are idle.
		/// </param>
		/// <returns> a newly created scheduled thread pool</returns>
		public static IScheduledExecutorService NewScheduledThreadPool(int corePoolSize)
		{
			return new ScheduledThreadPoolExecutor(corePoolSize);
		}

		/// <summary> 
		/// Creates a thread pool that can schedule commands to run after a
		/// given delay, or to execute periodically and uses the provided <see cref="Spring.Threading.IThreadFactory"/> to
		/// create a new thread when needed. 
		/// </summary>
		/// <param name="corePoolSize">the number of threads to keep in the pool, even if they are idle.</param>
		/// <param name="threadFactory">the factory to use when the executor creates a new thread.</param>
		/// <returns> a newly created scheduled thread pool</returns>
		public static IScheduledExecutorService NewScheduledThreadPool(int corePoolSize, IThreadFactory threadFactory)
		{
			return new ScheduledThreadPoolExecutor(corePoolSize, threadFactory);
		}

		/// <summary> 
		/// Returns an object that delegates all defined <see cref="Spring.Threading.Execution.IScheduledExecutorService"/> 
		/// methods to the given executor, but not any other methods that might otherwise be accessible using
		/// casts. This provides a way to safely "freeze" configuration and
		/// disallow tuning of a given concrete implementation.
		/// </summary>
		/// <param name="executor">the underlying implementation</param>
		/// <returns> a <see cref="Spring.Threading.Execution.IScheduledExecutorService"/> instance</returns>
		/// <exception cref="System.ArgumentNullException">if <paramref name="executor"/> is null</exception>
		public static IScheduledExecutorService UnconfigurableScheduledExecutorService(IScheduledExecutorService executor)
		{
			if (executor == null)
				throw new ArgumentNullException("executor");
			return new DelegatedScheduledExecutorService(executor);
		}
#endif

		/// <summary> 
		/// Returns a default thread factory used to create new threads.
		/// </summary>
		/// <remarks>
		/// This factory creates all new threads used by an <see cref="Spring.Threading.IExecutor"/>.
		/// invoking this <see cref="NewDefaultThreadFactory"/> method.
		/// New threads have names accessible via <see cref="System.Threading.Thread.Name"/> of
		/// <i>pool-N-thread-M</i>, where <i>N</i> is the sequence
		/// number of this factory, and <i>M</i> is the sequence number
		/// of the thread created by this factory.
		/// </remarks>
		/// <returns>a thread factory</returns>
		public static IThreadFactory NewDefaultThreadFactory()
		{
			return new DefaultThreadFactory();
		}

        /// <summary> 
        /// Returns a <see cref="ICallable{T}"/> object that, when
        /// called, runs the given task and returns <c>null</c>.
        /// </summary>
        /// <param name="runnable">the task to run</param>
        /// <returns> a callable object</returns>
        /// <exception cref="System.ArgumentNullException">if the task is <c>null</c></exception>
        public static ICallable<Void> CreateCallable(IRunnable runnable)
        {
            return CreateCallable(runnable, default(Void));
        }

        /// <summary>
        /// Returns a <see cref="ICallable{T}"/> object that, when called, runs 
        /// the given <paramref name="action"/> and returns <c>null</c>.  
        /// </summary>
        /// <param name="action">The task to run.</param>
        /// <returns>An <see cref="ICallable{T}"/> object.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// When the <paramref name="action"/> is <c>null</c>.
        /// </exception>
        /// <seealso cref="CreateCallable{T}(Action,T)"/>
        public static ICallable<Void> CreateCallable(Action action)
        {
            return CreateCallable(action, default(Void));
        }

        /// <summary> 
        /// Returns a <see cref="ICallable{T}"/>  object that, when called, 
        /// runs the given <paramref name="runnable"/> and returns the given 
        /// <paramref name="result"/>.  
        /// </summary>
        /// <remarks>
        /// This can be useful when applying methods requiring a
        /// <see cref="ICallable{T}"/> to an otherwise resultless action.
        /// </remarks>
        /// <typeparam name="T">Type of the result.</typeparam>
        /// <param name="runnable">the task to run</param>
        /// <param name="result">the result to return</param>
        /// <returns>An <see cref="ICallable{T}"/> object.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// When the <paramref name="runnable"/> is <c>null</c>.
        /// </exception>
        /// <seealso cref="CreateCallable{T}(Action,T)"/>
        public static ICallable<T> CreateCallable<T>(IRunnable runnable, T result)
        {
            return new Callable<T>(CreateCall(runnable, result));
        }

        /// <summary>
        /// Returns a <see cref="ICallable{T}"/>  object that, when called, 
        /// runs the given <paramref name="action"/> and returns the given 
        /// <paramref name="result"/>.  
        /// </summary>
        /// <remarks>
        /// This can be useful when applying methods requiring a
        /// <see cref="ICallable{T}"/> to an otherwise resultless action.
        /// </remarks>
        /// <typeparam name="T">Type of the result.</typeparam>
        /// <param name="action">The task to run.</param>
        /// <param name="result">The resul to return</param>
        /// <returns>An <see cref="ICallable{T}"/> object.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// When the <paramref name="action"/> is <c>null</c>.
        /// </exception>
        /// <seealso cref="CreateCallable{T}(IRunnable,T)"/>
        public static ICallable<T> CreateCallable<T>(Action action, T result)
        {
            return new Callable<T>(CreateCall(action, result));
        }

        /// <summary>
        /// Converts a <see cref="Func{T}"/> delegate to an 
        /// <see cref="ICallable{T}"/> object.
        /// </summary>
        /// <typeparam name="T">
        /// Type of the result to be returned by <paramref name="call"/>.
        /// </typeparam>
        /// <param name="call">The <see cref="Func{T}"/></param> delegate.
        /// <returns>An instance of <see cref="ICallable{T}"/>.</returns>
        /// <seealso cref="CreateCall{T}(ICallable{T})"/>
        public static ICallable<T> CreateCallable<T>(Func<T> call)
        {
            return new Callable<T>(call);
        }

        /// <summary> 
        /// Returns a <see cref="Func{T}"/> delegate that, when called, 
        /// runs the given <paramref name="runnable"/> and returns the given 
        /// <paramref name="result"/>.  
        /// </summary>
        /// <remarks>
        /// This can be useful when applying methods requiring a
        /// <see cref="Func{T}"/> to an otherwise resultless action.
        /// </remarks>
        /// <typeparam name="T">Type of the result.</typeparam>
        /// <param name="runnable">the task to run</param>
        /// <param name="result">the result to return</param>
        /// <returns>An <see cref="Func{T}"/> delegate.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// When the <paramref name="runnable"/> is <c>null</c>.
        /// </exception>
        /// <seealso cref="CreateCall{T}(Action,T)"/>
        public static Func<T> CreateCall<T>(IRunnable runnable, T result)
        {
            if (runnable == null) throw new ArgumentNullException("runnable");
            return delegate { runnable.Run(); return result; };
        }

        /// <summary> 
        /// Returns a <see cref="Func{T}"/> delegate that, when called, 
        /// runs the given <paramref name="action"/> and returns the given 
        /// <paramref name="result"/>.  
        /// </summary>
        /// <remarks>
        /// This can be useful when applying methods requiring a
        /// <see cref="Func{T}"/> to an otherwise resultless action.
        /// </remarks>
        /// <typeparam name="T">Type of the result.</typeparam>
        /// <param name="action">the task to run</param>
        /// <param name="result">the result to return</param>
        /// <returns>An <see cref="Func{T}"/> delegate.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// When the <paramref name="action"/> is <c>null</c>.
        /// </exception>
        /// <seealso cref="CreateCall{T}(IRunnable,T)"/>
        public static Func<T> CreateCall<T>(Action action, T result)
        {
            if (action == null) throw new ArgumentNullException("action");
            return delegate { action(); return result; };
        }

        /// <summary>
        /// Converts an <see cref="ICallable{T}"/> object to a 
        /// <see cref="Func{T}"/> delegate.
        /// </summary>
        /// <typeparam name="T">
        /// Type of the result to be returned by <paramref name="callable"/>.
        /// </typeparam>
        /// <param name="callable">The <see cref="ICallable{T}"/></param> object.
        /// <returns>A <see cref="Func{T}"/> delegate.</returns>
        public static Func<T> CreateCall<T>(ICallable<T> callable)
        {
            if (callable == null) throw new ArgumentNullException("callable");
            return callable.Call;
        }

        /// <summary>
        /// Converts a <see cref="Action"/> delegate to an
        /// <see cref="IRunnable"/> object.
        /// </summary>
        /// <param name="action">Task to be converted.</param>
        /// <returns>An <see cref="IRunnable"/> object.</returns>
        public static IRunnable CreateRunnable(Action action)
        {
            return new Runnable(action);
        }


		#endregion

		#region Non-public classes supporting the public methods

        internal class DefaultThreadFactory : IThreadFactory
		{
			internal static readonly AtomicInteger poolNumber = new AtomicInteger(1);
			internal AtomicInteger threadNumber = new AtomicInteger(1);
			internal String namePrefix;

			internal DefaultThreadFactory()
			{
                namePrefix = "pool-" + poolNumber.GetAndIncrement() + "-thread-";
			}

			public virtual Thread NewThread(IRunnable r)
			{
				Thread t = new Thread(r.Run);
                t.Name = namePrefix + threadNumber.GetAndIncrement();
				if (t.IsBackground)
					t.IsBackground = false;
				if (t.Priority != ThreadPriority.Normal)
				{
					t.Priority = ThreadPriority.Normal;
				}
				return t;
			}
		}

		internal class DelegatedExecutorService : IExecutorService
		{
			private readonly IExecutorService _executorService;

			internal DelegatedExecutorService(IExecutorService executor)
			{
				_executorService = executor;
			}

			public virtual bool IsShutdown
			{
				get { return _executorService.IsShutdown; }
			}

			public virtual bool IsTerminated
			{
				get { return _executorService.IsTerminated; }
			}

			public virtual void Execute(IRunnable command)
			{
				_executorService.Execute(command);
			}

            public virtual void Execute(Action action)
            {
                _executorService.Execute(action);
            }

            public virtual void Dispose()
            {
                _executorService.Dispose();
            }

			public virtual void Shutdown()
			{
				_executorService.Shutdown();
			}

			public virtual IList<IRunnable> ShutdownNow()
			{
				return _executorService.ShutdownNow();
			}

			public virtual bool AwaitTermination(TimeSpan duration)
			{
				return _executorService.AwaitTermination(duration);
			}

			public virtual IFuture<Void> Submit(IRunnable task)
			{
				return _executorService.Submit(task);
			}

			public virtual IFuture<T> Submit<T>(ICallable<T> task)
			{
				return _executorService.Submit(task);
			}

			public virtual IFuture<T> Submit<T>(IRunnable task, T result)
			{
				return _executorService.Submit(task, result);
			}

            public virtual IFuture<T> Submit<T>(Func<T> call)
            {
                return _executorService.Submit(call);
            }

            public virtual IFuture<T> Submit<T>(Action action, T result)
            {
                return _executorService.Submit(action, result);
            }

            public virtual IFuture<Void> Submit(Action action)
            {
                return _executorService.Submit(action);
            }

            public virtual IList<IFuture<T>> InvokeAll<T>(IEnumerable<Func<T>> tasks)
            {
                return _executorService.InvokeAll(tasks);
            }

            public virtual IList<IFuture<T>> InvokeAll<T>(TimeSpan durationToWait, IEnumerable<Func<T>> tasks)
            {
                return _executorService.InvokeAll(durationToWait, tasks);
            }

            public virtual IList<IFuture<T>> InvokeAll<T>(IEnumerable<ICallable<T>> tasks)
            {
                return _executorService.InvokeAll(tasks);
            }

            public virtual IList<IFuture<T>> InvokeAll<T>(TimeSpan durationToWait, IEnumerable<ICallable<T>> tasks)
            {
                return _executorService.InvokeAll(durationToWait, tasks);
            }

            public virtual T InvokeAny<T>(IEnumerable<Func<T>> tasks)
            {
                return _executorService.InvokeAny(tasks);
            }

            public virtual T InvokeAny<T>(TimeSpan durationToWait, IEnumerable<Func<T>> tasks)
            {
                return _executorService.InvokeAny(durationToWait, tasks);
            }

            public virtual T InvokeAny<T>(IEnumerable<ICallable<T>> tasks)
            {
                return _executorService.InvokeAny(tasks);
            }

            public virtual T InvokeAny<T>(TimeSpan durationToWait, IEnumerable<ICallable<T>> tasks)
            {
                return _executorService.InvokeAny(durationToWait, tasks);
            }

            public virtual IList<IFuture<T>> InvokeAll<T>(params ICallable<T>[] tasks)
            {
                return _executorService.InvokeAll(tasks);
            }

            public virtual IList<IFuture<T>> InvokeAll<T>(params Func<T>[] tasks)
            {
                return _executorService.InvokeAll(tasks);
            }

            public virtual IList<IFuture<T>> InvokeAll<T>(TimeSpan durationToWait, params ICallable<T>[] tasks)
            {
                return _executorService.InvokeAll(durationToWait, tasks);
            }

            public virtual IList<IFuture<T>> InvokeAll<T>(TimeSpan durationToWait, params Func<T>[] tasks)
            {
                return _executorService.InvokeAll(durationToWait, tasks);
            }

            public virtual IList<IFuture<T>> InvokeAllOrFail<T>(IEnumerable<ICallable<T>> tasks)
            {
                return _executorService.InvokeAllOrFail(tasks);
            }

            public virtual IList<IFuture<T>> InvokeAllOrFail<T>(params ICallable<T>[] tasks)
            {
                return _executorService.InvokeAllOrFail(tasks);
            }

            public virtual IList<IFuture<T>> InvokeAllOrFail<T>(IEnumerable<Func<T>> tasks)
            {
                return _executorService.InvokeAllOrFail(tasks);
            }

            public virtual IList<IFuture<T>> InvokeAllOrFail<T>(params Func<T>[] tasks)
            {
                return _executorService.InvokeAllOrFail(tasks);
            }

            public virtual IList<IFuture<T>> InvokeAllOrFail<T>(TimeSpan durationToWait, IEnumerable<ICallable<T>> tasks)
            {
                return _executorService.InvokeAllOrFail(durationToWait, tasks);
            }

            public virtual IList<IFuture<T>> InvokeAllOrFail<T>(TimeSpan durationToWait, params ICallable<T>[] tasks)
            {
                return _executorService.InvokeAllOrFail(durationToWait, tasks);
            }

            public virtual IList<IFuture<T>> InvokeAllOrFail<T>(TimeSpan durationToWait, IEnumerable<Func<T>> tasks)
            {
                return _executorService.InvokeAllOrFail(durationToWait, tasks);
            }

            public virtual IList<IFuture<T>> InvokeAllOrFail<T>(TimeSpan durationToWait, params Func<T>[] tasks)
            {
                return _executorService.InvokeAllOrFail(durationToWait, tasks);
            }

            public virtual T InvokeAny<T>(params ICallable<T>[] tasks)
            {
                return _executorService.InvokeAny(tasks);
            }

            public virtual T InvokeAny<T>(params Func<T>[] tasks)
            {
                return _executorService.InvokeAny(tasks);
            }

            public virtual T InvokeAny<T>(TimeSpan durationToWait, params ICallable<T>[] tasks)
            {
                return _executorService.InvokeAny(durationToWait, tasks);
            }

            public virtual T InvokeAny<T>(TimeSpan durationToWait, params Func<T>[] tasks)
            {
                return _executorService.InvokeAny(durationToWait, tasks);
            }
		}
        
        internal class FinalizableDelegatedExecutorService : DelegatedExecutorService
        {
            internal FinalizableDelegatedExecutorService(IExecutorService executor)
                : base(executor)
            {
            }

            ~FinalizableDelegatedExecutorService()
            {
                base.Shutdown();
            }
        }

#if !PHASED
       /// <summary> A wrapper class that exposes only the <see cref="Spring.Threading.Execution.IExecutorService"/> and
		/// <see cref="Spring.Threading.Execution.IScheduledExecutorService"/> methods of a <see cref="Spring.Threading.Execution.IScheduledExecutorService"/> implementation.
		/// </summary>
		internal class DelegatedScheduledExecutorService : DelegatedExecutorService, IScheduledExecutorService
		{
			private readonly IScheduledExecutorService e;

			internal DelegatedScheduledExecutorService(IScheduledExecutorService executor) : base(executor)
			{
				e = executor;
			}

			public virtual IScheduledFuture<Void> Schedule(IRunnable command, TimeSpan delay)
			{
				return e.Schedule(command, delay);
			}

			public virtual IScheduledFuture<T> Schedule<T>(ICallable<T> callable, TimeSpan delay)
			{
				return e.Schedule(callable, delay);
			}

			public virtual IScheduledFuture<Void> ScheduleAtFixedRate(IRunnable command, TimeSpan initialDelay, TimeSpan period)
			{
				return e.ScheduleAtFixedRate(command, initialDelay, period);
			}

			public virtual IScheduledFuture<Void> ScheduleWithFixedDelay(IRunnable command, TimeSpan initialDelay, TimeSpan delay)
			{
				return e.ScheduleWithFixedDelay(command, initialDelay, delay);
			}
		}
#endif
		#endregion
	}
}
