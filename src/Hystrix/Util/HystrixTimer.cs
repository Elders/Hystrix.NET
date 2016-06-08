// Copyright 2012 Netflix, Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Netflix.Hystrix.Util
{
    using System;
    using System.Threading;

    /// <summary>
    /// Provides timers to repeatedly call <see cref="ITimerListener"/> implementations.
    /// </summary>
    internal class HystrixTimer
    {
        /// <summary>
        /// The singleton instance of this type.
        /// </summary>
        public static readonly HystrixTimer Instance = new HystrixTimer();

        /// <summary>
        /// The logger instance for this type.
        /// </summary>
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(HystrixTimer));

        /// <summary>
        /// Prevents a default instance of the <see cref="HystrixTimer"/> class from being created.
        /// </summary>
        private HystrixTimer()
        {
        }

        /// <summary>
        /// Add a <see cref="ITimerListener"/> that will be executed until it is garbage collected or removed by clearing the returned <see cref="TimerReference"/>.
        /// NOTE: It is the responsibility of code that adds a listener via this method to clear this listener when completed.
        /// </summary>
        /// <param name="listener">ITimerListener implementation that will be triggered according to its <see cref="ITimerListener.GetIntervalTimeInMilliseconds()"/> method implementation.</param>
        /// <returns>The reference which can be used to stop the timer.</returns>
        public TimerReference AddTimerListener(ITimerListener listener)
        {
            Timer timer = new Timer(
                state =>
                {
                    try
                    {
                        listener.Tick();
                    }
                    catch (Exception e)
                    {
                        Logger.Error("Failed while ticking TimerListener", e);
                    }
                },
                null,
                listener.IntervalTimeInMilliseconds,
                listener.IntervalTimeInMilliseconds);

            return new TimerReference(listener, timer);
        }
    }
}
