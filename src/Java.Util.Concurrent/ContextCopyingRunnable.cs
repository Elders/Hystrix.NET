#region License

/*
 * Copyright (C) 2002-2008 the original author or authors.
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
    /// An implementation of <see cref="IRunnable"/> that restores the context
    /// before it is run.
    /// </summary>
    /// <author>Kenneth Xu</author>
    internal class ContextCopyingRunnable : Runnable, IContextCopyingTask //NET_ONLY
    {
        private IContextCarrier _contextCarrier;

        public ContextCopyingRunnable(IRunnable runnable, IContextCarrier contextCarrier)
            : base(ExtractAction(runnable))
        {
            _contextCarrier = contextCarrier;
        }

        private static Action ExtractAction(IRunnable runnable)
        {
            var r = runnable as Runnable;
            return r == null ? runnable.Run : r._action;
        }

        public override void Run()
        {
            if(_contextCarrier!=null) _contextCarrier.Restore();
            base.Run();
        }

        #region IContextCopyingTask Members

        /// <summary>
        /// Gets and sets the <see cref="IContextCarrier"/> that captures
        /// and restores the context.
        /// </summary>
        public IContextCarrier ContextCarrier
        {
            get { return _contextCarrier; }
            set { _contextCarrier = value; }
        }

        #endregion
    }
}
