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

namespace Netflix.Hystrix.CircuitBreaker
{
    using System;
    using Java.Util.Concurrent.Atomic;
    using Netflix.Hystrix.Util;

    /// <summary>
    /// The default production implementation of <see cref="IHystrixCircuitBreaker"/>.
    /// </summary>
    internal class HystrixCircuitBreakerImpl : IHystrixCircuitBreaker
    {
        /// <summary>
        /// Stores the properties of the owner command.
        /// </summary>
        private readonly IHystrixCommandProperties properties;
        
        /// <summary>
        /// Stores the metrics of the owner command.
        /// </summary>
        private readonly HystrixCommandMetrics metrics;
        
        /// <summary>
        /// Stores the state of this circuit breaker.
        /// </summary>
        private AtomicBoolean circuitOpen = new AtomicBoolean(false);

        /// <summary>
        /// Stores the last time the circuit breaker was opened or tested.
        /// </summary>
        private AtomicLong circuitOpenedOrLastTestedTime = new AtomicLong();

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixCircuitBreakerImpl"/> class.
        /// </summary>
        /// <param name="properties">The properties of the owner command.</param>
        /// <param name="metrics">The metrics of the owner command.</param>
        public HystrixCircuitBreakerImpl(IHystrixCommandProperties properties, HystrixCommandMetrics metrics)
        {
            if (properties == null)
            {
                throw new ArgumentNullException("properties");
            }

            if (metrics == null)
            {
                throw new ArgumentNullException("metrics");
            }

            this.properties = properties;
            this.metrics = metrics;
        }

        /// <inheritdoc />
        public bool AllowRequest()
        {
            if (this.properties.CircuitBreakerForceOpen.Get())
            {
                // properties have asked us to force the circuit open so we will allow NO requests
                return false;
            }

            if (this.properties.CircuitBreakerForceClosed.Get())
            {
                // we still want to allow IsOpen() to perform it's calculations so we simulate normal behavior
                this.IsOpen();

                // properties have asked us to ignore errors so we will ignore the results of isOpen and just allow all traffic through
                return true;
            }

            return !this.IsOpen() || this.AllowSingleTest();
        }

        /// <inheritdoc />
        public bool IsOpen()
        {
            if (this.circuitOpen.Value)
            {
                // if we're open we immediately return true and don't bother attempting to 'close' ourself as that is left to allowSingleTest and a subsequent successful test to close
                return true;
            }

            // we're closed, so let's see if errors have made us so we should trip the circuit open
            HealthCounts health = this.metrics.GetHealthCounts();

            // check if we are past the statisticalWindowVolumeThreshold
            if (health.TotalRequests < this.properties.CircuitBreakerRequestVolumeThreshold.Get())
            {
                // we are not past the minimum volume threshold for the statisticalWindow so we'll return false immediately and not calculate anything
                return false;
            }

            if (health.ErrorPercentage < this.properties.CircuitBreakerErrorThresholdPercentage.Get())
            {
                return false;
            }
            else
            {
                // our failure rate is too high, trip the circuit
                if (this.circuitOpen.CompareAndSet(false, true))
                {
                    // if the previousValue was false then we want to set the currentTime
                    // How could previousValue be true? If another thread was going through this code at the same time a race-condition could have
                    // caused another thread to set it to true already even though we were in the process of doing the same
                    this.circuitOpenedOrLastTestedTime.Value = ActualTime.CurrentTimeInMillis;
                }

                return true;
            }
        }

        /// <inheritdoc />
        public void MarkSuccess()
        {
            if (this.circuitOpen)
            {
                // If we have been 'open' and have a success then we want to close the circuit. This handles the 'singleTest' logic
                this.circuitOpen.Value = false;

                // TODO how can we can do this without resetting the counts so we don't lose metrics of short-circuits etc?
                this.metrics.ResetCounter();
            }
        }

        /// <summary>
        /// Gets whether the circuit breaker should permit a single test request.
        /// </summary>
        /// <returns>True if single test is permitted, otherwise false.</returns>
        private bool AllowSingleTest()
        {
            long timeCircuitOpenedOrWasLastTested = this.circuitOpenedOrLastTestedTime.Value;

            // 1) if the circuit is open
            // 2) and it's been longer than 'sleepWindow' since we opened the circuit
            if (this.circuitOpen.Value && ActualTime.CurrentTimeInMillis > timeCircuitOpenedOrWasLastTested + this.properties.CircuitBreakerSleepWindow.Get().TotalMilliseconds)
            {
                // We push the 'circuitOpenedTime' ahead by 'sleepWindow' since we have allowed one request to try.
                // If it succeeds the circuit will be closed, otherwise another singleTest will be allowed at the end of the 'sleepWindow'.
                if (this.circuitOpenedOrLastTestedTime.CompareAndSet(timeCircuitOpenedOrWasLastTested, ActualTime.CurrentTimeInMillis))
                {
                    // if this returns true that means we set the time so we'll return true to allow the singleTest
                    // if it returned false it means another thread raced us and allowed the singleTest before we did
                    return true;
                }
            }

            return false;
        }
    }
}
