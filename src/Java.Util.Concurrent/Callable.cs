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
    /// A class converts <see cref="Func{T}"/> delegate to 
    /// <see cref="ICallable{T}"/>.
    /// </summary>
    /// <typeparam name="T">Data type of the result to be returned.</typeparam>
    /// <author>Kenneth Xu</author>
    public class Callable<T> : ICallable<T> //NET_ONLY
    {
        private readonly Func<T> _call;

        /// <summary>
        /// Construct a new instance of <see cref="Callable{T}"/> which calls
        /// <paramref name="call"/> delegate with its <see cref="Call"/> method
        /// is invoked.
        /// </summary>
        /// <param name="call">
        /// The delegate to be called when <see cref="Call"/> is invoked.
        /// </param>
        public Callable(Func<T> call)
        {
            if(call==null) throw new ArgumentNullException("call");
            _call = call;
        }

        /// <summary>
        /// Perform some action that returns a result or throws an exception.
        /// </summary>
        ///<returns>The result of the action.</returns>
        public virtual T Call()
        {
            return _call();
        }

        /// <summary>
        /// Implicitly converts <see cref="Func{T}"/> delegate to an instance
        /// of <see cref="Callable{T}"/>.
        /// </summary>
        /// <param name="call">
        /// The delegate to be converted to <see cref="Callable{T}"/>.
        /// </param>
        /// <returns>
        /// An instance of <see cref="Callable{T}"/> based on <paramref name="call"/>.
        /// </returns>
        public static implicit operator Callable<T>(Func<T> call)
        {
            return call == null ? null : new Callable<T>(call);
        }

        /// <summary>
        /// Implicitly converts <see cref="Callable{T}"/> to <see cref="Func{T}"/>
        /// delegate.
        /// </summary>
        /// <param name="callable">
        /// The callable to be converted to <see cref="Func{T}"/>.
        /// </param>
        /// <returns>
        /// The original <see cref="Func{T}"/> delegate used to construct the
        /// <paramref name="callable"/>.
        /// </returns>
        public static implicit operator Func<T>(Callable<T> callable)
        {
            return callable == null ? null : callable._call;
        }
    }
}
