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

namespace Java.Util.Concurrent
{
	/// <summary> 
	/// A <see cref="ICompletionService{T}"/> that uses a supplied <see cref="IExecutor"/>
	/// to execute tasks.  
	/// </summary>
	/// <remarks>
	/// <para>
	/// This class arranges that submitted tasks are, upon completion, placed 
	/// on a queue accessible using <see cref="Take()"/>. The class is 
	/// lightweight enough to be suitable for transient use when processing 
	/// groups of tasks.
	/// </para>
	/// <example>
	/// Usage Examples.
	/// <para>
	/// Suppose you have a set of solvers for a certain problem, each 
	/// returning a value of some type <c>Result</c>, and would like to run 
	/// them concurrently, processing the results of each of them that
	/// return a non-null value, in some method <c>Use(Result r)</c>. You
	/// could write this as:
	/// </para>
	/// <code language="c#">
    ///   void Solve(IExecutor e,
    ///              ICollection&lt;ICallable&lt;Result&gt;&gt; solvers)
    ///   {
    ///       ICompletionService&lt;Result&gt; ecs
    ///           = new ExecutorCompletionService&lt;Result&gt;(e);
    ///       foreach (ICallable&lt;Result&gt; s in solvers)
    ///           ecs.Submit(s);
    ///       int n = solvers.size();
    ///       for (int i = 0; i &lt; n; ++i) {
    ///           Result r = ecs.Take().GetResult();
    ///           if (r != null) Use(r);
    ///       }
    ///   }
    /// </code>
    /// <para>
	/// Suppose instead that you would like to use the first non-null result
	/// of the set of tasks, ignoring any that encounter exceptions,
	/// and cancelling all other tasks when the first one is ready:
	/// </para>
	/// <code language="c#">
    ///   void Solve(IExecutor e,
    ///              ICollection&lt;ICallable&lt;Result&gt;&gt; solvers)
    ///   {
    ///       ICompletionService&lt;Result&gt; ecs
    ///           = new ExecutorCompletionService&lt;Result&gt;(e);
    ///       int n = solvers.Count;
    ///       IList&lt;IFuture&lt;Result&gt;&gt; futures
    ///           = new List&lt;IFuture&lt;Result&gt;&gt;(n);
    ///       Result result = null;
    ///       try {
    ///           foreach (ICallable&lt;Result&gt; s in solvers)
    ///               futures.Add(ecs.Submit(s));
    ///           for (int i = 0; i &lt; n; ++i) {
    ///               try {
    ///                   Result r = ecs.Take().GetResult();
    ///                   if (r != null) {
    ///                       result = r;
    ///                       break;
    ///                   }
    ///               } catch (ExecutionException ignore) {}
    ///           }
    ///       }
    ///       finally {
    ///           for (IFuture&lt;Result&gt; f : futures)
    ///               f.Cancel(true);
    ///       }
    ///
    ///       if (result != null)
    ///           Use(result);
    ///   }
    /// </code>
	/// </example>
	/// </remarks>
	/// <author>Doug Lea</author>
	/// <author>Griffin Caprio (.NET)</author>
	/// <author>Kenneth Xu (.NET)</author>
	public class ExecutorCompletionService<T> : ICompletionService<T> //JDK_1_6
	{
		private readonly IExecutor _executor;
	    private readonly AbstractExecutorService _aes;
		private readonly IBlockingQueue<IFuture<T>> _completionQueue;

        private class NoContextCarrier : IContextCarrier
        {
            public static readonly NoContextCarrier Instance = new NoContextCarrier();
            private NoContextCarrier() {}
            public void Restore() {}
        }

        /// <summary>
        /// <see cref="FutureTask{T}"/> extension to enqueue upon completion
        /// </summary>
		private class QueueingFuture : FutureTask<Void>
		{
			private readonly ExecutorCompletionService<T> _enclosingInstance;
		    private readonly IFuture<T> _task;

            internal QueueingFuture(ExecutorCompletionService<T> enclosingInstance, IRunnableFuture<T> task)
                : base(task, null)
			{
				_enclosingInstance = enclosingInstance;
                _task = task;
                var contextCopyingTask = task as IContextCopyingTask;
                if (contextCopyingTask != null && contextCopyingTask.ContextCarrier != null)
                {
                    // The task is already copying the context, so we don't do it again.
                    ((IContextCopyingTask) this).ContextCarrier = NoContextCarrier.Instance;
                }
			}

			protected internal override void Done()
			{
				_enclosingInstance._completionQueue.Add(_task);
			}
		}

        private IRunnableFuture<T> NewTaskFor(ICallable<T> task)
        {
            return _aes == null ? new FutureTask<T>(task) : _aes.NewTaskFor(task);
        }

	    private IRunnableFuture<T> NewTaskFor(Func<T> task)
	    {
	        return _aes == null ? new FutureTask<T>(task) : _aes.NewTaskFor(task);
	    }

	    private IRunnableFuture<T> NewTaskFor(IRunnable task, T result)
	    {
	        return _aes == null ? new FutureTask<T>(task, result) : _aes.NewTaskFor(task, result);
	    }

	    private IRunnableFuture<T> NewTaskFor(Action action, T result)
	    {
	        return _aes == null ? new FutureTask<T>(action, result) : _aes.NewTaskFor(action, result);
	    }

	    /// <summary> 
		/// Creates an <see cref="ExecutorCompletionService{T}"/> using the supplied
		/// executor for base task execution and a
		/// <see cref="LinkedBlockingQueue{T}"/> as a completion queue.
		/// </summary>
		/// <param name="executor">the executor to use</param>
		/// <exception cref="System.ArgumentNullException">
		/// if the executor is null
		/// </exception>
		public ExecutorCompletionService(IExecutor executor) 
            : this( executor, new LinkedBlockingQueue<IFuture<T>>())
		{
		}

		/// <summary> 
		/// Creates an <see cref="ExecutorCompletionService{T}"/> using the supplied
		/// executor for base task execution and the supplied queue as its
		/// completion queue.
		/// </summary>
		/// <param name="executor">the executor to use</param>
		/// <param name="completionQueue">the queue to use as the completion queue
		/// normally one dedicated for use by this service
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// if the executor is null
		/// </exception>
		public ExecutorCompletionService(IExecutor executor, IBlockingQueue<IFuture<T>> completionQueue)
		{
			if (executor == null)
				throw new ArgumentNullException("executor", "Executor cannot be null.");
			if (completionQueue == null)
				throw new ArgumentNullException("completionQueue", "Completion Queue cannot be null.");
			_executor = executor;
            _aes = executor as AbstractExecutorService;
			_completionQueue = completionQueue;
		}

	    /// <summary> 
	    ///	Submits a value-returning task for execution and returns an instance 
	    /// of <see cref="IFuture{T}"/> representing the pending results of the 
	    /// task. The future's <see cref="IFuture{T}.GetResult()"/> method will 
	    /// return the callable's result upon successful completion.
	    /// </summary>
	    /// <remarks>
	    /// <para>
	    /// If you would like to immediately block waiting for a callable, you 
	    /// can use constructions of the form
	    /// <code language="c#">
	    ///   result = exec.Submit(aCallable).GetResult();
	    /// </code>
	    /// </para>
	    /// </remarks>
	    /// <param name="callable">The task to submit.</param>
	    /// <returns>
	    /// A <see cref="IFuture{T}"/> representing pending completion of the 
	    /// task.
	    /// </returns>
	    /// <exception cref="RejectedExecutionException">
	    /// If the task cannot be accepted for execution.
	    /// </exception>
	    /// <exception cref="ArgumentNullException">
	    /// If the command is null.
	    /// </exception>
	    public virtual IFuture<T> Submit(ICallable<T> callable)
		{
            return DoSubmit(NewTaskFor(callable));
		}

	    /// <summary> 
	    ///	Submits a value-returning task for execution and returns an instance 
	    /// of <see cref="IFuture{T}"/> representing the pending results of the 
	    /// task. The future's <see cref="IFuture{T}.GetResult()"/> method will 
	    /// return the callable's result upon successful completion.
	    /// </summary>
	    /// <remarks>
	    /// <para>
	    /// If you would like to immediately block waiting for a callable, you 
	    /// can use constructions of the form
	    /// <code language="c#">
	    ///   result = exec.Submit(aCallable).GetResult();
	    /// </code>
	    /// </para>
	    /// </remarks>
	    /// <param name="call">The task to submit.</param>
	    /// <returns>
	    /// A <see cref="IFuture{T}"/> representing pending completion of the 
	    /// task.
	    /// </returns>
	    /// <exception cref="RejectedExecutionException">
	    /// If the task cannot be accepted for execution.
	    /// </exception>
	    /// <exception cref="ArgumentNullException">
	    /// If the command is null.
	    /// </exception>
	    public virtual IFuture<T> Submit(Func<T> call)
        {
            return DoSubmit(NewTaskFor(call));
        }

	    /// <summary> 
	    /// Submits a <see cref="IRunnable"/> task for execution and returns a 
	    /// <see cref="IFuture{T}"/> representing that task.  Upon completion, 
	    /// this task may be taken or polled.
	    /// </summary>
	    /// <param name="runnable">The task to submit.</param>
	    /// <param name="result">
	    /// The result to return upon successful completion.
	    /// </param>
	    /// <returns>
	    /// A <see cref="IFuture{T}"/> representing pending completion of the 
	    /// task, and whose <see cref="IFuture{T}.GetResult()"/> method will 
	    /// return the given result value upon completion.
	    /// </returns>
	    /// <exception cref="RejectedExecutionException">
	    /// If the task cannot be accepted for execution.
	    /// </exception>
	    /// <exception cref="ArgumentNullException">
	    /// If the command is null.
	    /// </exception>
	    public virtual IFuture<T> Submit(IRunnable runnable, T result)
		{
            return DoSubmit(NewTaskFor(runnable, result));
		}

	    /// <summary> 
	    /// Submits a <see cref="Action"/> task for execution and returns a 
	    /// <see cref="IFuture{T}"/> representing that task.  Upon completion, 
	    /// this task may be taken or polled.
	    /// </summary>
	    /// <param name="action">The task to submit.</param>
	    /// <param name="result">
	    /// The result to return upon successful completion.
	    /// </param>
	    /// <returns>
	    /// A <see cref="IFuture{T}"/> representing pending completion of the 
	    /// task, and whose <see cref="IFuture{T}.GetResult()"/> method will 
	    /// return the given result value upon completion.
	    /// </returns>
	    /// <exception cref="RejectedExecutionException">
	    /// If the task cannot be accepted for execution.
	    /// </exception>
	    /// <exception cref="ArgumentNullException">
	    /// If the command is null.
	    /// </exception>
	    public virtual IFuture<T> Submit(Action action, T result)
        {
            return DoSubmit(NewTaskFor(action, result));
        }

	    /// <summary> 
	    /// Submits a <see cref="IRunnable"/> task for execution and returns 
	    /// a <see cref="IFuture{T}"/> representing that task.  The future's
	    /// <see cref="IFuture{T}.GetResult()"/> will return <c>default(T)</c>
	    /// upon successful completion.
	    /// </summary>
	    /// <param name="runnable">The task to submit.</param>
	    /// <returns>
	    /// A <see cref="IFuture{T}"/> representing pending completion of the 
	    /// task, and whose <see cref="IFuture{T}.GetResult()"/> method will 
	    /// return the given result value upon completion.
	    /// </returns>
	    /// <exception cref="RejectedExecutionException">
	    /// If the task cannot be accepted for execution.
	    /// </exception>
	    /// <exception cref="ArgumentNullException">
	    /// If the command is null.
	    /// </exception>
	    public virtual IFuture<T> Submit(IRunnable runnable)
        {
            return DoSubmit(NewTaskFor(runnable, default(T)));
        }

	    /// <summary> 
	    /// Submits a <see cref="Action"/> task for execution and returns a 
	    /// <see cref="IFuture{T}"/> representing that task.  The future's
	    /// <see cref="IFuture{T}.GetResult()"/> will return <c>default(T)</c>
	    /// upon successful completion.
	    /// </summary>
	    /// <param name="action">The task to submit.</param>
	    /// <returns>
	    /// A <see cref="IFuture{T}"/> representing pending completion of the 
	    /// task, and whose <see cref="IFuture{T}.GetResult()"/> method will 
	    /// return the given result value upon completion.
	    /// </returns>
	    /// <exception cref="RejectedExecutionException">
	    /// If the task cannot be accepted for execution.
	    /// </exception>
	    /// <exception cref="ArgumentNullException">
	    /// If the command is null.
	    /// </exception>
	    public virtual IFuture<T> Submit(Action action)
        {
            return DoSubmit(NewTaskFor(action, default(T)));
        }

        internal IFuture<T> DoSubmit(IRunnableFuture<T> runnableFuture)
        {
            _executor.Execute(new QueueingFuture(this, runnableFuture));
            return runnableFuture;
        }

	    /// <summary> 
	    /// Retrieves and removes the <see cref="IFuture{T}"/> representing 
	    /// the next completed task, waiting if none are yet present.
	    /// </summary>
	    /// <returns>
	    /// The <see cref="IFuture{T}"/> representing the next completed task.
	    /// </returns>
	    public virtual IFuture<T> Take()
		{
			return _completionQueue.Take();
		}

	    /// <summary> 
	    /// Retrieves and removes the <see cref="IFuture{T}"/> representing 
	    /// the next completed task or <c>null</c> if none are present.
	    /// </summary>
	    /// <returns>
	    /// The <see cref="IFuture{T}"/> representing the next completed task, 
	    /// or <c>null</c> if none are present.
	    /// </returns>
	    public virtual IFuture<T> Poll()
		{
		    IFuture<T> next;
            return _completionQueue.Poll(out next) ? next : null;
		}

	    /// <summary> 
	    /// Retrieves and removes the <see cref="IFuture{T}"/> representing the 
	    /// next completed task, waiting, if necessary, up to the specified 
	    /// duration if none are yet present.
	    /// </summary>
        /// <param name="durationToWait">
	    /// Duration to wait if no completed task is present yet.
	    /// </param>
	    /// <returns> 
	    /// the <see cref="IFuture{T}"/> representing the next completed task or
	    /// <c>null</c> if the specified waiting time elapses before one
	    /// is present.
	    /// </returns>
	    public virtual IFuture<T> Poll(TimeSpan durationToWait)
		{
		    IFuture<T> next;
		    return _completionQueue.Poll(durationToWait, out next) ? next : null;
		}
	}
}