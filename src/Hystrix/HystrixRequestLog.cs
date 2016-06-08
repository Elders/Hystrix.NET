namespace Netflix.Hystrix
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Java.Util.Concurrent;
    using Netflix.Hystrix.Strategy;
    using Netflix.Hystrix.Strategy.Concurrency;

    /// <summary>
    /// Log of <see cref="HystrixCommand"/> executions and events during the current request.
    /// </summary>
    public class HystrixRequestLog
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(HystrixRequestLog));

        /**
         * RequestLog: Reduce Chance of Memory Leak
         * https://github.com/Netflix/Hystrix/issues/53
         * 
         * Upper limit on RequestLog before ignoring further additions and logging warnings.
         * 
         * Intended to help prevent memory leaks when someone isn't aware of the
         * HystrixRequestContext lifecycle or enabling/disabling RequestLog.
         */
        public const int MaxStorage = 1000;

        private static readonly HystrixRequestVariableHolder<HystrixRequestLog> currentRequestLog =
            new HystrixRequestVariableHolder<HystrixRequestLog>(new DelegateRequestVariableLifecycle<HystrixRequestLog>(() => new HystrixRequestLog()));

        /**
         * History of {@link HystrixCommand} executed in this request.
         */
        private LinkedBlockingQueue<HystrixCommand> executedCommands = new LinkedBlockingQueue<HystrixCommand>(MaxStorage);

        // prevent public instantiation
        private HystrixRequestLog()
        {
        }

        /**
         * {@link HystrixRequestLog} for current request as defined by {@link HystrixRequestContext}.
         * 
         * @return {@link HystrixRequestLog}
         */
        public static HystrixRequestLog GetCurrentRequest(IHystrixConcurrencyStrategy concurrencyStrategy)
        {
            return currentRequestLog.Get(concurrencyStrategy);
        }

        /**
         * {@link HystrixRequestLog} for current request as defined by {@link HystrixRequestContext}.
         * <p>
         * NOTE: This uses the default {@link HystrixConcurrencyStrategy} or global override. If an injected strategy is being used by commands you must instead use
         * {@link #getCurrentRequest(HystrixConcurrencyStrategy)}.
         * 
         * @return {@link HystrixRequestLog}
         */
        public static HystrixRequestLog GetCurrentRequest()
        {
            return currentRequestLog.Get(HystrixPlugins.Instance.ConcurrencyStrategy);
        }

        /**
         * Retrieve {@link HystrixCommand} instances that were executed during this {@link HystrixRequestContext}.
         * 
         * @return {@code Collection<HystrixCommand<?>>}
         */
        public IEnumerable<HystrixCommand> ExecutedCommands
        {
            get
            {
                return executedCommands;
            }
        }

        /**
         * Add {@link HystrixCommand} instance to the request log.
         * 
         * @param command
         *            {@code HystrixCommand<?>}
         */
        internal void AddExecutedCommand(HystrixCommand command)
        {
            if (!executedCommands.Offer(command))
            {
                // see RequestLog: Reduce Chance of Memory Leak https://github.com/Netflix/Hystrix/issues/53
                logger.Warn("RequestLog ignoring command after reaching limit of " + MaxStorage + ". See https://github.com/Netflix/Hystrix/issues/53 for more information.");
            }
        }


        /**
         * Formats the log of executed commands into a string usable for logging purposes.
         * <p>
         * Examples:
         * <ul>
         * <li>TestCommand[SUCCESS][1ms]</li>
         * <li>TestCommand[SUCCESS][1ms], TestCommand[SUCCESS, RESPONSE_FROM_CACHE][1ms]x4</li>
         * <li>TestCommand[TIMEOUT][1ms]</li>
         * <li>TestCommand[FAILURE][1ms]</li>
         * <li>TestCommand[THREAD_POOL_REJECTED][1ms]</li>
         * <li>TestCommand[THREAD_POOL_REJECTED, FALLBACK_SUCCESS][1ms]</li>
         * <li>TestCommand[FAILURE, FALLBACK_SUCCESS][1ms], TestCommand[FAILURE, FALLBACK_SUCCESS, RESPONSE_FROM_CACHE][1ms]x4</li>
         * <li>GetData[SUCCESS][1ms], PutData[SUCCESS][1ms], GetValues[SUCCESS][1ms], GetValues[SUCCESS, RESPONSE_FROM_CACHE][1ms], TestCommand[FAILURE, FALLBACK_FAILURE][1ms], TestCommand[FAILURE,
         * FALLBACK_FAILURE, RESPONSE_FROM_CACHE][1ms]</li>
         * </ul>
         * <p>
         * If a command has a multiplier such as <code>x4</code> that means this command was executed 4 times with the same events. The time in milliseconds is the sum of the 4 executions.
         * <p>
         * For example, <code>TestCommand[SUCCESS][15ms]x4</code> represents TestCommand being executed 4 times and the sum of those 4 executions was 15ms. These 4 each executed the run() method since
         * <code>RESPONSE_FROM_CACHE</code> was not present as an event.
         * 
         * @return String request log or "Unknown" if unable to instead of throwing an exception.
         */
        public string GetExecutedCommandsAsString()
        {
            try
            {
                Dictionary<string, int> aggregatedCommandsExecuted = new Dictionary<string, int>();
                Dictionary<string, int> aggregatedCommandExecutionTime = new Dictionary<string, int>();

                foreach (HystrixCommand command in this.executedCommands)
                {
                    StringBuilder displayString = new StringBuilder();
                    displayString.Append(command.CommandKey.Name);

                    List<HystrixEventType> events = new List<HystrixEventType>(command.ExecutionEvents);
                    if (events.Count > 0)
                    {
                        events.Sort();
                        displayString.Append("[" + String.Join(", ", events) + "]");
                    }
                    else
                    {
                        displayString.Append("[Executed]");
                    }

                    string display = displayString.ToString();
                    if (aggregatedCommandsExecuted.ContainsKey(display))
                    {
                        // increment the count
                        aggregatedCommandsExecuted[display] = aggregatedCommandsExecuted[display] + 1;
                    }
                    else
                    {
                        // add it
                        aggregatedCommandsExecuted.Add(display, 1);
                    }

                    int executionTime = command.ExecutionTimeInMilliseconds;
                    if (executionTime < 0)
                    {
                        // do this so we don't create negative values or subtract values
                        executionTime = 0;
                    }
                    if (aggregatedCommandExecutionTime.ContainsKey(display))
                    {
                        // add to the existing executionTime (sum of executionTimes for duplicate command displayNames)
                        aggregatedCommandExecutionTime[display] = aggregatedCommandExecutionTime[display] + executionTime;
                    }
                    else
                    {
                        // add it
                        aggregatedCommandExecutionTime.Add(display, executionTime);
                    }

                }

                StringBuilder header = new StringBuilder();
                foreach (string displayString in aggregatedCommandsExecuted.Keys)
                {
                    if (header.Length > 0)
                    {
                        header.Append(", ");
                    }
                    header.Append(displayString);

                    int totalExecutionTime = aggregatedCommandExecutionTime[displayString];
                    header.Append("[").Append(totalExecutionTime).Append("ms]");

                    int count = aggregatedCommandsExecuted[displayString];
                    if (count > 1)
                    {
                        header.Append("x").Append(count);
                    }
                }
                return header.ToString();
            }
            catch (System.Exception e)
            {
                logger.Error("Failed to create HystrixRequestLog response header string.", e);
                // don't let this cause the entire app to fail so just return "Unknown"
                return "Unknown";
            }
        }
    }
}
