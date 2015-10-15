#region License

/*
* Copyright (C)2008-2009 the original author or authors.
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
    /// Class to convert <see cref="Action"/> to <see cref="IRunnable"/>.
    /// </summary>
    /// <author>Kenneth Xu</author>
    public class Runnable : IRunnable //NET_ONLY
    {
        /// <summary>
        /// The action to execute;
        /// </summary>
        internal readonly Action _action;

        /// <summary>
        /// Construct a new instance of <see cref="Runnable"/> which calls
        /// <paramref name="action"/> delegate with its <see cref="Run"/> method
        /// is invoked.
        /// </summary>
        /// <param name="action">
        /// The delegate to be called when <see cref="Run"/> is invoked.
        /// </param>
        public Runnable(Action action)
        {
            if (action == null) throw new ArgumentNullException("action");
            _action = action;
        }

        #region IRunnable Members

        /// <summary>
        /// The entry point. Invokes the delegate passed to the constructor
        /// <see cref="Runnable(Action)"/>.
        /// </summary>
        public virtual void Run()
        {
            _action();
        }

        #endregion

        /// <summary>
        /// Implicitly converts <see cref="Action"/> delegate to an instance
        /// of <see cref="Runnable"/>.
        /// </summary>
        /// <param name="action">
        /// The delegate to be converted to <see cref="Runnable"/>.
        /// </param>
        /// <returns>
        /// An instance of <see cref="Runnable"/> based on <paramref name="action"/>.
        /// </returns>
        public static explicit operator Runnable(Action action)
        {
            return action == null ? null : new Runnable(action);
        }

        /// <summary>
        /// Implicitly converts <see cref="Runnable"/> to <see cref="Action"/>
        /// delegate.
        /// </summary>
        /// <param name="runnable">
        /// The callable to be converted to <see cref="Action"/>.
        /// </param>
        /// <returns>
        /// The original <see cref="Action"/> delegate used to construct the
        /// <paramref name="runnable"/>.
        /// </returns>
        public static explicit operator Action(Runnable runnable)
        {
            return runnable == null ? null : runnable._action;
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to 
        /// the current <see cref="Runnable"/>.
        /// </summary>
        /// <returns>
        /// true if the <paramref name="obj"/> is of type <see cref="Runnable"/> 
        /// and it carrys the same inner action as current instance; otherwise, false.
        /// </returns>
        /// <param name="obj">
        /// The <see cref="object"/> to compare with the current instance. 
        /// </param>
        /// <filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            Type type = obj.GetType();
            if (type != typeof(Runnable) &&
                type != typeof(ContextCopyingRunnable)) return false;
            return Equals((Runnable)obj);
        }

        /// <summary>
        /// Determines whether the specified <paramref name="other"/> instance 
        /// is equal to the current <see cref="Runnable"/>.
        /// </summary>
        /// <returns>
        /// true if the <paramref name="other"/> carrys the same inner action 
        /// as current instance; otherwise, false.
        /// </returns>
        /// <param name="other">
        /// The other <see cref="Runnable"/> to compare with the current instance. 
        /// </param>
        public virtual bool Equals(Runnable other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._action, _action);
        }

        /// <summary>
        /// Returns the has code of the inner action. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="Runnable"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return _action.GetHashCode();
        }
    }
}
