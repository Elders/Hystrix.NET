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
using System.Security;
using System.Threading;
using Java.Util.Concurrent.Atomic;
using Java.Util.Concurrent.Locks;

namespace Java.Util.Concurrent
{
    /// <summary>
    /// An <see cref="IExecutorService"/> that executes each submitted task 
    /// using one of possibly several pooled threads, normally configured
    /// using <see cref="Executors"/> factory methods.
    /// </summary> 
    /// <remarks>
    /// <para>
    /// Thread pools address two different problems: they usually provide
    /// improved performance when executing large numbers of asynchronous
    /// tasks, due to reduced per-task invocation overhead, and they provide
    /// a means of bounding and managing the resources, including threads,
    /// consumed when executing a collection of tasks. Each 
    /// <see cref="ThreadPoolExecutor"/> also maintains some basic statistics,
    /// such as the number of completed tasks.
    /// </para>
    /// <para>
    /// To be useful across a wide range of contexts, this class provides
    /// many adjustable parameters and extensibility hooks. However,
    /// programmers are urged to use the more convenient <see cref="Executors"/>
    /// factory methods <see cref="Executors.NewCachedThreadPool()"/>
    /// (unbounded thread pool, with automatic thread reclamation),
    /// <see cref="Executors.NewFixedThreadPool(int)"/> or
    /// <see cref="Executors.NewFixedThreadPool(int, IThreadFactory )"/> (fixed
    /// size thread pool) and <see cref="Executors.NewSingleThreadExecutor()"/>
    /// single background thread), that preconfigure settings for the most
    /// common usage scenarios. Otherwise, use the following guide when
    /// manually configuring and tuning this class:
    /// </para>
    /// <list type="bullet">
    /// <item>
    ///		<term>Core and maximum pool sizes</term>>
    /// 	<description>
    ///         A <see cref="ThreadPoolExecutor"/> will automatically adjustthe
    ///         pool size (<see cref="PoolSize"/>) according to the bounds set
    ///         by Core Pool Size (<see cref="CorePoolSize"/>) and Maximum Pool
    ///         Size (<see cref="MaximumPoolSize"/>)
    /// 		<p/>
    ///			When a new task is submitted in method
    /// 		<see cref="AbstractExecutorService.Execute(Action)"/> or
    ///         <see cref="AbstractExecutorService.Execute(IRunnable)"/>
    /// 		and fewer than <see cref="CorePoolSize"/> threads are running,
    ///         a new thread is created to handle the request, even if other
    ///         worker threads are idle.  If there are more than
    /// 		<see cref="CorePoolSize"/> but less than <see cref="MaximumPoolSize"/>
    /// 		threads running, a new thread will be created only if the queue
    ///         is full.  By setting core pool size and maximum pool size the
    ///         same, you create a fixed-size thread pool. By setting maximum
    ///         pool size to an essentially unbounded value such as
    ///         <see cref="int.MaxValue"/>, you allow the pool to accommodate
    ///         an arbitrary number of concurrent tasks. Most typically, core
    ///         and maximum pool sizes are set only upon construction, but they
    /// 		may also be changed dynamically using <see cref="CorePoolSize"/>
    ///         and <see cref="MaximumPoolSize"/>.
    /// 	</description>
    /// </item>
    /// <item>
    ///		<term>On-demand construction</term>
    ///		<description> 
    ///			By default, even core threads are initially created and started
    ///         only when new tasks arrive, but this can be overridden
    ///         dynamically using method <see cref="PreStartCoreThread"/> or
    /// 		<see cref="PreStartAllCoreThreads()"/>.
    /// 		You probably want to prestart threads if you construct the pool
    ///         with a non-empty queue. 
    /// 	</description>
    /// </item>
    /// <item>
    ///		<term>Creating new threads</term>
    /// 	<description>
    /// 		New threads are created using a <see cref="IThreadFactory"/>.
    /// 		If not otherwise specified, a  <see cref="Executors.DefaultThreadFactory"/>
    ///         is used, that creates threads to all with the same
    ///         <see cref="ThreadPriority"/> set to <see cref="ThreadPriority.Normal"/>
    /// 		priority and non-daemon status. By supplying a different
    ///         <see cref="IThreadFactory"/>, you can alter the thread's name,
    /// 		priority, daemon status, etc. If a <see cref="IThreadFactory"/>
    ///         fails to create a thread when asked by returning null from
    ///         <see cref="IThreadFactory.NewThread(IRunnable)"/>, the executor
    ///         will continue, but might not be able to execute any tasks. 
    /// 	</description>
    /// </item>
    /// <item>
    ///		<term>Keep-alive times</term>
    /// 	<description>
    /// 		If the pool currently has more than <see cref="CorePoolSize"/>
    ///         threads, excess threads will be terminated if they have been
    ///         idle for more than the <see cref="KeepAliveTime"/>. This
    ///         provides a means of reducing resource consumption when the pool
    ///         is not being actively used. If the pool becomes more active
    ///         later, new threads will be constructed. This parameter can
    ///         also be changed dynamically using method
    ///         <see cref="KeepAliveTime"/>. Using a value of
    ///         <see cref="System.Int32.MaxValue"/> effectively disables idle
    ///         threads from ever terminating prior to shut down. By default,
    ///         the keep-alive policy applies only when there are more than
    ///         <see cref="CorePoolSize"/> Threads. But method
    /// 		<see cref="AllowsCoreThreadsToTimeOut"/> can be used to apply
    /// 		this time-out policy to core threads as well, so long as the
    ///         <see cref="KeepAliveTime"/> value is non-zero. 
    /// 	</description>
    /// </item>
    /// <item>
    ///		<term>Queuing</term>
    ///		<description>
    ///			Any <see cref="IBlockingQueue{T}"/> may be used to transfer and
    ///         hold submitted tasks.  The use of this queue interacts with
    ///         pool sizing:
    /// 
    ///			<list type="bullet">
    /// 			<item> 
    /// 				If fewer than <see cref="CorePoolSize"/> threads are
    ///                 running, the Executor always prefers adding a new thread
    ///                 rather than queuing.
    /// 			</item>
    ///				<item> 
    ///					If <see cref="CorePoolSize"/> or more threads are
    ///                 running, the Executor always prefers queuing a request
    ///                 rather than adding a new thread.
    ///				</item>
    ///				<item> 
    ///					If a request cannot be queued, a new thread is created
    ///                 unless this would exceed <see cref="MaximumPoolSize"/>, 
    ///					in which case, the task will be rejected.
    /// 			</item>
    ///			</list>
    /// 
    ///			There are three general strategies for queuing:
    /// 
    ///			<list type="number">
    ///				<item> 
    ///					<term><i>Direct handoffs.</i></term>
    ///                 <description>A good default choice for a
    ///                 work queue is a <see cref="SynchronousQueue{T}"/> that
    ///                 hands off tasks to threads without otherwise holding
    ///                 them. Here, an attempt to queue a task will fail if no 
    ///                 threads are immediately available to run it, so a new 
    ///                 thread will be constructed. This policy avoids lockups 
    ///                 when handling sets of requests that might have internal 
    ///                 dependencies. Direct handoffs generally require 
    ///                 unbounded <see cref="MaximumPoolSize"/> to avoid 
    ///                 rejection of new submitted tasks. This in turn admits 
    ///                 the possibility of unbounded thread growth when commands 
    ///                 continue to arrive on average faster than they can be 
    ///                 processed.</description>
    /// 			</item>
    ///				<item>
    ///					<term><i>Unbounded queues.</i></term>
    ///                 <description>Using an unbounded queue (for
    ///					example a <see cref="LinkedBlockingQueue{T}"/> without 
    ///                 a predefined capacity) will cause new tasks to wait in 
    ///                 the queue when all <see cref="CorePoolSize"/> threads 
    ///                 are busy. Thus, no more than <see cref="CorePoolSize"/>
    /// 				threads will ever be created. (And the value of the 
    /// 				<see cref="MaximumPoolSize"/>
    /// 				therefore doesn't have any effect.)  This may be appropriate when
    /// 				each task is completely independent of others, so tasks cannot
    /// 				affect each others execution; for example, in a web page server.
    /// 				While this style of queuing can be useful in smoothing out
    /// 				transient bursts of requests, it admits the possibility of
    /// 				unbounded work queue growth when commands continue to arrive on
    /// 				average faster than they can be processed.</description>
    /// 			</item>
    ///				<item>
    ///					<term><i>Bounded queues.</i></term>
    ///                 <description>A bounded queue (for example, an
    ///					<see cref="ArrayBlockingQueue{T}"/>) helps prevent 
    ///                 resource exhaustion when used with finite 
    ///                 <see cref="MaximumPoolSize"/>, but can be more difficult to 
    ///                 tune and control.  Queue sizes and maximum pool sizes may be traded
    /// 				off for each other: Using large queues and small pools minimizes
    /// 				CPU usage, OS resources, and context-switching overhead, but can
    /// 				lead to artificially low throughput.  If tasks frequently block (for
    /// 				example if they are I/O bound), a system may be able to schedule
    /// 				time for more threads than you otherwise allow. Use of small queues
    /// 				generally requires larger pool sizes, which keeps CPUs busier but
    /// 				may encounter unacceptable scheduling overhead, which also
    /// 				decreases throughput.</description>
    /// 			</item>
    ///			</list>
    ///		</description>
    /// </item>
    /// <item>
    ///		<term>Rejected tasks</term>
    ///		<description> 
    ///			New tasks submitted in method <see cref="AbstractExecutorService.Execute(Action)"/>
    ///         or <see cref="AbstractExecutorService.Execute(IRunnable)"/>
    ///			will be <i>rejected</i> when the Executor has been shut down, 
    ///         and also when the Executor uses finite bounds for both maximum 
    ///         threads and work queue capacity, and is saturated.  In either 
    ///         case, the <see cref="AbstractExecutorService.Execute(Action)"/>
    ///         or <see cref="AbstractExecutorService.Execute(IRunnable)"/> method invokes the
    /// 		<see cref="IRejectedExecutionHandler.RejectedExecution"/> method of its
    /// 		<see cref="IRejectedExecutionHandler"/>.  Four predefined handler policies
    /// 		are provided:
    /// 		
    ///			<list type="number">
    ///				<item> 
    ///					In the default <see cref="AbortPolicy"/>, the handler throws a
    ///					runtime <see cref="RejectedExecutionException"/> upon rejection. 
    ///				</item>
    ///				<item> 
    ///					In <see cref="CallerRunsPolicy"/>, the thread that invokes
    ///					<see cref="AbstractExecutorService.Execute(Action)"/>
    ///                 or <see cref="AbstractExecutorService.Execute(IRunnable)"/> 
    ///                 itself runs the task. This provides a simple feedback 
    ///                 control mechanism that will slow down the rate that new 
    ///                 tasks are submitted. 
    /// 			</item>
    ///				<item> 
    ///					In <see cref="DiscardPolicy"/>, a task that cannot be 
    ///                 executed is simply dropped.
    ///				</item>
    ///				<item>
    ///					In <see cref="DiscardOldestPolicy"/>, if the executor is not
    ///					shut down, the task at the head of the work queue is dropped, and
    /// 				then execution is retried (which can fail again, causing this to be
    /// 				repeated.) 
    /// 			</item>
    ///			</list>
    ///			It is possible to define and use other kinds of
    /// 		<see cref="IRejectedExecutionHandler"/> classes. Doing so 
    ///         requires some care especially when policies are designed to 
    ///         work only under particular capacity or queuing policies. 
    ///		</description>
    /// </item>
    /// <item>
    ///		<term>Hook methods</term>
    /// 	<description>
    /// 		This class provides <i>protected</i> overridable <see cref="BeforeExecute"/>
    /// 		and <see cref="AfterExecute"/> methods that are called before and
    ///			after execution of each task.  These can be used to manipulate the
    /// 		execution environment; for example, reinitializing ThreadLocals,
    /// 		gathering statistics, or adding log entries. Additionally, method
    /// 		<see cref="Terminated"/> can be overridden to perform
    /// 		any special processing that needs to be done once the Executor has
    /// 		fully terminated.
    ///			<p/>
    ///			If hook or callback methods throw exceptions, internal worker 
    ///         threads may in turn fail and abruptly terminate.
    ///		</description>
    /// </item>
    /// <item>
    ///		<term>Queue maintenance</term>
    ///		<description> 
    ///			Method <see cref="Queue"/> allows access to the work queue for 
    ///         purposes of monitoring and debugging.  Use of this method for 
    ///         any other purpose is <i>strongly</i> discouraged. 
    /// 	</description>
    /// </item>
    /// <item>
    ///     <term>Finalization</term>
    ///     <description> A pool that is no longer referenced in a program <i>AND</i>
    ///         has no remaining threads will be <see cref="Shutdown"/> automatically. If
    ///         you would like to ensure that unreferenced pools are reclaimed even
    ///         if users forget to call <see cref="Shutdown"/>, then you must arrange
    ///         that unused threads eventually die, by setting appropriate
    ///         keep-alive times, using a lower bound of zero core threads and/or
    ///         setting <see cref="AllowsCoreThreadsToTimeOut"/>.
    ///     </description>
    /// </item>
    /// </list>
    /// <example>
    /// <b>Extension example</b>. Most extensions of this class
    /// override one or more of the protected hook methods. For example,
    /// here is a subclass that adds a simple pause/resume feature:
    /// 
    /// <code language="C#">
    ///		public class PausableThreadPoolExecutor : ThreadPoolExecutor {
    ///			private boolean _isPaused;
    /// 		private ReentrantLock _pauseLock = new ReentrantLock();
    /// 		private ICondition _unpaused = pauseLock.NewCondition();
    /// 
    /// 		public PausableThreadPoolExecutor(...) : base( ... ) { }
    /// 
    /// 		protected override void BeforeExecute(Thread t, IRunnable r) {
    /// 				base.BeforeExecute(t, r);
    /// 				_pauseLock.Lock();
    /// 				try {
    /// 					while (_isPaused) _unpaused.Await();
    /// 				} catch (ThreadInterruptedException ie) {
    ///						t.Interrupt();
    /// 				} finally {
    ///						_pauseLock.Unlock();
    ///					}
    ///			}
    /// 
    ///			public void Pause() {
    /// 			using(_pauseLock.Lock())
    /// 			{
    /// 				_isPaused = true;
    ///				}
    ///			}
    /// 
    ///			public void Resume() {
    ///				using(_pauseLock.Lock())
    ///				{
    ///					_isPaused = false;
    /// 				_unpaused.SignalAll();
    /// 			}
    /// 		}
    /// 	}
    /// </code>
    /// </example>
    /// </remarks>
    /// <author>Doug Lea</author>
    /// <author>Griffin Caprio (.NET)</author>
    /// <author>Kenneth Xu</author>
    public class ThreadPoolExecutor : AbstractExecutorService, IDisposable, IRecommendParallelism //BACKPORT_3_1
    {
        #region Worker Class

        /// <summary>
        /// Class Worker mainly maintains interrupt control state for
        /// threads running tasks, along with other minor bookkeeping. This
        /// class opportunistically extends ReentrantLock to simplify
        /// acquiring and releasing a lock surrounding each task execution.
        /// This protects against interrupts that are intended to wake up a
        /// worker thread waiting for a task from instead interrupting a
        /// task being run.
        /// </summary>
        protected internal class Worker : ReentrantLock, IRunnable
        {
            private readonly ThreadPoolExecutor _parentThreadPoolExecutor;

            /// <summary> 
            /// Per thread completed task counter; accumulated
            /// into completedTaskCount upon termination.
            /// </summary>
            protected internal volatile uint CompletedTasks;

            /// <summary> 
            /// Initial task to run before entering run loop
            /// </summary>
            protected internal IRunnable FirstTask;

            /// <summary> 
            /// Thread this worker is running in.  Acts as a final field,
            /// but cannot be set until thread is created.
            /// </summary>
            protected internal Thread Thread;

            /// <summary>
            /// Default Constructor
            /// </summary>
            /// <param name="firstTask">Task to run before entering run loop.</param>
            /// <param name="parentThreadPoolExecutor"><see cref="ThreadPoolExecutor"/> that controls this worker</param>
            internal Worker(ThreadPoolExecutor parentThreadPoolExecutor, IRunnable firstTask)
            {
                FirstTask = firstTask;
                _parentThreadPoolExecutor = parentThreadPoolExecutor;
                Thread = parentThreadPoolExecutor.ThreadFactory.NewThread(this);
            }

            #region IRunnable Members

            /// <summary>
            /// Runs the associated task, signalling the <see cref="ThreadPoolExecutor"/> when exiting.
            /// </summary>
            public void Run()
            {
                _parentThreadPoolExecutor.RunWorker(this);
            }

            #endregion
        }

        #endregion

        #region Private Fields

        private static readonly Action<IRunnable> _noAction = (r => { });

        private static readonly Predicate<IRunnable> _checkCancelled = r =>
        {
            var c = r as ICancellable;
            return c != null && c.IsCancelled;
        };

        /// <summary>
        /// The main pool control state, controlState, is an <see cref="AtomicInteger"/> packing
        /// two conceptual fields
        ///   workerCount, indicating the effective number of threads
        ///   runState,    indicating whether running, shutting down etc
        ///
        /// In order to pack them into one int, we limit workerCount to
        /// (2^29)-1 (about 500 million) threads rather than (2^31)-1 (2
        /// billion) otherwise representable. If this is ever an issue in
        /// the future, the variable can be changed to be an <see cref="AtomicLong"/>,
        /// and the shift/mask constants below adjusted. But until the need
        /// arises, this code is a bit faster and simpler using an <see cref="Int32"/>.
        ///
        /// The workerCount is the number of workers that have been
        /// permitted to start and not permitted to stop.  The value may be
        /// transiently different from the actual number of live threads,
        /// for example when a <see cref="IThreadFactory"/> fails to create a thread when
        /// asked, and when exiting threads are still performing
        /// bookkeeping before terminating. The user-visible pool size is
        /// reported as the current size of the workers set.
        ///
        /// The runState provides the main lifecyle control, taking on values:
        ///
        ///   RUNNING:  Accept new tasks and process queued tasks
        ///   SHUTDOWN: Don't accept new tasks, but process queued tasks
        ///   STOP:     Don't accept new tasks, don't process queued tasks,
        ///             and interrupt in-progress tasks
        ///   TIDYING:  All tasks have terminated, workerCount is zero,
        ///             the thread transitioning to state TIDYING
        ///             will run the <see cref="Terminated"/> hook method
        ///   TERMINATED: <see cref="Terminated"/> has completed
        ///
        /// The numerical order among these values matters, to allow
        /// ordered comparisons. The runState monotonically increases over
        /// time, but need not hit each state. The transitions are:
        ///
        /// RUNNING -> SHUTDOWN
        ///    On invocation of <see cref="Shutdown"/>, perhaps implicitly in ~ThreadPoolExecutor
        /// (RUNNING or SHUTDOWN) -> STOP
        ///    On invocation of <see cref="ShutdownNow"/>
        /// SHUTDOWN -> TIDYING
        ///    When both queue and pool are empty
        /// STOP -> TIDYING
        ///    When pool is empty
        /// TIDYING -> TERMINATED
        ///    When the <see cref="Terminated"/> hook method has completed
        ///
        /// Threads waiting in <see cref="AwaitTermination"/> will return when the
        /// state reaches TERMINATED.
        ///
        /// Detecting the transition from SHUTDOWN to TIDYING is less
        /// straightforward than you'd like because the queue may become
        /// empty after non-empty and vice versa during SHUTDOWN state, but
        /// we can only terminate if, after seeing that it is empty, we see
        /// that workerCount is 0 (which sometimes entails a recheck -- see
        /// below).
        /// </summary>
        private readonly AtomicInteger _controlState = new AtomicInteger(ControlOf(RUNNING, 0));
        private const int COUNT_BITS = 29;
        private const int CAPACITY = (1 << COUNT_BITS) - 1;

        /// <summary>
        /// runState is stored in the high-order bits 
        /// </summary>
        private const int RUNNING = -1 << COUNT_BITS;
        private const int SHUTDOWN = 0 << COUNT_BITS;
        private const int STOP = 1 << COUNT_BITS;
        private const int TIDYING = 2 << COUNT_BITS;
        private const int TERMINATED = 3 << COUNT_BITS;

        /// <summary> 
        /// Set containing all worker threads in pool. Accessed only when holding mainLock.
        /// </summary>
        private readonly IDictionary<Worker, Worker> _currentWorkerThreads = new Dictionary<Worker, Worker>();

        /// <summary> 
        /// Lock held on access to workers set and related bookkeeping.
        /// While we could use a concurrent set of some sort, it turns out
        /// to be generally preferable to use a lock. Among the reasons is
        /// that this serializes InterruptIdleWorkers, which avoids
        /// unnecessary interrupt storms, especially during shutdown.
        /// Otherwise exiting threads would concurrently interrupt those
        /// that have not yet interrupted. It also simplifies some of the
        /// associated statistics bookkeeping of largestPoolSize etc. We
        /// also hold mainLock on shutdown and shutdownNow, for the sake of
        /// ensuring workers set is stable while separately checking
        /// permission to interrupt and actually interrupting.
        /// </summary>
        private readonly ReentrantLock _mainLock = new ReentrantLock();

        /// <summary> 
        /// The queue used for holding tasks and handing off to worker
        /// threads.  We do not require that workQueue.poll() returning
        /// null necessarily means that workQueue.isEmpty(), so rely
        /// solely on isEmpty to see if the queue is empty (which we must
        /// do for example when deciding whether to transition from
        /// SHUTDOWN to TIDYING).  This accommodates special-purpose
        /// queues such as DelayQueues for which poll() is allowed to
        /// return null even if it may later return non-null when delays
        /// expire.
        /// </summary>
        private readonly IBlockingQueue<IRunnable> _workQueue;

        /// <summary>
        /// Wait condition to support AwaitTermination
        /// </summary>
        private readonly ICondition _termination;

        /// <summary> 
        /// Counter for completed tasks. Updated only on termination of
        /// worker threads.  Accessed only under mainlock
        /// </summary>
        private long _completedTaskCount;

        /// <summary> 
        /// Tracks largest attained pool size. Accessed only under mainLock.
        /// </summary>
        private int _largestPoolSize;

        #region User Control Params

        /*
         * All user control parameters are declared as volatiles so that
         * ongoing actions are based on freshest values, but without need
         * for locking, since no internal invariants depend on them
         * changing synchronously with respect to other actions.
         */

        /// <summary> 
        /// The default <see cref="IRejectedExecutionHandler"/>
        /// </summary>
        private static readonly IRejectedExecutionHandler _defaultRejectedExecutionHandler = new AbortPolicy();

        /// All user control parameters are declared as volatiles so that
        /// ongoing actions are based on freshest values, but without need
        /// for locking, since no internal invariants depend on them
        /// changing synchronously with respect to other actions.
        /// <summary> 
        /// Core pool size is the minimum number of workers to keep alive
        /// (and not allow to time out etc) unless _allowCoreThreadTimeOut
        /// is set, in which case the minimum is zero.
        /// </summary>
        private volatile int _corePoolSize;

        /// <summary> 
        /// Timeout for idle threads waiting for work. Threads use this 
        /// timeout when there are more than _corePoolSize present or 
        /// if _allowCoreThreadTimeOut. Otherwise they wait forever for 
        /// new work. We must use long instead of TimeSpan so that we
        /// can update it atomically. Add access must through property
        /// <see cref="KeepAliveTime"/>.
        /// </summary>
        private long _keepAliveTicks;

        /// <summary> 
        /// If <c>false</c> (the default), core threads stay alive even when idle.
        /// If <c>true</c>, core threads use <see cref="KeepAliveTime"/> 
        /// to time out waiting for work.
        /// </summary>
        private volatile bool _allowCoreThreadToTimeOut;

        /// <summary> 
        /// Maximum pool size. Note that the actual maximum is internally
        /// bounded by CAPACITY.
        /// </summary>
        private volatile int _maximumPoolSize;

        /// <summary> 
        /// <see cref="IRejectedExecutionHandler"/> called when
        /// <see cref="ThreadPoolExecutor"/> is saturated or  
        /// <see cref="Shutdown()"/> in executed.
        /// </summary>
        private volatile IRejectedExecutionHandler _rejectedExecutionHandler;

        /// <summary> 
        /// Factory for new threads. All threads are created using this
        /// factory (via method AddWorker).  All callers must be prepared
        /// for AddWorker to fail, which may reflect a system or user's
        /// policy limiting the number of threads.  Even though it is not
        /// treated as an error, failure to create threads may result in
        /// new tasks being rejected or existing ones remaining stuck in
        /// the queue. On the other hand, no special precautions exist to
        /// handle OutOfMemoryErrors that might be thrown while trying to
        /// create threads, since there is generally no recourse from
        /// within this class.
        /// </summary>
        private volatile IThreadFactory _threadFactory;

        #endregion

        #region Control State Packing & Unpacking Functions
        private static int RunStateOf(int c)
        {
            return c & ~CAPACITY;
        }

        private static int WorkerCountOf(int c)
        {
            return c & CAPACITY;
        }

        private static int ControlOf(int rs, int wc)
        {
            return rs | wc;
        }
        #endregion

        #region Control State Query Methods
        /// <summary>
        /// Bit field accessors that don't require unpacking _controlState.
        /// These depend on the bit layout and on workerCount being never negative.
        /// </summary>
        private static bool RunStateLessThan(int c, int s)
        {
            return c < s;
        }

        private static bool RunStateAtLeast(int c, int s)
        {
            return c >= s;
        }

        private static bool IsRunning(int c)
        {
            return c < SHUTDOWN;
        }
        #endregion

        #endregion

        #region Public Properties

        /// <summary> 
        /// Gets and sets the time limit for which threads may remain idle before
        /// being terminated.  
        /// </summary>
        /// <remarks>
        /// If there are more than the core number of
        /// threads currently in the pool, after waiting this amount of
        /// time without processing a task, excess threads will be
        /// terminated.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// if <i>value</i> is less than 0 or if <i>value</i> equals 0 and 
        /// <see cref="AllowsCoreThreadsToTimeOut"/> is 
        /// <c>true</c>
        /// </exception>
        public TimeSpan KeepAliveTime
        {
            set
            {
                if (value.Ticks < 0)
                {
                    throw new ArgumentOutOfRangeException("value", value, "Keep alive time must be greater than 0.");
                }
                if (value.Ticks == 0 && AllowsCoreThreadsToTimeOut)
                {
                    throw new ArgumentException("Core threads must have nonzero keep alive times");
                }
                var longValue = value.Ticks;
                var keepAliveTime = Interlocked.Exchange(ref _keepAliveTicks, longValue);
                var delta = longValue - keepAliveTime;
                if (delta < 0)
                    InterruptIdleWorkers();
            }
            get
            {
                return new TimeSpan(Interlocked.Read(ref _keepAliveTicks));
            }
        }

        /// <summary>
        /// Returns <c>true</c> if this pool allows core threads to time out and
        /// terminate if no tasks arrive within the keepAlive time, being
        /// replaced if needed when new tasks arrive. 
        /// </summary>
        /// <remarks>
        /// When true, the same keep-alive policy applying to non-core threads applies also to
        /// core threads. When false (the default), core threads are never
        /// terminated due to lack of incoming tasks.
        /// </remarks>
        /// <returns> 
        /// Sets the policy governing whether core threads may time out and
        /// terminate if no tasks arrive within the keep-alive time, being
        /// replaced if needed when new tasks arrive. When false, core
        /// threads are never terminated due to lack of incoming
        /// tasks. When true, the same keep-alive policy applying to
        /// non-core threads applies also to core threads. To avoid
        /// continual thread replacement, the keep-alive time must be
        /// greater than zero when setting <c>true</c>. This method
        /// should in general be called before the pool is actively used.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// If <c>true</c> and keep alive time is less than or equal to 0
        /// </exception>
        public bool AllowsCoreThreadsToTimeOut
        {
            get { return _allowCoreThreadToTimeOut; }
            set
            {
                if (value && KeepAliveTime.Ticks <= 0)
                {
                    throw new ArgumentException("Core threads must have nonzero keep alive times");
                }

                if (value == _allowCoreThreadToTimeOut) return;
                _allowCoreThreadToTimeOut = value;
                if (value)
                    InterruptIdleWorkers();
            }
        }

        /// <summary> 
        /// Returns <c>true</c> if this executor is in the process of terminating
        /// after <see cref="Shutdown()"/> or
        /// <see cref="ShutdownNow()"/> but has not
        /// completely terminated.  
        /// </summary>
        /// <remarks>
        /// This method may be useful for debugging. A return of <c>true</c> reported a sufficient
        /// period after shutdown may indicate that submitted tasks have
        /// ignored or suppressed interruption, causing this executor not
        /// to properly terminate.
        /// </remarks>
        /// <returns><c>true</c>if terminating but not yet terminated.</returns>
        public bool IsTerminating
        {
            get
            {
                var c = _controlState.Value;
                return !IsRunning(c) && RunStateLessThan(c, TERMINATED);
            }
        }

        /// <summary>
        /// Gets / Sets the thread factory used to create new threads.
        /// </summary>
        /// <returns>the current thread factory</returns>
        /// <exception cref="System.ArgumentNullException">if the threadfactory is null</exception>
        public IThreadFactory ThreadFactory
        {
            get { return _threadFactory; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _threadFactory = value;
            }
        }

        /// <summary> 
        /// Gets / Sets the current handler for unexecutable tasks.
        /// </summary>
        /// <returns>the current handler</returns>
        /// <exception cref="System.ArgumentNullException">if the execution handler is null.</exception>
        public IRejectedExecutionHandler RejectedExecutionHandler
        {
            get { return _rejectedExecutionHandler; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _rejectedExecutionHandler = value;
            }
        }

        /// <summary> 
        /// Returns the task queue used by this executor. Access to the
        /// task queue is intended primarily for debugging and monitoring.
        /// This queue may be in active use.  Retrieving the task queue
        /// does not prevent queued tasks from executing.
        /// </summary>
        /// <returns>the task queue</returns>
        public IBlockingQueue<IRunnable> Queue
        {
            get { return _workQueue; }
        }


        /// <summary> 
        /// Sets the core number of threads.  This overrides any value set
        /// in the constructor.  If the new value is smaller than the
        /// current value, excess existing threads will be terminated when
        /// they next become idle.  If larger, new threads will, if needed,
        /// be started to execute any queued tasks.
        /// </summary>
        public int CorePoolSize
        {
            get { return _corePoolSize; }

            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", value, "CorePoolSize cannot be less than 0");
                var delta = value - _corePoolSize;
                _corePoolSize = value;
                if (WorkerCountOf(_controlState.Value) > value)
                    InterruptIdleWorkers();
                else if (delta > 0)
                {
                    // We don't really know how many new threads are "needed".
                    // As a heuristic, prestart enough new workers (up to new
                    // core size) to handle the current number of tasks in
                    // queue, but stop if queue becomes empty while doing so.
                    var k = Math.Min(delta, _workQueue.Count);
                    while (k-- > 0 && AddWorker(null, true))
                    {
                        if (_workQueue.Count < 1)
                            break;
                    }
                }
            }
        }

        ///<summary>
        /// Returns the current number of threads in the pool.
        /// </summary>
        public int PoolSize
        {
            get
            {
                using(_mainLock.Lock())
                {
                    // Remove rare and surprising possibility of
                    // isTerminated() && getPoolSize() > 0
                    return RunStateAtLeast(_controlState.Value, TIDYING) ? 
                        0 : _currentWorkerThreads.Count;
                }
            }
        }

        /// <summary>
        /// Returns the approximate number of threads that are actively
        /// executing tasks.
        /// </summary>
        public int ActiveCount
        {
            get
            {
                using(_mainLock.Lock())
                {
                    var n = 0;
                    foreach (var worker in _currentWorkerThreads.Keys)
                    {
                        if (worker.IsLocked) ++n;
                    }
                    return n;
                }
            }
        }

        /// <summary> 
        /// Gets / Sets the maximum allowed number of threads. 
        /// </summary>
        /// <remarks>
        /// This overrides any
        /// value set in the constructor. If the new value is smaller than
        /// the current value, excess existing threads will be
        /// terminated when they next become idle.
        /// </remarks>
        /// <exception cref="System.ArgumentOutOfRangeException">If value is less than zero or less than 
        /// <see cref="CorePoolSize"/>. 
        /// </exception>
        public int MaximumPoolSize
        {
            get { return _maximumPoolSize; }

            set
            {
                if (value <= 0 || value < _corePoolSize)
                {
                    throw new ArgumentOutOfRangeException("value", value, String.Format(
                        "Maximum pool size cannont be less than 1 and cannot be less than Core Pool Size {0}",
                        _corePoolSize));
                }
                _maximumPoolSize = value;
                if (WorkerCountOf(_controlState.Value) > value)
                    InterruptIdleWorkers();
            }
        }

        /// <summary> 
        /// Returns the largest number of threads that have ever
        /// simultaneously been in the pool.
        /// </summary>
        /// <returns> the number of threads</returns>
        public int LargestPoolSize
        {
            get
            {
                using(_mainLock.Lock())
                {
                    return _largestPoolSize;
                }
            }
        }

        /// <summary> 
        /// Returns the approximate total number of tasks that have been
        /// scheduled for execution. 
        /// </summary>
        /// <remarks>
        /// Because the states of tasks and
        /// threads may change dynamically during computation, the returned
        /// value is only an approximation
        /// </remarks>
        /// <returns>the number of tasks</returns>
        public long TaskCount
        {
            get
            {
                using(_mainLock.Lock())
                {
                    var n = _completedTaskCount;
                    foreach (var w in _currentWorkerThreads.Keys)
                    {
                        n += w.CompletedTasks;
                        if (w.IsLocked)
                            ++n;
                    }
                    return n + _workQueue.Count;
                }
            }
        }

        /// <summary> 
        /// Returns the approximate total number of tasks that have
        /// completed execution. 
        /// </summary>
        /// <remarks>
        /// Because the states of tasks and threads
        /// may change dynamically during computation, the returned value
        /// is only an approximation, but one that does not ever decrease
        /// across successive calls.
        /// </remarks>
        /// <returns>the number of tasks</returns>
        public long CompletedTaskCount
        {
            get
            {
                using(_mainLock.Lock())
                {
                    var n = _completedTaskCount;
                    foreach (var worker in _currentWorkerThreads.Keys)
                    {
                        n += worker.CompletedTasks;
                    }
                    return n;
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary> 
        /// Creates a new <see cref="ThreadPoolExecutor"/> with the given initial
        /// parameters and default thread factory and rejected execution handler.
        /// </summary>
        /// <remarks>>
        /// It may be more convenient to use one of the <see cref="Executors"/> factory
        /// methods instead of this general purpose constructor.
        /// </remarks>
        /// <param name="corePoolSize">the number of threads to keep in the pool, even if they are idle.</param>
        /// <param name="maximumPoolSize">the maximum number of threads to allow in the pool.</param>
        /// <param name="keepAliveTime">
        /// When the number of threads is greater than
        /// <see cref="CorePoolSize"/>, this is the maximum time that excess idle threads
        /// will wait for new tasks before terminating.
        /// </param>
        /// <param name="workQueue">
        /// The queue to use for holding tasks before they
        /// are executed. This queue will hold only the <see cref="IRunnable"/>
        /// tasks submitted by the <see cref="AbstractExecutorService.Execute(IRunnable)"/> method.
        /// </param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If <paramref name="corePoolSize"/> or <paramref name="keepAliveTime"/> is less than zero, or if <paramref name="maximumPoolSize"/>
        /// is less than or equal to zero, or if <paramref name="corePoolSize"/> is greater than <paramref name="maximumPoolSize"/>
        /// </exception>
        /// <exception cref="System.ArgumentNullException">if <paramref name="workQueue"/> is null</exception>
        /// <throws>  NullPointerException if <tt>workQueue</tt> is null </throws>
        public ThreadPoolExecutor(int corePoolSize, int maximumPoolSize, TimeSpan keepAliveTime, IBlockingQueue<IRunnable> workQueue)
            : this(
                corePoolSize, maximumPoolSize, keepAliveTime, workQueue, Executors.NewDefaultThreadFactory(),
                _defaultRejectedExecutionHandler)
        {
        }

        /// <summary> 
        /// Creates a new <see cref="ThreadPoolExecutor"/> with the given initial
        /// parameters and default <see cref="RejectedExecutionException"/>.
        /// </summary>
        /// <param name="corePoolSize">the number of threads to keep in the pool, even if they are idle.</param>
        /// <param name="maximumPoolSize">the maximum number of threads to allow in the pool.</param>
        /// <param name="keepAliveTime">
        /// When the number of threads is greater than
        /// <see cref="CorePoolSize"/>, this is the maximum time that excess idle threads
        /// will wait for new tasks before terminating.
        /// </param>
        /// <param name="workQueue">
        /// The queue to use for holding tasks before they
        /// are executed. This queue will hold only the <see cref="IRunnable"/>
        /// tasks submitted by the <see cref="AbstractExecutorService.Execute(IRunnable)"/> method.
        /// </param>
        /// <param name="threadFactory">
        /// <see cref="IThreadFactory"/> to use for new thread creation.
        /// </param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If <paramref name="corePoolSize"/> or <paramref name="keepAliveTime"/> is less than zero, or if <paramref name="maximumPoolSize"/>
        /// is less than or equal to zero, or if <paramref name="corePoolSize"/> is greater than <paramref name="maximumPoolSize"/>
        /// </exception>
        /// <exception cref="System.ArgumentNullException">if <paramref name="workQueue"/> or <paramref name="threadFactory"/> is null</exception>
        public ThreadPoolExecutor(int corePoolSize, int maximumPoolSize, TimeSpan keepAliveTime, IBlockingQueue<IRunnable> workQueue,
                                  IThreadFactory threadFactory)
            : this(corePoolSize, maximumPoolSize, keepAliveTime, workQueue, threadFactory, _defaultRejectedExecutionHandler)
        {
        }

        /// <summary> 
        /// Creates a new <see cref="ThreadPoolExecutor"/> with the given initial
        /// parameters and <see cref="IThreadFactory"/>.
        /// </summary>
        /// <summary> 
        /// Creates a new <see cref="ThreadPoolExecutor"/> with the given initial
        /// parameters and default <see cref="RejectedExecutionException"/>.
        /// </summary>
        /// <param name="corePoolSize">the number of threads to keep in the pool, even if they are idle.</param>
        /// <param name="maximumPoolSize">the maximum number of threads to allow in the pool.</param>
        /// <param name="keepAliveTime">
        /// When the number of threads is greater than
        /// <see cref="CorePoolSize"/>, this is the maximum time that excess idle threads
        /// will wait for new tasks before terminating.
        /// </param>
        /// <param name="workQueue">
        /// The queue to use for holding tasks before they
        /// are executed. This queue will hold only the <see cref="IRunnable"/>
        /// tasks submitted by the <see cref="AbstractExecutorService.Execute(IRunnable)"/> method.
        /// </param>
        /// <param name="handler">
        /// The <see cref="IRejectedExecutionHandler"/> to use when execution is blocked
        /// because the thread bounds and queue capacities are reached.
        /// </param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If <paramref name="corePoolSize"/> or <paramref name="keepAliveTime"/> is less than zero, or if <paramref name="maximumPoolSize"/>
        /// is less than or equal to zero, or if <paramref name="corePoolSize"/> is greater than <paramref name="maximumPoolSize"/>
        /// </exception>
        /// <exception cref="System.ArgumentNullException">if <paramref name="workQueue"/> or <paramref name="handler"/> is null</exception>
        public ThreadPoolExecutor(int corePoolSize, int maximumPoolSize, TimeSpan keepAliveTime, IBlockingQueue<IRunnable> workQueue,
                                  IRejectedExecutionHandler handler)
            : this(corePoolSize, maximumPoolSize, keepAliveTime, workQueue, Executors.NewDefaultThreadFactory(), handler)
        {
        }

        /// <summary> Creates a new <see cref="ThreadPoolExecutor"/> with the given initial
        /// parameters.
        /// 
        /// </summary>
        /// <param name="corePoolSize">the number of threads to keep in the pool, even if they are idle.</param>
        /// <param name="maximumPoolSize">the maximum number of threads to allow in the pool.</param>
        /// <param name="keepAliveTime">
        /// When the number of threads is greater than
        /// <see cref="CorePoolSize"/>, this is the maximum time that excess idle threads
        /// will wait for new tasks before terminating.
        /// </param>
        /// <param name="workQueue">
        /// The queue to use for holding tasks before they
        /// are executed. This queue will hold only the <see cref="IRunnable"/>
        /// tasks submitted by the <see cref="AbstractExecutorService.Execute(IRunnable)"/> method.
        /// </param>
        /// <param name="threadFactory">
        /// <see cref="IThreadFactory"/> to use for new thread creation.
        /// </param>
        /// <param name="handler">
        /// The <see cref="IRejectedExecutionHandler"/> to use when execution is blocked
        /// because the thread bounds and queue capacities are reached.
        /// </param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If <paramref name="corePoolSize"/> or <paramref name="keepAliveTime"/> is less than zero, or if <paramref name="maximumPoolSize"/>
        /// is less than or equal to zero, or if <paramref name="corePoolSize"/> is greater than <paramref name="maximumPoolSize"/>
        /// </exception>
        /// <exception cref="System.ArgumentNullException">if <paramref name="workQueue"/>, <paramref name="handler"/>, or <paramref name="threadFactory"/> is null</exception>
        public ThreadPoolExecutor(int corePoolSize, int maximumPoolSize, TimeSpan keepAliveTime, IBlockingQueue<IRunnable> workQueue,
                                  IThreadFactory threadFactory, IRejectedExecutionHandler handler)
        {
            if (corePoolSize < 0)
            {
                throw new ArgumentOutOfRangeException("corePoolSize", corePoolSize, 
                    "core pool size cannot be less than zero.");
            }
            if (maximumPoolSize <= 0)
            {
                throw new ArgumentOutOfRangeException("maximumPoolSize", maximumPoolSize, 
                    "maximum pool size must be greater than zero");
            }
            if (maximumPoolSize < corePoolSize)
            {
                throw new ArgumentException("maximum pool size, " + maximumPoolSize + 
                    " cannot be less than core pool size, " + corePoolSize + ".", "maximumPoolSize");
            }
            if (keepAliveTime.Ticks < 0)
            {
                throw new ArgumentOutOfRangeException("keepAliveTime", keepAliveTime, 
                    "keep alive time must be greater than or equal to zero.");
            }
            if (workQueue == null)
            {
                throw new ArgumentNullException("workQueue");
            }
            if (threadFactory == null)
            {
                throw new ArgumentNullException("threadFactory");
            }
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }
            _corePoolSize = corePoolSize;
            _maximumPoolSize = maximumPoolSize;
            _workQueue = workQueue;
            KeepAliveTime = keepAliveTime;
            _threadFactory = threadFactory;
            _rejectedExecutionHandler = handler;
            _termination = _mainLock.NewCondition();
        }

        #endregion

        #region AbstractExecutorService Implementations

        /// <summary> 
        /// Returns <c>true</c> if this executor has been shut down.
        /// </summary>
        /// <returns> 
        /// Returns <c>true</c> if this executor has been shut down.
        /// </returns>
        public override bool IsShutdown
        {
            get { return !IsRunning(_controlState.Value); }
        }

        /// <summary> 
        /// Returns <c>true</c> if all tasks have completed following shut down.
        /// </summary>
        /// <remarks>
        /// Note that this will never return <c>true</c> unless
        /// either <see cref="IExecutorService.Shutdown()"/> or 
        /// <see cref="IExecutorService.ShutdownNow()"/> was called first.
        /// </remarks>
        /// <returns> <c>true</c> if all tasks have completed following shut down</returns>
        public override bool IsTerminated
        {
            get { return RunStateAtLeast(_controlState.Value, TERMINATED); }
        }

        /// <summary> 
        /// Executes the given task sometime in the future.  The task may 
        /// execute in a new thread or in an existing pooled thread.
        /// </summary>
        /// <remarks>
        /// If the task cannot be submitted for execution, either because this
        /// executor has been shutdown or because its capacity has been reached,
        /// the task is handled by the current <see cref="IRejectedExecutionHandler"/>
        /// for this <see cref="ThreadPoolExecutor"/>.
        /// </remarks>
        /// <param name="command">the task to execute</param>
        /// <exception cref="RejectedExecutionException">
        /// if the task cannot be accepted. 
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// if <paramref name="command"/> is <c>null</c>
        /// </exception>
        protected override void DoExecute(IRunnable command)
        {

            /*
             * Proceed in 3 steps:
             *
             * 1. If fewer than corePoolSize threads are running, try to
             * start a new thread with the given command as its first
             * task.  The call to AddWorker atomically checks runState and
             * workerCount, and so prevents false alarms that would add
             * threads when it shouldn't, by returning false.
             *
             * 2. If a task can be successfully queued, then we still need
             * to double-check whether we should have added a thread
             * (because existing ones died since last checking) or that
             * the pool shut down since entry into this method. So we
             * recheck state and if necessary roll back the enqueuing if
             * stopped, or start a new thread if there are none.
             *
             * 3. If we cannot queue task, then we try to add a new
             * thread.  If it fails, we know we are shut down or saturated
             * and so reject the task.
             */
            var c = _controlState.Value;
            if (WorkerCountOf(c) < _corePoolSize)
            {
                if (AddWorker(command, true)) 
                    return;
                c = _controlState.Value;
            }
            if (IsRunning(c) && _workQueue.Offer(command))
            {
                int recheck = _controlState.Value;
                if (!IsRunning(recheck) && Remove(command))
                    Reject(command);
                else if (WorkerCountOf(recheck) == 0)
                    AddWorker(null, false);
            }
            else if (!AddWorker(command, false))
                Reject(command);
        }

        /// <summary>
        /// Removes this task from the executor's internal queue if it is
        /// present, thus causing it not to be run if it has not already
        /// started.
        /// </summary>
        /// <remarks>
        /// This method may be useful as one part of a cancellation scheme.  
        /// It may fail to remove tasks that have been converted into other 
        /// forms before being placed on the internal queue. For example, a 
        /// task entered using <see cref="IExecutorService.Submit(Action)"/> 
        /// might be converted into a form that maintains <see cref="IFuture{T}"/> 
        /// status. However, in such cases, method <see cref="Purge"/> may be 
        /// used to remove those Futures that have been cancelled.
        /// </remarks>
        /// <param name="task">The task to remove.</param>
        /// <returns><c>true</c> if the task was removed.</returns>
        public bool Remove(IRunnable task)
        {
            var removed = _workQueue.Remove(task);
            TryTerminate(); // In case SHUTDOWN and now empty
            return removed;
        }

        /// <summary>
        /// Removes this task from the executor's internal queue if it is
        /// present, thus causing it not to be run if it has not already
        /// started.
        /// </summary>
        /// <remarks>
        /// This method may be useful as one part of a cancellation scheme.  
        /// It may fail to remove tasks that have been converted into other 
        /// forms before being placed on the internal queue. For example, a 
        /// task entered using <see cref="IExecutorService.Submit(Action)"/> 
        /// might be converted into a form that maintains <see cref="IFuture{T}"/> 
        /// status. However, in such cases, method <see cref="Purge"/> may be 
        /// used to remove those Futures that have been cancelled.
        /// </remarks>
        /// <param name="task">The task to remove.</param>
        /// <returns><c>true</c> if the task was removed.</returns>
        public bool Remove(Action task)
        {
            return Remove((IRunnable)new Runnable(task));
        }

        /// <summary> 
        /// Initiates an orderly shutdown in which previously submitted
        /// tasks are executed, but no new tasks will be
        /// accepted. Invocation has no additional effect if already shut
        /// down.
        /// </summary>
        public override void Shutdown()
        {
            using(_mainLock.Lock())
            {
                //TODO_SECURITY_CHECK
                AdvanceRunState(SHUTDOWN);
                InterruptIdleWorkers();
                OnShutdown(); // hook for ScheduledThreadPoolExecutor
            }
            TryTerminate();
        }

        /// <summary> 
        /// Attempts to stop all actively executing tasks, halts the
        /// processing of waiting tasks, and returns a list of the tasks
        /// that were awaiting execution. These tasks are drained (removed)
        /// from the task queue upon return from this method.
        ///
        /// <p/>There are no guarantees beyond best-effort attempts to stop
        /// processing actively executing tasks.  This implementation
        /// cancels tasks via <see cref="Thread.Interrupt"/>, so any task that
        /// fails to respond to interrupts may never terminate.
        ///
        /// </summary> 
        public override IList<IRunnable> ShutdownNow()
        {
            IList<IRunnable> tasks;
            using(_mainLock.Lock())
            {
                AdvanceRunState(STOP);
                InterruptWorkers();
                tasks = DrainQueue();
            }
            TryTerminate();
            return tasks;
        }

        /// <summary> 
        /// Blocks until all tasks have completed execution after a shutdown
        /// request, or the timeout occurs, or the current thread is
        /// interrupted, whichever happens first. 
        /// </summary>
        /// <param name="duration">the time span to wait.
        /// </param>
        /// <returns> <c>true</c> if this executor terminated and <c>false</c>
        /// if the timeout elapsed before termination
        /// </returns>
        public override bool AwaitTermination(TimeSpan duration)
        {
            var durationToWait = duration;
            var deadline = DateTime.Now.Add(durationToWait);
            using(_mainLock.Lock())
            {
                if (RunStateAtLeast(_controlState.Value, TERMINATED))
                {
                    return true;
                }
                while (duration == Timeout.InfiniteTimeSpan || durationToWait.Ticks > 0)
                {
                    _termination.Await(durationToWait);
                    if (RunStateAtLeast(_controlState.Value, TERMINATED))
                    {
                        return true;
                    }
                    durationToWait = deadline.Subtract(DateTime.Now);
                }
                return false;
            }
        }

        #endregion

        #region Public Methods

        /// <summary> 
        /// Tries to remove from the work queue all <see cref="IFuture{T}"/>
        /// tasks that have been cancelled. This method can be useful as a
        /// storage reclamation operation, that has no other impact on
        /// functionality. Cancelled tasks are never executed, but may
        /// accumulate in work queues until worker threads can actively
        /// remove them. Invoking this method instead tries to remove them now.
        /// However, this method may fail to remove tasks in
        /// the presence of interference by other threads.
        /// </summary>
        public void Purge()
        {
            _workQueue.Drain(_noAction, _checkCancelled);

            TryTerminate(); // In case SHUTDOWN and now empty
        }

        /// <summary> 
        /// Starts a core thread, causing it to idly wait for work. This
        /// overrides the default policy of starting core threads only when
        /// new tasks are executed. This method will return <c>false</c>
        /// if all core threads have already been started.
        /// </summary>
        /// <returns><c>true</c> if a thread was started.</returns>
        public bool PreStartCoreThread()
        {
            return WorkerCountOf(_controlState.Value) < _corePoolSize &&
                   AddWorker(null, true);
        }

        /// <summary> 
        /// Starts all core threads, causing them to idly wait for work. 
        /// </summary>
        /// <remarks>
        /// This overrides the default policy of starting core threads only when
        /// new tasks are executed.
        /// </remarks>
        /// <returns>the number of threads started.</returns>
        public int PreStartAllCoreThreads()
        {
            var n = 0;
            while (AddWorker(null, true))
                ++n;
            return n;
        }

        #endregion

        #region Non-public Methods

        private const bool ONLY_ONE = true;

        /// <summary>
        /// Transitions control state to given target or leaves if alone if
        /// already at least the given target.
        /// </summary>
        /// <param name="targetState">the desired state, either SHUTDOWN or STOP ( but 
        /// not TIDYING or TERMINATED -- use TryTerminate for that )</param>
        private void AdvanceRunState(int targetState)
        {
            for (; ; )
            {
                var state = _controlState.Value;
                if (RunStateAtLeast(state, targetState) ||
                    _controlState.CompareAndSet(state, ControlOf(targetState, WorkerCountOf(state))))
                    break;
            }
        }

        /// <summary> 
        /// Transitions to TERMINATED state if either (SHUTDOWN and pool
        /// and queue empty) or (STOP and pool empty).  If otherwise
        /// eligible to terminate but workerCount is nonzero, interrupts an
        /// idle worker to ensure that shutdown signals propagate. This
        /// method must be called following any action that might make
        /// termination possible -- reducing worker count or removing tasks
        /// from the queue during shutdown. The method is non-private to
        /// allow access from ScheduledThreadPoolExecutor.
        /// </summary>
        private void TryTerminate()
        {
            for (; ; )
            {
                var c = _controlState.Value;
                if (IsRunning(c) ||
                    RunStateAtLeast(c, TIDYING) ||
                    (RunStateOf(c) == SHUTDOWN && _workQueue.Count > 0))
                {
                    return;
                }
                if (WorkerCountOf(c) != 0)
                {
                    // Eligible to terminate
                    InterruptIdleWorkers(ONLY_ONE);
                    return;
                }

                using(_mainLock.Lock())
                {
                    if (_controlState.CompareAndSet(c, ControlOf(TIDYING, 0)))
                    {
                        try
                        {
                            Terminated();
                        }
                        finally
                        {
                            _controlState.Value = ControlOf(TERMINATED, 0);
                            _termination.SignalAll();
                        }
                        return;
                    }
                }
            }
        }

        /// <summary> 
        /// Interrupts all threads, even if active. Ignores SecurityExceptions
        /// (in which case some threads may remain uninterrupted).
        /// </summary> 
        private void InterruptWorkers()
        {
            using(_mainLock.Lock())
            {
                foreach (var worker in _currentWorkerThreads.Keys)
                {
                    try { worker.Thread.Interrupt(); }
                    catch (SecurityException) { } // ignore
                }
            }
        }

        /// <summary> 
        /// Drains the task queue into a new list, normally using
        /// drainTo. But if the queue is a DelayQueue or any other kind of
        /// queue for which poll or drainTo may fail to remove some
        /// elements, it deletes them one by one.
        /// </summary> 
        private IList<IRunnable> DrainQueue()
        {
            var q = _workQueue;
            IList<IRunnable> taskList = new List<IRunnable>();
            q.DrainTo(taskList);
            if (q.Count > 0)
            {
                foreach (var runnable in q)
                {
                    if (q.Remove(runnable))
                        taskList.Add(runnable);
                }
            }
            return taskList;
        }

        /// <summary>
        /// Main worker run loop.  Repeatedly gets tasks from queue and
        /// executes them, while coping with a number of issues:
        ///
        /// 1. We may start out with an initial task, in which case we
        /// don't need to get the first one. Otherwise, as long as pool is
        /// running, we get tasks from GetTask. If it returns null then the
        /// worker exits due to changed pool state or configuration
        /// parameters.  Other exits result from exception throws in
        /// external code, in which case completedAbruptly holds, which
        /// usually leads ProcessWorkerExit to replace this thread.
        ///
        /// 2. Before running any task, the lock is acquired to prevent
        /// other pool interrupts while the task is executing, and
        /// ClearInterruptsForTaskRun called to ensure that unless pool is
        /// stopping, this thread does not have its interrupt set.
        ///
        /// 3. Each task run is preceded by a call to BeforeExecute, which
        /// might throw an exception, in which case we cause thread to die
        /// (breaking loop with completedAbruptly true) without processing
        /// the task.
        ///
        /// 4. Assuming BeforeExecute completes normally, we run the task,
        /// gathering any of its thrown exceptions to send to
        /// AfterExecute. We separately handle RuntimeException, Error
        /// (both of which the specs guarantee that we trap) and arbitrary
        /// Throwables.  Because we cannot rethrow Throwables within
        /// Runnable.run, we wrap them within Errors on the way out (to the
        /// thread's UncaughtExceptionHandler).  Any thrown exception also
        /// conservatively causes thread to die.
        ///
        /// 5. After task.run completes, we call AfterExecute, which may
        /// also throw an exception, which will also cause thread to
        /// die. According to JLS Sec 14.20, this exception is the one that
        /// will be in effect even if task.run throws.
        ///
        /// The net effect of the exception mechanics is that AfterExecute
        /// and the thread's UncaughtExceptionHandler have as accurate
        /// information as we can provide about any problems encountered by
        /// user code.
        ///
        /// <param name="worker">the worker to run</param>
        /// </summary>
        private void RunWorker(Worker worker)
        {
            var task = worker.FirstTask;
            worker.FirstTask = null;
            var completedAbruptly = true;
            try
            {
                while (task != null || (task = GetTask()) != null)
                {
                    worker.Lock();
                    ClearInterruptsForTaskRun();
                    try
                    {
                        BeforeExecute(worker.Thread, task);
                        Exception thrown = null;
                        try
                        {
                            task.Run();
                        }
                        catch (Exception x)
                        {
                            thrown = x;
                            OnThreadException(task, x);
                        }
                        finally
                        {
                            AfterExecute(task, thrown);
                        }
                    }
                    finally
                    {
                        task = null;
                        worker.CompletedTasks++;
                        worker.Unlock();
                    }
                }
                completedAbruptly = false;
            }
            finally
            {
                ProcessWorkerExit(worker, completedAbruptly);
            }
        }

        /// <summary>
        /// Performs cleanup and bookkeeping for a dying worker. Called
        /// only from worker threads. Unless completedAbruptly is set,
        /// assumes that workerCount has already been adjusted to account
        /// for exit.  This method removes thread from worker set, and
        /// possibly terminates the pool or replaces the worker if either
        /// it exited due to user task exception or if fewer than
        /// corePoolSize workers are running or queue is non-empty but
        /// there are no workers.
        ///
        /// <param name="w">the worker</param>
        /// <param name="completedAbruptly">if the worker died to the user exception</param>
        /// </summary>
        private void ProcessWorkerExit(Worker w, bool completedAbruptly)
        {
            if (completedAbruptly) // If abrupt, then workerCount wasn't adjusted
                DecrementWorkerCount();

            using(_mainLock.Lock())
            {
                _completedTaskCount += w.CompletedTasks;
                _currentWorkerThreads.Remove(w);
            }

            TryTerminate();

            var c = _controlState.Value;
            if (!RunStateLessThan(c, STOP)) return;
            if (!completedAbruptly)
            {
                int min = _allowCoreThreadToTimeOut ? 0 : _corePoolSize;
                if (min == 0 && _workQueue.Count > 0)
                    min = 1;
                if (WorkerCountOf(c) >= min)
                    return; // replacement not needed
            }
            AddWorker(null, false);
        }

        /// <summary>
        /// Checks if a new worker can be added with respect to current
        /// pool state and the given bound (either core or maximum). If so,
        /// the worker count is adjusted accordingly, and, if possible, a
        /// new worker is created and started running firstTask as its
        /// first task. This method returns false if the pool is stopped or
        /// eligible to shut down. It also returns false if the thread
        /// factory fails to create a thread when asked, which requires a
        /// backout of workerCount, and a recheck for termination, in case
        /// the existence of this worker was holding up termination.
        /// </summary>
        ///
        /// <param name="firstTask"> the task the new thread should run first (or
        /// null if none). Workers are created with an initial first task
        /// (in method <see cref="AbstractExecutorService.Execute(IRunnable)"/>) to bypass queuing when there are fewer
        /// than <see cref="CorePoolSize"/> threads (in which case we always start one),
        /// or when the queue is full (in which case we must bypass queue).
        /// Initially idle threads are usually created via
        /// <see cref="PreStartCoreThread"/> or to replace other dying workers.
        /// </param>
        /// <param name="core">
        /// if true use <see cref="CorePoolSize"/> as bound, else
        /// <see cref="MaximumPoolSize"/>. (A bool indicator is used here rather than a
        /// value to ensure reads of fresh values after checking other pool
        /// state).</param>
        /// <returns><c>true</c> if successful</returns>
        private bool AddWorker(IRunnable firstTask, bool core)
        {
            retry:
            for (;;)
            {
                var c = _controlState.Value;
                var rs = RunStateOf(c);

                // Check if queue empty only if necessary.
                if (rs >= SHUTDOWN && !(rs == SHUTDOWN && firstTask == null && _workQueue.Count > 0))
                    return false;

                for (;;)
                {
                    int wc = WorkerCountOf(c);
                    if (wc >= CAPACITY || wc >= (core ? _corePoolSize : _maximumPoolSize))
                        return false;
                    if (CompareAndIncrementWorkerCount(c)) 
                        goto proceed;
                    c = _controlState.Value; // Re-read control state
                    if (RunStateOf(c) != rs)
                        goto retry;
                    // else CAS failed due to workerCount change; retry inner loop
                }
            }
            proceed:

            var w = new Worker(this, firstTask);
            var t = w.Thread;

            using(_mainLock.Lock())
            {
                // Recheck while holding lock.
                // Back out on ThreadFactory failure or if
                // shut down before lock acquired.
                var c = _controlState.Value;
                var rs = RunStateOf(c);

                if (t == null || (rs >= SHUTDOWN && !(rs == SHUTDOWN && firstTask == null)))
                {
                    DecrementWorkerCount();
                    TryTerminate();
                    return false;
                }

                _currentWorkerThreads[w] = w;

                var s = _currentWorkerThreads.Count;
                if (s > _largestPoolSize)
                    _largestPoolSize = s;
            }

            t.Start();
            // It is possible (but unlikely) for a thread to have been
            // added to workers, but not yet started, during transition to
            // STOP, which could result in a rare missed interrupt,
            // because Thread.interrupt is not guaranteed to have any effect
            // on a non-yet-started Thread (see Thread#interrupt).
            if (RunStateOf(_controlState.Value) == STOP && t.IsAlive)
                t.Interrupt();

            return true;
        }

        /// <summary>
        /// Attempt to CAS-increment the workerCount field of control state.
        /// </summary>
        private bool CompareAndIncrementWorkerCount(int expect)
        {
            return _controlState.CompareAndSet(expect, expect + 1);
        }

        /// <summary>
        ///Attempt to CAS-decrement the workerCount field of control state.
        /// </summary>
        private bool CompareAndDecrementWorkerCount(int expect)
        {
            return _controlState.CompareAndSet(expect, expect - 1);
        }

        /// <summary>
        /// Decrements the workerCount field of ctl. This is called only on
        /// abrupt termination of a thread (see ProcessWorkerExit). Other
        /// decrements are performed within GetTask.
        /// </summary>
        private void DecrementWorkerCount()
        {
            do { } while (! CompareAndDecrementWorkerCount(_controlState.Value));
        }

        /// <summary> 
        /// Ensures that unless the pool is stopping, the current thread
        /// does not have its interrupt set. This requires a double-check
        /// of state in case the interrupt was cleared concurrently with a
        /// shutdownNow -- if so, the interrupt is re-enabled.
        /// </summary>
        private void ClearInterruptsForTaskRun()
        {
            if (RunStateLessThan(_controlState.Value, STOP) &&
                SystemExtensions.IsCurrentThreadInterrupted() &&
                RunStateAtLeast(_controlState.Value, STOP))
            {
                Thread.CurrentThread.Interrupt();
            }
        }

        /// <summary> 
        /// State check needed by ScheduledThreadPoolExecutor to
        /// enable running tasks during shutdown.
        /// </summary>
        ///
        /// <param name="shutdownOK"><c>true</c> if should return true if SHUTDOWN.</param>
        internal virtual bool IsRunningOrShutdown(bool shutdownOK)
        {
            var rs = RunStateOf(_controlState.Value);
            return rs == RUNNING || (rs == SHUTDOWN && shutdownOK);
        }

        /// <summary> 
        /// Performs blocking or timed wait for a task, depending on
        /// current configuration settings, or returns null if this worker
        /// must exit because of any of:
        /// 1. There are more than maximumPoolSize workers (due to
        ///    a call to setMaximumPoolSize).
        /// 2. The pool is stopped.
        /// 3. The pool is shutdown and the queue is empty.
        /// 4. This worker timed out waiting for a task, and timed-out
        ///    workers are subject to termination (that is,
        ///    <c>allowCoreThreadTimeOut || workerCount > corePoolSize</c>)
        ///    both before and after the timed wait.
        /// </summary> 
        ///<returns><see cref="IRunnable"/> task or <c>null</c>
        /// if the worker must exit, in which case workerCount is decremented
        /// </returns>
        private IRunnable GetTask()
        {
            var timedOut = false; // Did the last Poll() time out?

            retry:
            for (;;)
            {
                int c = _controlState.Value;
                var rs = RunStateOf(c);

                // Check if queue empty only if necessary.
                if (rs >= SHUTDOWN && (rs >= STOP || _workQueue.Count == 0))
                {
                    DecrementWorkerCount();
                    return null;
                }

                bool timed; // Are workers subject to culling?

                for (;;)
                {
                    var wc = WorkerCountOf(c);
                    timed = _allowCoreThreadToTimeOut || wc > _corePoolSize;

                    if (wc <= _maximumPoolSize && ! (timedOut && timed))
                        break;
                    if (CompareAndDecrementWorkerCount(c))
                        return null;
                    c = _controlState.Value; // Re-read control state
                    if (RunStateOf(c) != rs)
                        goto retry;
                }

                try
                {
                    if (!timed) return _workQueue.Take();
                    IRunnable r;
                    if(_workQueue.Poll(KeepAliveTime, out r)) return r;
                    timedOut = true;
                }
                catch (ThreadInterruptedException)
                {
                    timedOut = false;
                }
            }
        }

        /// <summary> 
        /// Invokes the rejected execution handler for the given command.
        /// </summary>
        private void Reject(IRunnable command)
        {
            _rejectedExecutionHandler.RejectedExecution(command, this);
        }

        /// <summary> 
        /// Interrupts all threads that might be waiting for tasks.
        /// </summary>
        private void InterruptIdleWorkers()
        {
            InterruptIdleWorkers(false);
        }

        /// <summary> 
        /// Interrupts threads that might be waiting for tasks (as
        /// indicated by not being locked) so they can check for
        /// termination or configuration changes. Ignores
        /// SecurityExceptions (in which case some threads may remain
        /// uninterrupted).
        ///
        /// @param onlyOne If true, interrupt at most one worker. This is
        /// called only from TryTerminate when termination is otherwise
        /// enabled but there are still other workers.  In this case, at
        /// most one waiting worker is interrupted to propagate shutdown
        /// signals in case all threads are currently waiting.
        /// Interrupting any arbitrary thread ensures that newly arriving
        /// workers since shutdown began will also eventually exit.
        /// To guarantee eventual termination, it suffices to always
        /// interrupt only one idle worker, but shutdown() interrupts all
        /// idle workers so that redundant workers exit promptly, not
        /// waiting for a straggler task to finish.
        /// </summary>
        private void InterruptIdleWorkers(bool onlyOne)
        {
            using(_mainLock.Lock())
            {
                foreach (var worker in _currentWorkerThreads.Keys)
                {
                    var t = worker.Thread;
                    if (t.IsAlive && worker.TryLock())
                    {
                        try { t.Interrupt(); } 
                        catch (SecurityException) { } //ignore
                        finally { worker.Unlock(); }
                    }
                    if (onlyOne)
                        break;
                }
            }
        }

        #endregion

        #region Overriddable Methods

        /// <summary> 
        /// Method invoked prior to executing the given <see cref="IRunnable"/> in the
        /// given thread.  
        /// </summary>
        /// <remarks>
        /// This method is invoked by <paramref name="thread"/> that
        /// will execute <paramref name="runnable"/>, and may be used to re-initialize
        /// ThreadLocals, or to perform logging. This implementation does
        /// nothing, but may be customized in subclasses. <b>Note:</b> To properly
        /// nest multiple overridings, subclasses should generally invoke
        /// <i>base.BeforeExecute</i> at the end of this method.
        /// </remarks>
        /// <param name="thread">the thread that will run <paramref name="runnable"/>.</param>
        /// <param name="runnable">the task that will be executed.</param>
        protected internal virtual void BeforeExecute(Thread thread, IRunnable runnable)
        {
        }

        /// <summary>
        /// Method invoked upon completion of execution of the given Runnable.
        /// This method is invoked by the thread that executed the task. If
        /// non-null, the <paramref name="exception"/> is the unhandle
        /// exception that caused execution to terminate abruptly.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This implementation does nothing, but may be customized in
        /// subclasses. Note: To properly nest multiple overridings, subclasses
        /// should generally invoke <c>base.AfterExecute</c> at the
        /// beginning of this method.
        /// </para>
        /// <para>
        /// <b>Note:</b> When actions are enclosed in tasks (such as
        /// <see cref="FutureTask{T}"/>) either explicitly or via methods such
        /// as <see cref="IExecutorService.Submit(System.Action)"/>, these task
        /// objects catch and maintain computational exceptions, and so they do
        /// not cause abrupt termination, and the internal exceptions are
        /// <i>not</i> passed to this method. If you would like to trap both 
        /// kinds of failures in this method, you can further probe for such 
        /// cases, as in this sample subclass that prints either the direct 
        /// cause or the underlying exception if a task has been aborted:
        /// </para>
        /// <code language="c#">
        /// class ExtendedExecutor : ThreadPoolExecutor {
        ///   // ...
        ///   protected void AfterExecute(IRunnable r, Exception t) {
        ///     base.AfterExecute(r, t);
        ///     if (t == null &amp;&amp; r is IFuture) {
        ///       try {
        ///         Object result = ((IFuture) r).Get();
        ///       } catch (CancellationException ce) {
        ///           t = ce;
        ///       } catch (ExecutionException ee) {
        ///           t = ee.InnerException;
        ///       } catch (InterruptedException ie) {
        ///           Thread.CurrentThread.Interrupt(); // ignore/reset
        ///       }
        ///     }
        ///     if (t != null)
        ///       Console.WriteLine(t);
        ///   }
        /// }
        /// </code>
        /// </remarks>
        /// <param name="runnable">The runnable that has completed.</param>
        /// <param name="exception">
        /// The exception that caused termination, or <c>null</c> if
        /// execution completed normally.
        /// </param>
        protected internal virtual void AfterExecute(IRunnable runnable, Exception exception)
        {
        }

        /// <summary> 
        ///Performs any further cleanup following run state transition on
        /// invocation of shutdown.  A no-op here, but used by
        /// ScheduledThreadPoolExecutor to cancel delayed tasks.
        /// </summary>
        protected void OnShutdown()
        {
        }

        /// <summary> 
        /// Method invoked when the <see cref="IExecutor"/> has terminated.  
        /// Default implementation does nothing. 
        /// <p/>
        /// <b>Note:</b> To properly nest multiple
        /// overridings, subclasses should generally invoke
        /// <i>base.terminated</i> within this method.
        /// </summary>
        protected internal virtual void Terminated()
        {
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Shutsdown and disposes of this <see cref="ThreadPoolExecutor"/>.
        /// </summary>
        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>
        /// Helper method to dispose of this <see cref="ThreadPoolExecutor"/>
        /// </summary>
        /// <param name="disposing"><c>true</c> if being called from <see cref="Dispose()"/>,
        /// <c>false</c> if being called from finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_termination != null) Shutdown();
        }

        #region Finalizer

        /// <summary>
        /// Finalizer
        /// </summary>
        ~ThreadPoolExecutor()
        {
            Dispose(false);
        }

        #endregion

        int IRecommendParallelism.MaxParallelism
        {
            get { return CorePoolSize; }
        }

        #region Predefined RejectedExecutionHandlers

        /// <summary>
        /// A handler for rejected tasks that runs the rejected task
        /// directly in the calling thread of the <see cref="IRunnable.Run"/>
        /// method, unless the executor has been shut down, in which case the
        /// task is discarded.
        /// </summary>
        public class CallerRunsPolicy : IRejectedExecutionHandler
        {
            /// <summary>
            /// Executes task <paramref name="runnable"/> in the caller's
            /// thread, unless <paramref name="executor"/> has been shut down,
            /// in which case the task is discarded.
            ///
            /// <param name="executor">the executor attempting to execute this task</param>
            /// <param name="runnable">the runnable task requested to be executed</param>
            /// </summary>
            public void RejectedExecution(IRunnable runnable, ThreadPoolExecutor executor)
            {
                if (executor.IsShutdown) return;
                runnable.Run();
            }
        }

        /// <summary> 
        /// A <see cref="IRejectedExecutionHandler"/> for rejected tasks that
        /// throws a <see cref="RejectedExecutionException"/>.
        /// </summary>
        public class AbortPolicy : IRejectedExecutionHandler
        {
            /// <summary> 
            /// Always throws <see cref="RejectedExecutionException"/>.
            /// </summary>
            /// <param name="runnable">
            /// The <see cref="IRunnable"/> task requested to be executed.
            /// </param>
            /// <param name="executor">
            /// The <see cref="ThreadPoolExecutor"/> attempting to execute this task.
            /// </param>
            /// <exception cref="RejectedExecutionException">
            /// Always thrown upon execution.
            /// </exception>
            public virtual void RejectedExecution(IRunnable runnable, ThreadPoolExecutor executor)
            {
                throw new RejectedExecutionException("IRunnable: " + runnable + 
                    " rejected from execution by ThreadPoolExecutor: " + executor);
            }
        }

        /// <summary> 
        /// A <see cref="RejectedExecutionHandler"/> for rejected tasks that 
        /// silently discards the rejected task.
        /// </summary>
        public class DiscardPolicy : IRejectedExecutionHandler
        {
            /// <summary> 
            /// Does nothing, which has the effect of discarding task
            /// <paramref name="runnable"/>.
            /// </summary>
            /// <param name="runnable">
            /// The <see cref="IRunnable"/> task requested to be executed.
            /// </param>
            /// <param name="executor">
            /// The <see cref="ThreadPoolExecutor"/> attempting to execute this
            /// task.
            /// </param>
            public virtual void RejectedExecution(IRunnable runnable, ThreadPoolExecutor executor)
            {
            }
        }

        /// <summary> 
        /// A <see cref="IRejectedExecutionHandler"/> for rejected tasks that
        /// discards the oldest unhandled request and then retries
        /// <see cref="IExecutor.Execute(IRunnable)"/>, unless the executor
        /// is shut down, in which case the task is discarded.
        /// </summary>
        public class DiscardOldestPolicy : IRejectedExecutionHandler
        {
            /// <summary> 
            /// Obtains and ignores the next task that the <paramref name="executor"/>
            /// would otherwise execute, if one is immediately available,
            /// and then retries execution of task <paramref name="runnable"/>,
            /// unless the <paramref name="executor"/> is shut down, in which
            /// case task <paramref name="runnable"/> is instead discarded.
            /// </summary>
            /// <param name="runnable">
            /// The <see cref="IRunnable"/> task requested to be executed.
            /// </param>
            /// <param name="executor">
            /// The <see cref="ThreadPoolExecutor"/> attempting to execute this
            /// task.
            /// </param>
            public virtual void RejectedExecution(IRunnable runnable, ThreadPoolExecutor executor)
            {
                if (executor.IsShutdown) return;
                IRunnable head;
                executor.Queue.Poll(out head);
                executor.Execute(runnable);
            }
        }

        #endregion
    }
}