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

#region Imports

using System;
using System.Collections;
using System.Collections.Generic;

#endregion

namespace Java.Util
{
    /// <summary>
    /// Serve as base class to be inherited by the classes that needs to
    /// implement both the <see cref="System.Collections.IEnumerator"/> and 
    /// the <see cref="IEnumerator{T}"/> interfaces.
    /// </summary>
    /// <typeparam name="T">Type of the elements to be iterated.</typeparam>
    /// <author>Kenneth Xu</author>
    public abstract class AbstractEnumerator<T> : IEnumerator<T>, IEnumerable<T> //NET_ONLY
    {
        /// <summary>
        /// Indicates if the enumerator has not startet, is in progress, 
        /// or has already finished.
        /// </summary>
        private EnumeratorState _state = EnumeratorState.BeforeStart;

        #region IEnumerable<T> Members

        ///<summary>
        ///Returns an enumerator that iterates through the collection.
        ///</summary>
        ///
        ///<returns>
        ///A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> 
        /// that can be used to iterate through the collection.
        ///</returns>
        ///<filterpriority>1</filterpriority>
        public IEnumerator<T> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        ///
        /// <returns>
        /// true if the enumerator was successfully advanced to the next 
        /// element; false if the enumerator has passed the end of the collection.
        /// </returns>
        ///
        /// <exception cref="T:System.InvalidOperationException">
        /// The collection was modified after the enumerator was created. 
        /// </exception>
        protected abstract bool GoNext();


        /// <summary>
        /// Fetch the current element of the enumerator.
        /// </summary>
        /// <returns>The current element</returns>
        protected abstract T FetchCurrent();

        #endregion

        #region IEnumerator<T> Members

        /// <summary>
        /// Gets the element in the collection at the current position of the 
        /// enumerator.
        /// </summary>
        ///
        /// <returns>
        /// The element in the collection at the current position of the 
        /// enumerator.
        /// </returns>
        ///
        public T Current
        {
            get
            {
                return (_state == EnumeratorState.InProgress) ? FetchCurrent() : default(T);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, 
        /// or resetting unmanaged resources. This implementation does nothing.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public virtual void Dispose()
        {
        }

        object IEnumerator.Current
        {
            get
            {
                if (_state == EnumeratorState.InProgress) return FetchCurrent();
                throw new InvalidOperationException(
                    "Enumeration has either not started or has already finished.");
            }
        }

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        ///
        /// <returns>
        /// true if the enumerator was successfully advanced to the next 
        /// element; false if the enumerator has passed the end of the collection.
        /// </returns>
        ///
        /// <exception cref="T:System.InvalidOperationException">
        /// The collection was modified after the enumerator was created. 
        /// </exception>
        /// <filterpriority>2</filterpriority>
        public bool MoveNext()
        {
            bool hasNext = GoNext();
            _state = hasNext ? EnumeratorState.InProgress : EnumeratorState.AfterFinish;
            return hasNext;
        }

        /// <summary>
        /// Sets the enumerator to its initial position, which is before 
        /// the first element in the collection. This implementation
        /// always throw <see cref="NotSupportedException"/>.
        /// </summary>
        /// <remarks>
        /// This method is intentionally sealed. Derived class should override
        /// <see cref="DoReset"/> protected method instead.
        /// </remarks>
        /// <exception cref="NotSupportedException">
        /// Always thown.
        /// </exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// The collection was modified after the enumerator was created. 
        /// </exception>
        /// <filterpriority>2</filterpriority>
        public void Reset()
        {
            DoReset();
            _state = EnumeratorState.BeforeStart;
        }

        /// <summary>
        /// For derived class to implement the <see cref="Reset"/> function. 
        /// This implmenetation always throw <see cref="NotSupportedException"/>.
        /// </summary>
        protected virtual void DoReset()
        {
            throw new NotSupportedException();
        }

        #endregion

        #region Nested type: EnumeratorState

        /// <summary>
        /// Indicates if the enumerator has not startet, is in progress, 
        /// or has already finished.
        /// </summary>
        private enum EnumeratorState
        {
            /// <summary>
            /// Enuemrator has not started.
            /// </summary>
            BeforeStart,

            /// <summary>
            /// Enuemrator is in progress.
            /// </summary>
            InProgress,

            /// <summary>
            /// Enuemrator has already finished.
            /// </summary>
            AfterFinish,
        }

        #endregion
    }
}