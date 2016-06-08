namespace Netflix.Hystrix
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Java.Util.Concurrent;
    using Java.Util.Concurrent.Atomic;
    using Netflix.Hystrix.CircuitBreaker;
    using Netflix.Hystrix.Exceptions;
    using Netflix.Hystrix.Strategy;
    using Netflix.Hystrix.Strategy.Concurrency;
    using Netflix.Hystrix.Strategy.EventNotifier;
    using Netflix.Hystrix.Strategy.ExecutionHook;
    using Netflix.Hystrix.Strategy.Properties;
    using Netflix.Hystrix.ThreadPool;
    using Netflix.Hystrix.Util;

    public abstract class HystrixCommand
    {
        protected static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(HystrixCommand));

        internal protected readonly IHystrixCircuitBreaker circuitBreaker;
        internal protected readonly IHystrixThreadPool threadPool;
        internal protected readonly HystrixThreadPoolKey threadPoolKey;
        internal protected readonly IHystrixCommandProperties properties;
        internal protected readonly HystrixCommandMetrics metrics;

        /* result of execution (if this command instance actually gets executed, which may not occur due to request caching) */
        internal volatile ExecutionResult executionResult = ExecutionResult.Empty;

        /* If this command executed and timed-out */
        internal protected readonly AtomicBoolean isCommandTimedOut = new AtomicBoolean(false);
        internal protected readonly AtomicBoolean isExecutionComplete = new AtomicBoolean(false);
        internal protected readonly AtomicBoolean isExecutedInThread = new AtomicBoolean(false);

        internal protected readonly HystrixCommandKey commandKey;
        internal protected readonly HystrixCommandGroupKey commandGroup;

        /* FALLBACK Semaphore */
        internal readonly TryableSemaphore fallbackSemaphoreOverride;
        /* each circuit has a semaphore to restrict concurrent fallback execution */
        internal static readonly ConcurrentDictionary<string, TryableSemaphore> fallbackSemaphorePerCircuit = new ConcurrentDictionary<string, TryableSemaphore>();
        /* END FALLBACK Semaphore */

        /* EXECUTION Semaphore */
        internal readonly TryableSemaphore executionSemaphoreOverride;
        /* each circuit has a semaphore to restrict concurrent fallback execution */
        internal static readonly ConcurrentDictionary<string, TryableSemaphore> executionSemaphorePerCircuit = new ConcurrentDictionary<string, TryableSemaphore>();
        /* END EXECUTION Semaphore */

        /* used to track whenever the user invokes the command using execute(), queue() or fireAndForget() ... also used to know if execution has begun */
        internal protected AtomicLong invocationStartTime = new AtomicLong(-1);

        /// <summary>
        /// Instance of RequestCache logic.
        /// </summary>
        internal protected readonly HystrixRequestCache requestCache;

        /// <summary>
        /// Plugin implementations.
        /// </summary>
        internal protected readonly IHystrixEventNotifier eventNotifier;
        internal protected readonly IHystrixConcurrencyStrategy concurrencyStrategy;
        internal protected readonly IHystrixCommandExecutionHook executionHook;


        protected HystrixCommand(HystrixCommandGroupKey group)
            : this(new HystrixCommandSetter(group))
        {
        }
        protected HystrixCommand(HystrixCommandSetter setter)
            : this(setter.GroupKey, setter.CommandKey, setter.ThreadPoolKey, null, null, setter.CommandPropertiesDefaults, setter.ThreadPoolPropertiesDefaults, null, null, null, null, null)
        {
        }
        internal HystrixCommand(HystrixCommandGroupKey group, HystrixCommandKey key, HystrixThreadPoolKey threadPoolKey, IHystrixCircuitBreaker circuitBreaker, IHystrixThreadPool threadPool,
                HystrixCommandPropertiesSetter commandPropertiesDefaults, HystrixThreadPoolPropertiesSetter threadPoolPropertiesDefaults,
                HystrixCommandMetrics metrics, TryableSemaphore fallbackSemaphore, TryableSemaphore executionSemaphore,
                IHystrixPropertiesStrategy propertiesStrategy, IHystrixCommandExecutionHook executionHook)
        {
            /*
             * CommandGroup initialization
             */
            if (group == null)
                throw new ArgumentNullException("group");

            this.commandGroup = group;

            /*
             * CommandKey initialization
             */
            this.commandKey = key ?? new HystrixCommandKey(this.GetType());

            /*
             * Properties initialization
             */
            if (propertiesStrategy == null)
            {
                this.properties = HystrixPropertiesFactory.GetCommandProperties(this.commandKey, commandPropertiesDefaults);
            }
            else
            {
                // used for unit testing
                this.properties = propertiesStrategy.GetCommandProperties(this.commandKey, commandPropertiesDefaults);
            }

            /*
             * ThreadPoolKey
             *
             * This defines which thread-pool this command should run on.
             *
             * It uses the HystrixThreadPoolKey if provided, then defaults to use HystrixCommandGroup.
             *
             * It can then be overridden by a property if defined so it can be changed at runtime.
             */
            if (this.properties.ExecutionIsolationThreadPoolKeyOverride.Get() == null)
            {
                // we don't have a property overriding the value so use either HystrixThreadPoolKey or HystrixCommandGroup
                this.threadPoolKey = threadPoolKey ?? new HystrixThreadPoolKey(commandGroup.Name);
            }
            else
            {
                // we have a property defining the thread-pool so use it instead
                this.threadPoolKey = new HystrixThreadPoolKey(properties.ExecutionIsolationThreadPoolKeyOverride.Get());
            }

            /* strategy: HystrixEventNotifier */
            this.eventNotifier = HystrixPlugins.Instance.EventNotifier;

            /* strategy: HystrixConcurrentStrategy */
            this.concurrencyStrategy = HystrixPlugins.Instance.ConcurrencyStrategy;

            /*
             * Metrics initialization
             */
            this.metrics = metrics ?? HystrixCommandMetrics.GetInstance(this.commandKey, this.commandGroup, this.properties);

            /*
             * CircuitBreaker initialization
             */
            if (this.properties.CircuitBreakerEnabled.Get())
            {
                this.circuitBreaker = circuitBreaker ?? HystrixCircuitBreakerFactory.GetInstance(this.commandKey, this.properties, this.metrics);
            }
            else
            {
                this.circuitBreaker = new NoOpCircuitBreaker();
            }

            /* strategy: HystrixMetricsPublisherCommand */
            HystrixMetricsPublisherFactory.CreateOrRetrievePublisherForCommand(this.commandKey, this.commandGroup, this.metrics, this.circuitBreaker, this.properties);

            /* strategy: HystrixCommandExecutionHook */
            if (executionHook == null)
            {
                this.executionHook = HystrixPlugins.Instance.CommandExecutionHook;
            }
            else
            {
                // used for unit testing
                this.executionHook = executionHook;
            }

            /*
             * ThreadPool initialization
             */
            if (threadPool == null)
            {
                // get the default implementation of HystrixThreadPool
                this.threadPool = HystrixThreadPoolFactory.GetInstance(this.threadPoolKey, threadPoolPropertiesDefaults);
            }
            else
            {
                this.threadPool = threadPool;
            }

            /* fallback semaphore override if applicable */
            this.fallbackSemaphoreOverride = fallbackSemaphore;

            /* execution semaphore override if applicable */
            this.executionSemaphoreOverride = executionSemaphore;

            /* setup the request cache for this instance */
            this.requestCache = HystrixRequestCache.GetInstance(this.commandKey, this.concurrencyStrategy);
        }


        /**
         * @return {@link HystrixCommandGroupKey} used to group together multiple {@link HystrixCommand} objects.
         *         <p>
         *         The {@link HystrixCommandGroupKey} is used to represent a common relationship between commands. For example, a library or team name, the system all related commands interace with,
         *         common business purpose etc.
         */
        public HystrixCommandGroupKey CommandGroup { get { return this.commandGroup; } }

        /**
         * @return {@link HystrixCommandKey} identifying this command instance for statistics, circuit-breaker, properties, etc.
         */
        public HystrixCommandKey CommandKey { get { return this.commandKey; } }

        /**
         * @return {@link HystrixThreadPoolKey} identifying which thread-pool this command uses (when configured to run on separate threads via
         *         {@link HystrixCommandProperties#executionIsolationStrategy()}).
         */
        public HystrixThreadPoolKey ThreadPoolKey { get { return this.threadPoolKey; } }

        internal IHystrixCircuitBreaker CircuitBreaker { get { return this.circuitBreaker; } }

        /**
         * The {@link HystrixCommandMetrics} associated with this {@link HystrixCommand} instance.
         *
         * @return HystrixCommandMetrics
         */
        public HystrixCommandMetrics Metrics { get { return this.metrics; } }

        /**
         * The {@link HystrixCommandProperties} associated with this {@link HystrixCommand} instance.
         *
         * @return HystrixCommandProperties
         */
        public IHystrixCommandProperties Properties { get { return this.properties; } }

        /**
         * Allow the Collapser to mark this command instance as being used for a collapsed request and how many requests were collapsed.
         *
         * @param sizeOfBatch
         */
        internal void MarkAsCollapsedCommand(int sizeOfBatch)
        {
            Metrics.MarkCollapsed(sizeOfBatch);
            this.executionResult = this.executionResult.AddEvents(HystrixEventType.Collapsed);
        }



        public bool IsCircuitBreakerOpen { get { return this.circuitBreaker.IsOpen(); } }
        public bool IsExecutionComplete { get { return this.isExecutionComplete.Value; } }
        public bool IsExecutedInThread { get { return this.isExecutedInThread.Value; } }
        public bool IsSuccessfulExecution { get { return this.executionResult.Events.Contains(HystrixEventType.Success); } }
        public bool IsFailedExecution { get { return this.executionResult.Events.Contains(HystrixEventType.Failure); } }
        public System.Exception FailedExecutionException { get { return this.executionResult.Exception; } }
        public bool IsResponseFromFallback { get { return this.executionResult.Events.Contains(HystrixEventType.FallbackSuccess); } }
        public bool IsResponseTimedOut { get { return this.executionResult.Events.Contains(HystrixEventType.Timeout); } }
        public bool IsResponseShortCircuited { get { return this.executionResult.Events.Contains(HystrixEventType.ShortCircuited); } }
        public bool IsResponseFromCache { get { return this.executionResult.Events.Contains(HystrixEventType.ResponseFromCache); } }
        public bool IsResponseRejected
        {
            get
            {
                return this.executionResult.Events.Contains(HystrixEventType.SemaphoreRejected) ||
                    this.executionResult.Events.Contains(HystrixEventType.ThreadPoolRejected);
            }
        }
        public IEnumerable<HystrixEventType> ExecutionEvents { get { return this.executionResult.Events; } }
        public int ExecutionTimeInMilliseconds { get { return this.executionResult.ExecutionTimeInMilliseconds; } }

    }
    public abstract class HystrixCommand<R> : HystrixCommand, IHystrixExecutable<R>
    {
        protected HystrixCommand(HystrixCommandGroupKey group)
            : this(new HystrixCommandSetter(group))
        {
        }
        protected HystrixCommand(HystrixCommandSetter setter)
            : this(setter.GroupKey, setter.CommandKey, setter.ThreadPoolKey, null, null, setter.CommandPropertiesDefaults, setter.ThreadPoolPropertiesDefaults, null, null, null, null, null)
        {
        }
        internal HystrixCommand(HystrixCommandGroupKey group, HystrixCommandKey key, HystrixThreadPoolKey threadPoolKey, IHystrixCircuitBreaker circuitBreaker, IHystrixThreadPool threadPool,
                HystrixCommandPropertiesSetter commandPropertiesDefaults, HystrixThreadPoolPropertiesSetter threadPoolPropertiesDefaults,
                HystrixCommandMetrics metrics, TryableSemaphore fallbackSemaphore, TryableSemaphore executionSemaphore,
                IHystrixPropertiesStrategy propertiesStrategy, IHystrixCommandExecutionHook executionHook)
            : base(group, key, threadPoolKey, circuitBreaker, threadPool, commandPropertiesDefaults, threadPoolPropertiesDefaults, metrics, fallbackSemaphore, executionSemaphore, propertiesStrategy, executionHook)
        {
        }


        /**
         * Implement this method with code to be executed when {@link #execute()} or {@link #queue()} are invoked.
         *
         * @return R response type
         * @throws Exception
         *             if command execution fails
         */
        protected abstract R Run();

        /**
         * If {@link #execute()} or {@link #queue()} fails in any way then this method will be invoked to provide an opportunity to return a fallback response.
         * <p>
         * This should do work that does not require network transport to produce.
         * <p>
         * In other words, this should be a static or cached result that can immediately be returned upon failure.
         * <p>
         * If network traffic is wanted for fallback (such as going to MemCache) then the fallback implementation should invoke another {@link HystrixCommand} instance that protects against that network
         * access and possibly has another level of fallback that does not involve network access.
         * <p>
         * DEFAULT BEHAVIOR: It throws UnsupportedOperationException.
         *
         * @return R or throw UnsupportedOperationException if not implemented
         */
        protected virtual R GetFallback()
        {
            throw new NotSupportedException("No fallback available.");
        }

        /**
         * Used for synchronous execution of command.
         *
         * @return R
         *         Result of {@link #run()} execution or a fallback from {@link #getFallback()} if the command fails for any reason.
         * @throws HystrixRuntimeException
         *             if a failure occurs and a fallback cannot be retrieved
         * @throws HystrixBadRequestException
         *             if invalid arguments or state were used representing a user failure, not a system failure
         */
        public R Execute()
        {
            try
            {
                /* used to track userThreadExecutionTime */
                if (!invocationStartTime.CompareAndSet(-1, ActualTime.CurrentTimeInMillis))
                {
                    throw new InvalidOperationException("This instance can only be executed once. Please instantiate a new instance.");
                }
                try
                {
                    /* try from cache first */
                    if (IsRequestCachingEnabled())
                    {
                        IFuture<R> fromCache = requestCache.Get<R>(GetCacheKey());
                        if (fromCache != null)
                        {
                            /* mark that we received this response from cache */
                            metrics.MarkResponseFromCache();
                            return AsCachedFuture(fromCache).Get();
                        }
                    }

                    // mark that we're starting execution on the ExecutionHook
                    executionHook.OnStart(this);

                    /* determine if we're allowed to execute */
                    if (!circuitBreaker.AllowRequest())
                    {
                        // record that we are returning a short-circuited fallback
                        metrics.MarkShortCircuited();
                        // short-circuit and go directly to fallback
                        return GetFallbackOrThrowException(HystrixEventType.ShortCircuited, FailureType.Shortcircuit, "short-circuited");
                    }

                    try
                    {
                        if (properties.ExecutionIsolationStrategy.Get() == ExecutionIsolationStrategy.Thread)
                        {
                            // we want to run in a separate thread with timeout protection
                            return QueueInThread().Get();
                        }
                        else
                        {
                            return ExecuteWithSemaphore();
                        }
                    }
                    catch (ExecutionException)
                    {
                        // Do nothing...
                        // In the original code there's only 1 catch for RuntimeExceptions, which marks exception throw,
                        // any other exception is untouched.
                        // Unfortunately .NET doesn't have this kind of exception hierarchy, so we have to filter here
                        // the exceptions we don't want to mark.

                        throw;
                    }
                    catch
                    {
                        // count that we're throwing an exception and rethrow
                        metrics.MarkExceptionThrown();
                        throw;
                    }

                }
                catch (System.Exception e)
                {
                    if (e is HystrixBadRequestException)
                    {
                        throw (HystrixBadRequestException)e;
                    }
                    if (e.InnerException is HystrixBadRequestException)
                    {
                        throw (HystrixBadRequestException)e.InnerException;
                    }
                    if (e is HystrixRuntimeException)
                    {
                        throw (HystrixRuntimeException)e;
                    }
                    // if we have an exception we know about we'll throw it directly without the wrapper exception
                    if (e.InnerException is HystrixRuntimeException)
                    {
                        throw (HystrixRuntimeException)e.InnerException;
                    }
                    // we don't know what kind of exception this is so create a generic message and throw a new HystrixRuntimeException
                    String message = GetLogMessagePrefix() + " failed while executing.";
                    logger.Debug(message, e); // debug only since we're throwing the exception and someone higher will do something with it
                    throw new HystrixRuntimeException(FailureType.CommandException, GetType(), message, e, null);
                }
            }
            finally
            {
                RecordExecutedCommand();
            }
        }

        private R ExecuteWithSemaphore()
        {
            TryableSemaphore executionSemaphore = GetExecutionSemaphore();
            // acquire a permit
            if (executionSemaphore.TryAcquire())
            {
                try
                {
                    // we want to run it synchronously
                    R response = ExecuteCommand();
                    response = executionHook.OnComplete(this, response);
                    // put in cache
                    if (IsRequestCachingEnabled())
                    {
                        requestCache.PutIfAbsent(GetCacheKey(), AsFutureForCache(response));
                    }
                    /*
                     * We don't bother looking for whether someone else also put it in the cache since we've already executed and received a response.
                     * In this path we are synchronous so don't have the option of queuing a Future.
                     */
                    return response;
                }
                finally
                {
                    executionSemaphore.Release();

                    /* execution time on execution via semaphore */
                    RecordTotalExecutionTime(invocationStartTime.Value);
                }
            }
            else
            {
                // mark on counter
                metrics.MarkSemaphoreRejection();
                logger.Debug("HystrixCommand Execution Rejection by Semaphore"); // debug only since we're throwing the exception and someone higher will do something with it
                return GetFallbackOrThrowException(HystrixEventType.SemaphoreRejected, FailureType.RejectedSemaphoreExecution, "could not acquire a semaphore for execution");
            }
        }

        /**
         * Used for asynchronous execution of command.
         * <p>
         * This will queue up the command on the thread pool and return an {@link Future} to get the result once it completes.
         * <p>
         * NOTE: If configured to not run in a separate thread, this will have the same effect as {@link #execute()} and will block.
         * <p>
         * We don't throw an exception but just flip to synchronous execution so code doesn't need to change in order to switch a command from running on a separate thread to the calling thread.
         *
         * @return {@code Future<R>} Result of {@link #run()} execution or a fallback from {@link #getFallback()} if the command fails for any reason.
         * @throws HystrixRuntimeException
         *             if a fallback does not exist
         *             <p>
         *             <ul>
         *             <li>via {@code Future.get()} in {@link ExecutionException#getCause()} if a failure occurs</li>
         *             <li>or immediately if the command can not be queued (such as short-circuited or thread-pool/semaphore rejected)</li>
         *             </ul>
         * @throws HystrixBadRequestException
         *             via {@code Future.get()} in {@link ExecutionException#getCause()} if invalid arguments or state were used representing a user failure, not a system failure
         */
        public IFuture<R> Queue()
        {
            try
            {
                /* used to track userThreadExecutionTime */
                if (!invocationStartTime.CompareAndSet(-1, ActualTime.CurrentTimeInMillis))
                {
                    throw new InvalidOperationException("This instance can only be executed once. Please instantiate a new instance.");
                }
                if (IsRequestCachingEnabled())
                {
                    /* try from cache first */
                    IFuture<R> fromCache = requestCache.Get<R>(GetCacheKey());
                    if (fromCache != null)
                    {
                        /* mark that we received this response from cache */
                        metrics.MarkResponseFromCache();
                        return AsCachedFuture(fromCache);
                    }
                }

                // mark that we're starting execution on the ExecutionHook
                executionHook.OnStart(this);

                /* determine if we're allowed to execute */
                if (!circuitBreaker.AllowRequest())
                {
                    // record that we are returning a short-circuited fallback
                    metrics.MarkShortCircuited();
                    // short-circuit and go directly to fallback (or throw an exception if no fallback implemented)
                    return AsFuture(GetFallbackOrThrowException(HystrixEventType.ShortCircuited, FailureType.Shortcircuit, "short-circuited"));
                }

                /* nothing was found in the cache so proceed with queuing the execution */
                try
                {
                    if (properties.ExecutionIsolationStrategy.Get() == ExecutionIsolationStrategy.Thread)
                    {
                        return QueueInThread();
                    }
                    else
                    {
                        return QueueInSemaphore();
                    }
                }
                catch (System.Exception e)
                {
                    // count that we are throwing an exception and re-throw it
                    metrics.MarkExceptionThrown();
                    throw e;
                }
            }
            finally
            {
                RecordExecutedCommand();
            }
        }

        private IFuture<R> QueueInSemaphore()
        {
            TryableSemaphore executionSemaphore = GetExecutionSemaphore();
            // acquire a permit
            if (executionSemaphore.TryAcquire())
            {
                CountdownEvent executionCompleted = new CountdownEvent(1);
                try
                {
                    /**
                     * we want to run it synchronously so wrap a Future interface around the synchronous call that doesn't do any threading
                     * <p>
                     * we do this so that client code can execute .queue() and act as if its multi-threaded even if we choose to run it synchronously
                     * <p>
                     * We create the Future *before* execution so we can cache it. This allows us to dedupe calls rather than executing them all concurrently only then to find out we could have had them
                     * cached.
                     * <p>
                     * Theoretically we could do this all completely synchronously but because of caching we could have multiple threads still hitting the code in the Future we create so we need to have a
                     * CountdownLatch to make the get() block until execution is completed.
                     */

                    Reference<R> value = new Reference<R>();
                    ICommandFuture<R> responseFuture = new SemaphoreQueuedWrapperFuture<R>(value, executionCompleted, this);

                    // put in cache before executing so if multiple threads all try and execute duplicate commands we can de-dupe it
                    // they will each receive the same Future and block on the executionCompleted CountDownLatch until the execution below on the first
                    // thread completes at which point all threads who receive this cached Future will unblock and receive the same result
                    if (IsRequestCachingEnabled())
                    {
                        IFuture<R> fromCache = requestCache.PutIfAbsent(GetCacheKey(), responseFuture);
                        if (fromCache != null)
                        {
                            // another thread beat us so let's return it from the cache and skip executing the one we just created
                            /* mark that we received this response from cache */
                            metrics.MarkResponseFromCache();
                            return AsCachedFuture(fromCache);
                        }
                    }

                    // execute outside of future so that fireAndForget will still work (ie. someone calls queue() but not get()) and so that multiple requests can be deduped through request caching
                    R r = ExecuteCommand();
                    r = executionHook.OnComplete(this, r);
                    value.Value = r;

                    return responseFuture;

                }
                finally
                {
                    // mark that we're completed
                    executionCompleted.Signal();
                    // release the semaphore
                    executionSemaphore.Release();

                    /* execution time on queue via semaphore */
                    RecordTotalExecutionTime(invocationStartTime.Value);
                }
            }
            else
            {
                metrics.MarkSemaphoreRejection();
                logger.Debug("HystrixCommand Execution Rejection by Semaphore."); // debug only since we're throwing the exception and someone higher will do something with it
                // retrieve a fallback or throw an exception if no fallback available
                return AsFuture(GetFallbackOrThrowException(HystrixEventType.SemaphoreRejected, FailureType.RejectedSemaphoreExecution, "could not acquire a semaphore for execution"));
            }
        }

        private IFuture<R> QueueInThread()
        {
            // mark that we are executing in a thread (even if we end up being rejected we still were a THREAD execution and not SEMAPHORE)
            isExecutedInThread.Value = true;

            // final reference to the current calling thread so the child thread can access it if needed
            Thread callingThread = Thread.CurrentThread;

            // a snapshot of time so the thread can measure how long it waited before executing
            long timeBeforeExecution = ActualTime.CurrentTimeInMillis;

            HystrixCommand<R> _this = this;

            // wrap the synchronous execute() method in a Callable and execute in the threadpool
            QueuedExecutionFuture<R> future = new QueuedExecutionFuture<R>(this, threadPool.Executor, new HystrixContextCallable<R>(new Callable<R>(() =>
            {
                try
                {
                    // assign 'callingThread' to our NFExceptionThreadingUtility ThreadLocal variable so that if we blow up
                    // anywhere along the way the exception knows who the calling thread is and can include it in the stacktrace
                    ExceptionThreadingUtility.AssignCallingThread(callingThread);

                    // execution hook
                    executionHook.OnThreadStart(_this);

                    // count the active thread
                    threadPool.MarkThreadExecution();

                    // see if this command should still be executed, or if the requesting thread has walked away (timed-out) already
                    long timeQueued = ActualTime.CurrentTimeInMillis - timeBeforeExecution;
                    if (isCommandTimedOut.Value || (properties.ExecutionIsolationThreadTimeout.Get() != Timeout.InfiniteTimeSpan && timeQueued > properties.ExecutionIsolationThreadTimeout.Get().TotalMilliseconds))
                    {
                        /*
                         * We check isCommandTimedOut first as that is what most time outs will result in.
                         * We also check the actual time because fireAndForget() will never result in isCommandTimedOut=true since the user-thread never calls the Future.get() method.
                         * Thus, we want to ensure we don't continue with execution below if we're past the timeout duration regardless of whether the Future.get() was invoked (such as
                         * fireAndForget or when the user kicks of many
                         * calls asynchronously to come back to them later)
                         */
                        if (logger.IsDebugEnabled)
                        {
                            logger.Debug("Callable is being skipped since the user-thread has already timed-out this request after " + timeQueued + "ms.");
                        }
                        if (isCommandTimedOut.Value)
                        {
                            // we don't need to mark any stats here as that will have already been done in the Future.get() method
                        }
                        else
                        {
                            // try setting it if the Future.get() hasn't already done it
                            if (isCommandTimedOut.CompareAndSet(false, true))
                            {
                                // the Future.get() method has not been called so we'll mark it here (this can happen on fireAndForget executions)
                                metrics.MarkTimeout(timeQueued);
                            }
                        }

                        return default(R);
                    }

                    // execute the command
                    R r = ExecuteCommand();
                    return executionHook.OnComplete(_this, r);
                }
                catch (Exception)
                {
                    if (!isCommandTimedOut.Value)
                    {
                        // count (if we didn't timeout) that we are throwing an exception and re-throw it
                        metrics.MarkExceptionThrown();
                    }
                    throw;
                }
                finally
                {
                    threadPool.MarkThreadCompletion();
                    try
                    {
                        executionHook.OnThreadComplete(_this);
                    }
                    catch (System.Exception e)
                    {
                        logger.Warn("ExecutionHook.onThreadComplete threw an exception that will be ignored.", e);
                    }
                }
            })));

            // put in cache BEFORE starting so we're sure that one-and-only-one Future exists
            if (IsRequestCachingEnabled())
            {
                /*
                 * NOTE: As soon as this Future is added another thread could retrieve it and call get() before we return from this method.
                 */
                IFuture<R> fromCache = requestCache.PutIfAbsent(GetCacheKey(), future);
                if (fromCache != null)
                {
                    // another thread beat us so let's return it from the cache and skip executing the one we just created
                    /* mark that we received this response from cache */
                    metrics.MarkResponseFromCache();
                    return AsCachedFuture(fromCache);
                }
            }

            // start execution and throw an exception if rejection occurs
            future.Start(true);

            return future;
        }

        /**
         * Executes the command and marks success/failure on the circuit-breaker and calls <code>getFallback</code> if a failure occurs.
         * <p>
         * This does NOT use the circuit-breaker to determine if the command should be executed, use <code>execute()</code> for that. This method will ALWAYS attempt to execute the method.
         *
         * @return R
         */
        private R ExecuteCommand()
        {
            /**
             * NOTE: Be very careful about what goes in this method. It gets invoked within another thread in most circumstances.
             *
             * The modifications of booleans 'isResponseFromFallback' etc are going across thread-boundaries thus those
             * variables MUST be volatile otherwise they are not guaranteed to be seen by the user thread when the executing thread modifies them.
             */

            /* capture start time for logging */
            long startTime = ActualTime.CurrentTimeInMillis;
            // allow tracking how many concurrent threads are executing
            this.metrics.IncrementConcurrentExecutionCount();
            try
            {
                this.executionHook.OnRunStart(this);
                R response = Run();
                response = this.executionHook.OnRunSuccess(this, response);
                long duration = ActualTime.CurrentTimeInMillis - startTime;
                metrics.AddCommandExecutionTime(duration);

                if (isCommandTimedOut.Value)
                {
                    // the command timed out in the wrapping thread so we will return immediately
                    // and not increment any of the counters below or other such logic
                    return default(R);
                }
                else
                {
                    // report success
                    executionResult = executionResult.AddEvents(HystrixEventType.Success);
                    metrics.MarkSuccess(duration);
                    circuitBreaker.MarkSuccess();
                    eventNotifier.MarkCommandExecution(CommandKey, properties.ExecutionIsolationStrategy.Get(), TimeSpan.FromMilliseconds(duration), executionResult.Events);
                    return response;
                }
            }
            catch (HystrixBadRequestException e)
            {
                try
                {
                    System.Exception decorated = executionHook.OnRunError(this, e);
                    if (decorated is HystrixBadRequestException)
                    {
                        e = (HystrixBadRequestException)decorated;
                    }
                    else
                    {
                        logger.Warn("ExecutionHook.endRunFailure returned an exception that was not an instance of HystrixBadRequestException so will be ignored.", decorated);
                    }
                    throw e;
                }
                catch (System.Exception hookException)
                {
                    logger.Warn("Error calling ExecutionHook.endRunFailure", hookException);
                }

                /*
                 * HystrixBadRequestException is treated differently and allowed to propagate without any stats tracking or fallback logic
                 */
                throw e;
            }
            catch (System.Exception e)
            {
                try
                {
                    e = executionHook.OnRunError(this, e);
                }
                catch (System.Exception hookException)
                {
                    logger.Warn("Error calling ExecutionHook.endRunFailure", hookException);
                }

                if (isCommandTimedOut.Value)
                {
                    // http://jira/browse/API-4905 HystrixCommand: Error/Timeout Double-count if both occur
                    // this means we have already timed out then we don't count this error stat and we just return
                    // as this means the user-thread has already returned, we've already done fallback logic
                    // and we've already counted the timeout stat
                    logger.Error("Error executing HystrixCommand.run() [TimedOut]. Proceeding to fallback logic ...", e);
                    return default(R);
                }
                else
                {
                    logger.Error("Error executing HystrixCommand.run(). Proceeding to fallback logic ...", e);
                }
                // report failure
                metrics.MarkFailure(ActualTime.CurrentTimeInMillis - startTime);
                // record the exception
                executionResult = executionResult.SetException(e);
                return GetFallbackOrThrowException(HystrixEventType.Failure, FailureType.CommandException, "failed", e);
            }
            finally
            {
                metrics.DecrementConcurrentExecutionCount();
                // record that we're completed
                isExecutionComplete.Value = true;
            }
        }

        /**
         * Execute <code>getFallback()</code> within protection of a semaphore that limits number of concurrent executions.
         * <p>
         * Fallback implementations shouldn't perform anything that can be blocking, but we protect against it anyways in case someone doesn't abide by the contract.
         * <p>
         * If something in the <code>getFallback()</code> implementation is latent (such as a network call) then the semaphore will cause us to start rejecting requests rather than allowing potentially
         * all threads to pile up and block.
         *
         * @return K
         * @throws UnsupportedOperationException
         *             if getFallback() not implemented
         * @throws HystrixException
         *             if getFallback() fails (throws an Exception) or is rejected by the semaphore
         */
        private R GetFallbackWithProtection()
        {
            TryableSemaphore fallbackSemaphore = GetFallbackSemaphore();
            // acquire a permit
            if (fallbackSemaphore.TryAcquire())
            {
                try
                {
                    this.executionHook.OnFallbackStart(this);
                    return this.executionHook.OnFallbackSuccess(this, GetFallback());
                }
                catch (Exception e)
                {
                    throw executionHook.OnFallbackError(this, e);
                }
                finally
                {
                    fallbackSemaphore.Release();
                }
            }
            else
            {
                this.metrics.MarkFallbackRejection();
                logger.Debug("HystrixCommand Fallback Rejection."); // debug only since we're throwing the exception and someone higher will do something with it
                // if we couldn't acquire a permit, we "fail fast" by throwing an exception
                throw new HystrixRuntimeException(FailureType.RejectedSemaphoreFallback, GetType(), GetLogMessagePrefix() + " fallback execution rejected.", null, null);
            }
        }

        /**
         * Record the duration of execution as response or exception is being returned to the caller.
         */
        private void RecordTotalExecutionTime(long startTime)
        {
            long duration = ActualTime.CurrentTimeInMillis - startTime;
            // the total execution time for the user thread including queuing, thread scheduling, run() execution
            this.metrics.AddUserThreadExecutionTime(duration);

            /*
             * We record the executionTime for command execution.
             *
             * If the command is never executed (rejected, short-circuited, etc) then it will be left unset.
             *
             * For this metric we include failures and successes as we use it for per-request profiling and debugging
             * whereas 'metrics.addCommandExecutionTime(duration)' is used by stats across many requests.
             */
            executionResult = executionResult.SetExecutionTime((int)duration);
        }

        /**
         * Record that this command was executed in the HystrixRequestLog.
         * <p>
         * This can be treated as an async operation as it just adds a references to "this" in the log even if the current command is still executing.
         */
        private void RecordExecutedCommand()
        {
            if (properties.RequestLogEnabled.Get())
            {
                // log this command execution regardless of what happened
                if (concurrencyStrategy is HystrixConcurrencyStrategyDefault)
                {
                    // if we're using the default we support only optionally using a request context
                    if (HystrixRequestContext.IsCurrentThreadInitialized)
                    {
                        HystrixRequestLog.GetCurrentRequest(concurrencyStrategy).AddExecutedCommand(this);
                    }
                }
                else
                {
                    // if it's a custom strategy it must ensure the context is initialized
                    if (HystrixRequestLog.GetCurrentRequest(concurrencyStrategy) != null)
                    {
                        HystrixRequestLog.GetCurrentRequest(concurrencyStrategy).AddExecutedCommand(this);
                    }
                }
            }
        }

        /**
         * Get the TryableSemaphore this HystrixCommand should use if a fallback occurs.
         *
         * @param circuitBreaker
         * @param fallbackSemaphore
         * @return TryableSemaphore
         */
        private TryableSemaphore GetFallbackSemaphore()
        {
            if (fallbackSemaphoreOverride == null)
            {
                return fallbackSemaphorePerCircuit.GetOrAdd(this.commandKey.Name, w =>
                {
                    return new TryableSemaphore(this.properties.FallbackIsolationSemaphoreMaxConcurrentRequests);
                });
            }
            else
            {
                return fallbackSemaphoreOverride;
            }
        }

        /**
         * Get the TryableSemaphore this HystrixCommand should use for execution if not running in a separate thread.
         *
         * @param circuitBreaker
         * @param fallbackSemaphore
         * @return TryableSemaphore
         */
        private TryableSemaphore GetExecutionSemaphore()
        {
            if (executionSemaphoreOverride == null)
            {
                return executionSemaphorePerCircuit.GetOrAdd(this.commandKey.Name, w =>
                {
                    return new TryableSemaphore(this.properties.ExecutionIsolationSemaphoreMaxConcurrentRequests);
                });
            }
            else
            {
                return executionSemaphoreOverride;
            }
        }

        /**
         * @throws HystrixRuntimeException
         */
        private R GetFallbackOrThrowException(HystrixEventType eventType, FailureType failureType, String message)
        {
            return GetFallbackOrThrowException(eventType, failureType, message, null);
        }

        /**
         * @throws HystrixRuntimeException
         */
        private R GetFallbackOrThrowException(HystrixEventType eventType, FailureType failureType, string message, Exception e)
        {
            try
            {
                if (this.properties.FallbackEnabled.Get())
                {
                    /* fallback behavior is permitted so attempt */
                    try
                    {
                        // retrieve the fallback
                        R fallback = GetFallbackWithProtection();
                        // mark fallback on counter
                        this.metrics.MarkFallbackSuccess();
                        // record the executionResult
                        this.executionResult = this.executionResult.AddEvents(eventType, HystrixEventType.FallbackSuccess);
                        return this.executionHook.OnComplete(this, fallback);
                    }
                    catch (NotSupportedException fe)
                    {
                        logger.Debug("No fallback for HystrixCommand. ", fe); // debug only since we're throwing the exception and someone higher will do something with it
                        // record the executionResult
                        this.executionResult = this.executionResult.AddEvents(eventType);

                        /* executionHook for all errors */
                        try
                        {
                            e = this.executionHook.OnError(this, failureType, e);
                        }
                        catch (System.Exception hookException)
                        {
                            logger.Warn("Error calling IExecutionHook.OnError", hookException);
                        }

                        throw new HystrixRuntimeException(failureType, GetType(), GetLogMessagePrefix() + " " + message + " and no fallback available.", e, fe);
                    }
                    catch (System.Exception fe)
                    {
                        logger.Error("Error retrieving fallback for HystrixCommand. ", fe);
                        this.metrics.MarkFallbackFailure();
                        // record the executionResult
                        this.executionResult = this.executionResult.AddEvents(eventType, HystrixEventType.FallbackFailure);

                        /* executionHook for all errors */
                        try
                        {
                            e = this.executionHook.OnError(this, failureType, e);
                        }
                        catch (System.Exception hookException)
                        {
                            logger.Warn("Error calling ExecutionHook.onError", hookException);
                        }

                        throw new HystrixRuntimeException(failureType, GetType(), GetLogMessagePrefix() + " " + message + " and failed retrieving fallback.", e, fe);
                    }
                }
                else
                {
                    /* fallback is disabled so throw HystrixRuntimeException */

                    logger.Debug("Fallback disabled for HystrixCommand so will throw HystrixRuntimeException. ", e); // debug only since we're throwing the exception and someone higher will do something with it
                    // record the executionResult
                    this.executionResult = this.executionResult.AddEvents(eventType);

                    /* executionHook for all errors */
                    try
                    {
                        e = this.executionHook.OnError(this, failureType, e);
                    }
                    catch (System.Exception hookException)
                    {
                        logger.Warn("Error calling ExecutionHook.onError", hookException);
                    }
                    throw new HystrixRuntimeException(failureType, GetType(), GetLogMessagePrefix() + " " + message + " and fallback disabled.", e, null);
                }
            }
            finally
            {
                // record that we're completed (to handle non-successful events we do it here as well as at the end of executeCommand
                this.isExecutionComplete.Value = true;
            }
        }




        /* ******************************************************************************** */
        /* ******************************************************************************** */
        /* RequestCache */
        /* ******************************************************************************** */
        /* ******************************************************************************** */

        /**
         * Key to be used for request caching.
         * <p>
         * By default this returns null which means "do not cache".
         * <p>
         * To enable caching override this method and return a string key uniquely representing the state of a command instance.
         * <p>
         * If multiple command instances in the same request scope match keys then only the first will be executed and all others returned from cache.
         *
         * @return cacheKey
         */
        protected virtual String GetCacheKey()
        {
            return null;
        }

        private IFuture<R> AsFutureForCache(R value)
        {
            return AsFuture(value);
        }

        private IFuture<R> AsCachedFuture(IFuture<R> actualFuture)
        {
            if (!(actualFuture is ICommandFuture<R>))
            {
                throw new System.Exception("This should be a CommandFuture from the AsFutureForCache method.");
            }

            return new CachedFuture<R>(this, (ICommandFuture<R>)actualFuture);
        }

        private bool IsRequestCachingEnabled()
        {
            return this.properties.RequestCacheEnabled.Get();
        }














        /**
         * This wrapper around Future allows extending the <code>get</code> method to always include timeout functionality.
         * <p>
         * We do not want developers queueing up commands and calling the normal <code>get()</code> and blocking indefinitely.
         * <p>
         * This implementation routes all <code>get()</code> calls to <code>get(long timeout, TimeUnit unit)</code> so that timeouts occur automatically for commands executed via <code>execute()</code> or
         * <code>queue().get()</code>
         */
        private class QueuedExecutionFuture<R> : ICommandFuture<R>
        {
            private readonly HystrixCommand<R> command;
            private readonly ThreadPoolExecutor executor;
            private readonly ICallable<R> callable;

            private readonly CountdownEvent actualResponseReceived = new CountdownEvent(1);
            private readonly AtomicBoolean actualFutureExecuted = new AtomicBoolean(false);
            private R result; // the result of the get()

            private volatile ExecutionException executionException; // in case an exception is thrown
            private volatile HystrixRuntimeException rejectedException;
            private volatile IFuture<R> actualFuture = null;
            private volatile bool isInterrupted = false;
            private readonly CountdownEvent futureStarted = new CountdownEvent(1);
            private readonly AtomicBoolean started = new AtomicBoolean(false);

            public QueuedExecutionFuture(HystrixCommand<R> command, ThreadPoolExecutor executor, ICallable<R> callable)
            {
                this.command = command;
                this.executor = executor;
                this.callable = callable;
            }

            public void Start()
            {
                Start(false);
            }

            /**
             * Start execution of Callable<K> on ThreadPoolExecutor
             *
             * @param throwIfRejected
             *            since we want an exception thrown in the main queue() path but not via cached responses
             */
            public void Start(bool throwIfRejected)
            {
                // make sure we only start once
                if (this.started.CompareAndSet(false, true))
                {
                    try
                    {
                        if (!this.command.threadPool.IsQueueSpaceAvailable)
                        {
                            // we are at the property defined max so want to throw a RejectedExecutionException to simulate reaching the real max
                            throw new RejectedExecutionException("Rejected command because thread-pool queueSize is at rejection threshold.");
                        }
                        // allow the ConcurrencyStrategy to wrap the Callable if desired and then submit to the ThreadPoolExecutor
                        actualFuture = executor.Submit(this.command.concurrencyStrategy.WrapCallable(this.callable));
                    }
                    catch (RejectedExecutionException e)
                    {
                        // mark on counter
                        this.command.metrics.MarkThreadPoolRejection();
                        // use a fallback instead (or throw exception if not implemented)
                        try
                        {
                            actualFuture = this.command.AsFuture(this.command.GetFallbackOrThrowException(HystrixEventType.ThreadPoolRejected, FailureType.RejectedThreadExecution, "could not be queued for execution", e));
                        }
                        catch (HystrixRuntimeException hre)
                        {
                            actualFuture = this.command.AsFuture(hre);
                            // store this so it can be thrown to queue()
                            rejectedException = hre;
                        }
                    }
                    catch (System.Exception e)
                    {
                        // unknown exception
                        logger.Error(this.command.GetLogMessagePrefix() + ": Unexpected exception while submitting to queue.", e);
                        try
                        {
                            actualFuture = this.command.AsFuture(this.command.GetFallbackOrThrowException(HystrixEventType.ThreadPoolRejected, FailureType.RejectedThreadExecution, "had unexpected exception while attempting to queue for execution.", e));
                        }
                        catch (HystrixRuntimeException hre)
                        {
                            actualFuture = this.command.AsFuture(hre);
                            throw hre;
                        }
                    }
                    finally
                    {
                        this.futureStarted.Signal();
                    }
                }
                else
                {
                    /*
                     * This else path can occur when:
                     *
                     * - HystrixCommand.getCacheKey() is implemented
                     * - multiple threads execute a command concurrently with the same cache key
                     * - each command returns the same Future
                     * - as the Future is submitted we want only 1 thread to submit in the if block above
                     * - other threads waiting on the same Future will come through this else path and wait
                     */
                    try
                    {
                        // wait for if block above to finish on a different thread
                        this.futureStarted.Wait();
                    }
                    catch (System.Exception e)
                    {
                        isInterrupted = true;
                        logger.Error(this.command.GetLogMessagePrefix() + ": Unexpected interruption while waiting on other thread submitting to queue.", e);
                        actualFuture = this.command.AsFuture(this.command.GetFallbackOrThrowException(HystrixEventType.ThreadPoolRejected, FailureType.RejectedThreadExecution, "Unexpected interruption while waiting on other thread submitting to queue.", e));
                    }
                }

                if (throwIfRejected && rejectedException != null)
                {
                    throw rejectedException;
                }
            }

            /**
             * We override the get(long timeout, TimeUnit unit) to handle timeouts, fallbacks, etc.
             */
            public R Get(TimeSpan timeout)
            {
                /* in case another thread got to this (via cache) before the constructing thread started it, we'll optimistically try to start it and start() will ensure only one time wins */
                Start();
                // now we try to get the response
                if (this.actualFutureExecuted.CompareAndSet(false, true))
                {
                    // 1 thread will execute this 1 time
                    PerformActualGet();
                }
                else
                {
                    // all other threads/requests will go here waiting on performActualGet completing
                    this.actualResponseReceived.Wait();
                    /**
                     * I am considering putting a timeout value in this latch.await() even though performActualGet seems
                     * like it should never NOT properly call latch.countDown().
                     *
                     * One scenario I'm trying to determine if it can ever happen is a thread interrupt that prevents the finally block
                     * from doing the latch.countDown().
                     *
                     * http://docs.oracle.com/javase/tutorial/essential/exceptions/finally.html
                     *
                     * Note: If the JVM exits while the try or catch code is being executed, then the finally block may not execute.
                     * Likewise, if the thread executing the try or catch code is interrupted or killed, the finally block may not
                     * execute even though the application as a whole continues.
                     */
                }

                if (executionException != null)
                {
                    throw executionException;
                }
                else
                {
                    return result;
                }
            }

            /**
             * We override the get() to force it to always have a timeout so developers can not
             * accidentally use the HystrixCommand.queue().get() methods and block indefinitely.
             */
            public R Get()
            {
                return Get(this.command.properties.ExecutionIsolationThreadTimeout.Get());
            }

            /**
             * The actual Future.get() that we want only 1 thread to perform once.
             * <p>
             * NOTE: This sets mutable variables on this Future instance so as to do so in a thread-safe manner.
             * <p>
             * The execution of this method is protected by a CountDownLatch to occur only once and by a single thread.
             * <p>
             * As soon as this method returns all other threads are released so the correct state must be set before this method returns.
             *
             * @return
             * @throws CancellationException
             * @throws InterruptedException
             * @throws ExecutionException
             */
            private void PerformActualGet()
            {
                try
                {
                    // this check needs to be inside the try/finally so even if an exception is thrown
                    // we will countDown the latch and release threads
                    if (!this.started.Value || this.actualFuture == null)
                    {
                        /**
                         * https://github.com/Netflix/Hystrix/issues/113
                         *
                         * Output any extra information that can help tracking down how this failed
                         * as it most likely means there's a concurrency bug.
                         */
                        throw new InvalidOperationException("Response Not Available.  Key: "
                                + this.command.CommandKey.Name + "  ActualFuture: " + actualFuture
                                + "  Started: " + this.started.Value + "  actualFutureExecuted: " + this.actualFutureExecuted.Value
                                + "  futureStarted: " + this.futureStarted.CurrentCount
                                + "  isInterrupted: " + this.isInterrupted
                                + "  actualResponseReceived: " + this.actualResponseReceived.CurrentCount
                                + "  isCommandTimedOut: " + this.command.isCommandTimedOut.Value
                                + "  Events: " + String.Join(", ", command.ExecutionEvents));
                    }
                    // get on the actualFuture with timeout values from properties
                    result = this.actualFuture.Get(this.command.properties.ExecutionIsolationThreadTimeout.Get());
                }
                catch (TimeoutException e)
                {
                    // try to cancel the future (interrupt it)
                    this.actualFuture.Cancel(this.command.properties.ExecutionIsolationThreadInterruptOnTimeout.Get());
                    // mark this command as timed-out so the run() when it completes can ignore it
                    if (this.command.isCommandTimedOut.CompareAndSet(false, true))
                    {
                        // report timeout failure (or skip this if the compareAndSet failed as that means a thread-race occurred with the execution as the object lived in the queue too long)
                        this.command.metrics.MarkTimeout(ActualTime.CurrentTimeInMillis - this.command.invocationStartTime.Value);
                    }

                    try
                    {
                        result = this.command.GetFallbackOrThrowException(HystrixEventType.Timeout, FailureType.Timeout, "timed-out", e);
                    }
                    catch (HystrixRuntimeException re)
                    {
                        // we want to obey the contract of NFFuture.get() and throw an ExecutionException rather than a random RuntimeException that developers wouldn't expect
                        executionException = new ExecutionException(re);
                        // we can't capture this in execute/queue so we do it here
                        this.command.metrics.MarkExceptionThrown();
                    }
                }
                catch (ExecutionException e)
                {
                    // if the actualFuture itself throws an ExcecutionException we want to capture it
                    executionException = e;
                }
                finally
                {
                    // mark that we are done and other threads can proceed
                    actualResponseReceived.Signal();

                    /* execution time on threaded execution */
                    this.command.RecordTotalExecutionTime(this.command.invocationStartTime.Value);
                }
            }

            /**
             * Allow retrieving the executionResult from 1 Future in another Future (due to request caching).
             *
             * @return
             */
            public ExecutionResult GetExecutionResult()
            {
                return this.command.executionResult;
            }

            public bool Cancel(bool mayInterruptIfRunning)
            {
                // we don't want to allow canceling
                return false;
            }

            public bool IsCancelled
            {
                get
                {
                    /* in case another thread got to this (via cache) before the constructing thread started it, we'll optimistically try to start it and start() will ensure only one time wins */
                    Start();
                    /* now 'actualFuture' will be set to something */
                    return actualFuture.IsCancelled;
                }
            }
            public bool IsDone
            {
                get
                {
                    /* in case another thread got to this (via cache) before the constructing thread started it, we'll optimistically try to start it and start() will ensure only one time wins */
                    Start();
                    /* now 'actualFuture' will be set to something */
                    return actualFuture.IsDone;
                }
            }


            //object IFuture.Get()
            //{
            //    return Get();
            //}
            //object IFuture.Get(TimeSpan timeout)
            //{
            //    return Get(timeout);
            //}
        }

        internal IFuture<R> AsFuture(R value)
        {
            return new FakeFuture<R>(value, this.executionResult);
        }
        internal IFuture<R> AsFuture(HystrixRuntimeException e)
        {
            return new ExceptionFuture<R>(e, this.executionResult);
        }

        internal string GetLogMessagePrefix()
        {
            return CommandKey.Name;
        }
    }
}
