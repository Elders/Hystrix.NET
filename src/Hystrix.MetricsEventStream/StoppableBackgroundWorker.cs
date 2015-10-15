// Copyright 2013 Loránd Biró
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

namespace Hystrix.MetricsEventStream
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Threading;
    using slf4net;

    /// <summary>
    /// A base class for background thread execution, supporting graceful shutdown.
    /// </summary>
    public abstract class StoppableBackgroundWorker : IDisposable
    {
        /// <summary>
        /// The logger instance to report events of the background workers.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(StoppableBackgroundWorker));

        /// <summary>
        /// The background thread running the <see cref="BackgroundThreadMain"/> method. It is recreated after every start.
        /// </summary>
        private Thread backgroundThread;

        /// <summary>
        /// The synchronization object for the state changes, so the class is thread safe.
        /// </summary>
        private object controlLocker = new object();

        /// <summary>
        /// The Stop method signals to the thread to stop through this event.
        /// </summary>
        private ManualResetEvent threadShouldStop = new ManualResetEvent(true);

        /// <summary>
        /// The <see cref="AwaitTermination"/> method wait for completion of the thread, which is signalled through this event.
        /// </summary>
        private ManualResetEvent threadStopped = new ManualResetEvent(true);

        /// <summary>
        /// Tracks whether Dispose has been called.
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="StoppableBackgroundWorker"/> class.
        /// </summary>
        /// <param name="name">The name of the worker and the background thread.</param>
        protected StoppableBackgroundWorker(string name)
        {
            this.Name = name;
            this.IsRunning = false;
            this.IsStopping = false;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="StoppableBackgroundWorker"/> class.
        /// </summary>
        ~StoppableBackgroundWorker()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Occurs when the background thread is stopped. If the thread stopped with an exception,
        /// the arguments will contain the exception.
        /// </summary>
        public event AsyncCompletedEventHandler Stopped;

        /// <summary>
        /// Gets a value indicating whether the worker thread is running or not. This property is true until the background thread
        /// is completely stopped.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the worker thread is stopping or not. This property change to true after <see cref="Stop"/>
        /// is called.
        /// </summary>
        public bool IsStopping { get; private set; }

        /// <summary>
        /// Gets the name of the background worker. This name is used to create the background thread.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Starts the background thread.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the worker is already running.</exception>
        /// <exception cref="ObjectDisposedException">If the worker is disposed.</exception>
        public void Start()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(null);
            }

            lock (this.controlLocker)
            {
                if (this.IsRunning)
                {
                    throw new InvalidOperationException("Background worker is already running.");
                }

                Logger.Debug(CultureInfo.InvariantCulture, "Starting {0}.", this.Name);

                this.OnStarting();

                this.threadShouldStop.Reset();
                this.threadStopped.Reset();

                this.backgroundThread = new Thread(new ThreadStart(this.BackgroundThreadMain));
                this.backgroundThread.Name = this.Name;
                this.backgroundThread.IsBackground = true;
                this.backgroundThread.Start();

                this.IsRunning = true;
            }
        }

        /// <summary>
        /// Signals to the background thread to stop gracefully. If the worker is not running, or still stopping, this method does nothing.
        /// </summary>
        /// <exception cref="ObjectDisposedException">If the worker is disposed.</exception>
        public void Stop()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(null);
            }

            lock (this.controlLocker)
            {
                if (!this.IsRunning || this.IsStopping)
                {
                    return;
                }

                Logger.Debug(CultureInfo.InvariantCulture, "Stopping {0}.", this.Name);

                this.OnStopping();

                this.IsStopping = true;
                this.threadShouldStop.Set();
            }
        }

        /// <summary>
        /// Block the current thread until the background thread is completely stopped. If the worker is not running, the method returns immediately.
        /// </summary>
        /// <exception cref="ObjectDisposedException">If the worker is disposed.</exception>
        public void AwaitTermination()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(null);
            }

            this.threadStopped.WaitOne();
        }

        /// <summary>
        /// Releases managed resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposable pattern method.
        /// </summary>
        /// <param name="disposing">True if disposing objects in heap is allowed, false if no other object should be accessed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.Stop();
                    this.threadShouldStop.Dispose();
                    this.threadStopped.Dispose();
                }

                this.disposed = true;
            }
        }

        /// <summary>
        /// Sleeps the background thread for the specified duration. The sleep will be interrupted if <see cref="Stop"/> is called.
        /// Returns whether the background thread should stop or not.
        /// </summary>
        /// <param name="duration">The duration to sleep.</param>
        /// <returns>True if the background thread should stop, otherwise false.</returns>
        protected bool SleepAndGetShouldStop(TimeSpan duration)
        {
            if (Thread.CurrentThread != this.backgroundThread)
            {
                throw new InvalidOperationException("This method can be called only from the background thread.");
            }

            return this.threadShouldStop.WaitOne(duration);
        }

        /// <summary>
        /// This method is called from the <see cref="Start"/> method before creating and starting the background thread.
        /// Implement this to extend the starting procedure.
        /// </summary>
        protected virtual void OnStarting()
        {
        }

        /// <summary>
        /// This method is called from the <see cref="Stop"/> method before signaling to the background thread to stop.
        /// Implement this to extend the stopping procedure.
        /// </summary>
        protected virtual void OnStopping()
        {
        }

        /// <summary>
        /// Implement this method to define the background work.
        /// </summary>
        protected abstract void DoWork();

        /// <summary>
        /// The main method for the background thread which is just a wrapper for the abstract <see cref="DoWork"/> method.
        /// Implements stop signaling and calls the <see cref="Stopped"/> event.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We must catch here everything, because any uncatched exception would terminate the process.")]
        private void BackgroundThreadMain()
        {
            Logger.Debug(CultureInfo.InvariantCulture, "{0} started.", this.Name);

            Exception thrownException = null;
            try
            {
                this.DoWork();
            }
            catch (Exception e)
            {
                Logger.Error(e, CultureInfo.InvariantCulture, "Unhandled exception in {0}.", this.Name);
                thrownException = e;
            }
            finally
            {
                lock (this.controlLocker)
                {
                    this.IsRunning = false;
                    this.IsStopping = false;
                    this.backgroundThread = null;
                }

                this.threadShouldStop.Set();
                this.threadStopped.Set();

                Logger.Debug(CultureInfo.InvariantCulture, "{0} stopped.", this.Name);

                AsyncCompletedEventHandler handler = this.Stopped;
                if (handler != null)
                {
                    handler(this, new AsyncCompletedEventArgs(thrownException, false, null));
                }
            }
        }
    }
}
