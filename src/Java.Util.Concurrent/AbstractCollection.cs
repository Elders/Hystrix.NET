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
using System.Collections.Generic;
using System.Text;
using System.Collections;
#endregion

namespace Java.Util
{
    /// <summary>
    /// Serve as based class to be inherited by the classes that needs to
    /// implement both the <see cref="ICollection"/> and 
    /// the <see cref="ICollection{T}"/> interfaces.
    /// </summary>
    /// <remarks>
    /// <para>
    /// By inheriting from this abstract class, subclass is only required
    /// to implement the <see cref="GetEnumerator()"/> to complete a concrete
    /// read only collection class.
    /// </para>
    /// <para>
    /// <see cref="AbstractCollection{T}"/> throws <see cref="NotSupportedException"/> 
    /// for all access to the collection mutating members. 
    /// </para>
    /// </remarks>
    /// <typeparam name="T">Element type of the collection</typeparam>
    /// <author>Kenneth Xu</author>
    [Serializable]
    public abstract class AbstractCollection<T> : ICollection<T>, ICollection //NET_ONLY
    {
        #region ICollection<T> Members

        /// <summary>
        /// Adds an item to the <see cref="ICollection{T}"/>. This implementation
        /// always throw <see cref="NotSupportedException"/>.
        /// </summary>
        /// 
        /// <param name="item">
        /// The object to add to the <see cref="ICollection{T}"/>.
        /// </param>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ICollection{T}"/> is read-only. This implementation 
        /// always throw this exception.
        /// </exception>
        public virtual void Add(T item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Removes all items from the <see cref="ICollection{T}"/>. This implementation
        /// always throw <see cref="NotSupportedException"/>.
        /// </summary>
        /// 
        /// <exception cref="NotSupportedException">
        /// The <see cref="ICollection{T}"/> is read-only. This implementation always 
        /// throw exception.
        /// </exception>
        public virtual void Clear()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Determines whether the <see cref="ICollection{T}"/> contains a specific 
        /// value. This implementation searchs the element by iterating through the 
        /// enumerator returned by <see cref="GetEnumerator()"/> method.
        /// </summary>
        /// 
        /// <returns>
        /// true if item is found in the <see cref="ICollection{T}"/>; otherwise, false.
        /// </returns>
        /// 
        /// <param name="item">
        /// The object to locate in the <see cref="ICollection{T}"/>.
        /// </param>
        public virtual bool Contains(T item)
        {
            foreach (T t in this)
            {
                if (Equals(t, item)) return true;
            }
            return false;
        }

        /// <summary> 
        /// Returns an array containing all of the elements in this collection,
        /// in proper sequence.
        /// </summary>
        /// <remarks> 
        /// <para>
        /// The returned array will be "safe" in that no references to it are
        /// maintained by this collection.  (In other words, this method must
        /// allocate a new array).  The caller is thus free to modify the
        /// returned array.
        /// </para>
        /// <para>
        /// This method acts as bridge between array-based and collection-based
        /// APIs.
        /// </para>
        /// </remarks>
        /// <returns>
        /// An array containing all of the elements in this collection.
        /// </returns>
        public virtual T[] ToArray()
        {
            return DoCopyTo(null, 0, true);
        }

        /// <summary>
        /// Returns an array containing all of the elements in this collection, 
        /// in proper sequence; the runtime type of the returned array is that 
        /// of the specified array.  If the collection fits in the specified
        /// array, it is returned therein.  Otherwise, a new array is allocated
        /// with the runtime type of the specified array and the size of this 
        /// collection.
        ///	</summary>	 
        /// <remarks>
        /// <para>
        /// Like the <see cref="ToArray()"/> method, this method acts as bridge
        /// between array-based and collection-based APIs.  Further, this
        /// method allows precise control over the runtime type of the output
        /// array, and may, under certain circumstances, be used to save
        /// allocation costs.
        /// </para>
        /// <para>
        /// Suppose <i>x</i> is a collection known to contain only strings.
        /// The following code can be used to dump the collection into a newly
        /// allocated array of <see cref="string"/>s:
        /// 
        /// <code language="c#">
        ///		string[] y = (string[]) x.ToArray(new string[0]);
        ///	</code>
        /// </para>
        /// <para>
        /// Note that <i>ToArray(new T[0])</i> is identical in function to
        /// <see cref="AbstractCollection{T}.ToArray()"/>.
        /// </para>
        /// </remarks>
        /// <param name="targetArray">
        /// The array into which the elements of the colleciton are to be
        /// stored, if it is big enough; otherwise, a new array of the same 
        /// runtime type is allocated for this purpose.
        /// </param>
        /// <returns>
        /// An array containing all of the elements in this collection.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If the supplied <paramref name="targetArray"/> is
        /// <c>null</c>.
        /// </exception>
        /// <exception cref="ArrayTypeMismatchException">
        /// If type of <paramref name="targetArray"/> is a derived type of
        /// <typeparamref name="T"/> and the collection contains element that
        /// is not that derived type.
        /// </exception>
        public virtual T[] ToArray(T[] targetArray)
        {
            if (targetArray == null) throw new ArgumentNullException("targetArray");
            return DoCopyTo(targetArray, 0, true);
        }

        /// <summary>
        /// Copies the elements of the <see cref="ICollection{T}"/> to an 
        /// <see cref="Array"/>, starting at a particular <see cref="Array"/> 
        /// index.
        /// </summary>
        /// <remarks>
        /// This method is intentionally sealed. Subclass should override
        /// <see cref="DoCopyTo(T[], int, bool)"/> instead.
        /// </remarks>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the 
        /// destination of the elements copied from <see cref="ICollection{T}"/>. 
        /// The <see cref="Array"/> must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">
        /// The zero-based index in array at which copying begins.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// arrayIndex is less than 0.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// array is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// array is multidimensional.<br/>-or-<br/>
        /// arrayIndex is equal to or greater than the length of array. <br/>-or-<br/>
        /// The number of elements in the source <see cref="ICollection{T}"/> 
        /// is greater than the available space from arrayIndex to the end of 
        /// the destination array. <br/>-or-<br/>
        /// Type T cannot be cast automatically to the type of the destination array.
        /// </exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException("array");
            if (arrayIndex<array.GetLowerBound(0))
            {
                throw new ArgumentOutOfRangeException("arrayIndex", arrayIndex,
                    "arrayIndex must not be less then the lower bound of the array.");
            }
            try
            {
                DoCopyTo(array, arrayIndex, false);
            }
            catch (IndexOutOfRangeException e)
            {
                throw new ArgumentException("array is too small to fit the collection.", "array", e);
            }
        }

        /// <summary>
        /// Does the actual work of copying to array. Subclass is recommended to 
        /// override this method instead of <see cref="CopyTo(T[], int)"/> method, which 
        /// does all neccessary parameter checking and raises proper exception
        /// before calling this method.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the 
        /// destination of the elements copied from <see cref="ICollection{T}"/>. 
        /// The <see cref="Array"/> must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">
        /// The zero-based index in array at which copying begins.
        /// </param>
        /// <param name="ensureCapacity">
        /// If is <c>true</c>, calls <see cref="EnsureCapacity"/>
        /// </param>
        /// <returns>
        /// A new array of same runtime type as <paramref name="array"/> if 
        /// <paramref name="array"/> is too small to hold all elements and 
        /// <paramref name="ensureCapacity"/> is <c>false</c>. Otherwise
        /// the <paramref name="array"/> instance itself.
        /// </returns>
        protected virtual T[] DoCopyTo(T[] array, int arrayIndex, bool ensureCapacity)
        {
            if (ensureCapacity) array = EnsureCapacity(array, Count);
            foreach (T e in this) array[arrayIndex++] = e;
            return array;
        }

        /// <summary>
        /// Ensures the returned array has capacity specified by <paramref name="length"/>.
        /// </summary>
        /// <remarks>
        /// If <typeparamref name="T"/> is <see cref="object"/> but array is 
        /// actaully <c>string[]</c>, the returned array is always <c>string[]</c>.
        /// </remarks>
        /// <param name="array">
        /// The source array.
        /// </param>
        /// <param name="length">
        /// Expected length of array.
        /// </param>
        /// <returns>
        /// <paramref name="array"/> itself if <c>array.Length >= length</c>. 
        /// Otherwise a new array of same type of <paramref name="array"/> of given
        /// <paramref name="length"/>.
        /// </returns>
        protected static T[] EnsureCapacity(T[] array, int length)
        {
            if (array == null) return new T[length];
            if (array.Length >= length) return array;
            // new T[size] won't work here when targetArray is subtype of T.
            return (T[])Array.CreateInstance(array.GetType().GetElementType(), length);
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="ICollection{T}"/>.
        /// This implementation counts the elements by iterating through the 
        /// enumerator returned by <see cref="GetEnumerator()"/> method.
        /// </summary>
        /// 
        /// <returns>
        /// The number of elements contained in the <see cref="ICollection{T}"/>.
        /// </returns>
        /// 
        public virtual int Count
        {
            get
            {
                int count = 0;
                foreach (T item in this) count++;
                return count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="ICollection{T}"/> is read-only.
        /// This implementation always return true;
        /// </summary>
        /// 
        /// <returns>
        /// true if the <see cref="ICollection{T}"/> is read-only; otherwise, false.
        /// This implementation always return true;
        /// </returns>
        /// 
        public virtual bool IsReadOnly
        {
            get { return true; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="ICollection{T}"/>.
        /// This implementation always throw <see cref="NotSupportedException"/>.
        /// </summary>
        /// 
        /// <returns>
        /// true if item was successfully removed from the <see cref="ICollection{T}"/>; 
        /// otherwise, false. This method also returns false if item is not found in the 
        /// original <see cref="ICollection{T}"/>.
        /// </returns>
        /// 
        /// <param name="item">The object to remove from the <see cref="ICollection{T}"/>.</param>
        /// <exception cref="NotSupportedException">
        /// When the <see cref="ICollection{T}"/> is read-only. This implementation always 
        /// throw this exception.
        /// </exception>
        public virtual bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IEnumerable<T> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <remarks>
        /// Subclass must implement this method.
        /// </remarks>
        /// <returns>
        /// A <see cref="IEnumerator{T}"/> that can be used to iterate 
        /// through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public abstract IEnumerator<T> GetEnumerator();

        #endregion

        #region IEnumerable Members

        ///<summary>
        ///Returns an enumerator that iterates through a collection.
        ///</summary>
        ///
        ///<returns>
        ///An <see cref="T:System.Collections.IEnumerator"></see> 
        /// object that can be used to iterate through the collection.
        ///</returns>
        ///<filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region ICollection Members

        ///<summary>
        ///Copies the elements of the <see cref="T:System.Collections.ICollection"></see> 
        /// to an <see cref="T:System.Array"></see>, starting at a particular 
        /// <see cref="T:System.Array"></see> index.
        ///</summary>
        ///
        ///<param name="array">The one-dimensional <see cref="T:System.Array"></see> 
        /// that is the destination of the elements copied from 
        /// <see cref="T:System.Collections.ICollection"></see>. The 
        /// <see cref="T:System.Array"></see> must have zero-based indexing. </param>
        ///<param name="index">The zero-based index in array at which copying begins. </param>
        ///<exception cref="T:System.ArgumentNullException">array is null. </exception>
        ///<exception cref="T:System.ArgumentOutOfRangeException">index is less than zero. </exception>
        ///<exception cref="T:System.ArgumentException">
        /// array is multidimensional.-or- index is equal to or greater than 
        /// the length of array.-or- The number of elements in the source 
        /// <see cref="T:System.Collections.ICollection"></see> is greater 
        /// than the available space from index to the end of the destination 
        /// array. </exception>
        ///<exception cref="T:System.InvalidCastException">
        /// The type of the source <see cref="T:System.Collections.ICollection"></see> 
        /// cannot be cast automatically to the type of the destination array. </exception>
        /// <filterpriority>2</filterpriority>
        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null) throw new ArgumentNullException("array");
            if (index < array.GetLowerBound(0))
            {
                throw new ArgumentOutOfRangeException("index", index, 
                    "index must not be less then lower bound of the array");
            }
            try
            {
                CopyTo(array, index);
            }
            catch (RankException re)
            {
                throw new ArgumentException("array must not be multi-dimensional.", "array", re);
            }
            catch (IndexOutOfRangeException e)
            {
                throw new ArgumentException("array is too small to fit the collection.", "array", e);
            }
        }

        ///<summary>
        ///Gets a value indicating whether access to the 
        /// <see cref="T:System.Collections.ICollection"></see> 
        /// is synchronized (thread safe).
        ///</summary>
        ///
        ///<returns>
        ///true if access to the <see cref="T:System.Collections.ICollection"></see> 
        /// is synchronized (thread safe); otherwise, false.
        ///</returns>
        ///<filterpriority>2</filterpriority>
        bool ICollection.IsSynchronized
        {
            get { return IsSynchronized; }
        }

        ///<summary>
        ///Gets an object that can be used to synchronize access to the 
        /// <see cref="T:System.Collections.ICollection"></see>.
        ///</summary>
        ///
        ///<returns>
        ///An object that can be used to synchronize access to the 
        /// <see cref="T:System.Collections.ICollection"></see>.
        ///</returns>
        ///<filterpriority>2</filterpriority>
        object ICollection.SyncRoot
        {
            get { return SyncRoot; }
        }

        #endregion

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="object"/>.
        /// </summary>
        /// <remarks>
        /// This implmentation list out all the elements separated by comma.
        /// </remarks>
        /// <returns>
        /// A <see cref="string"/> that represents the current <see cref="object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(GetType().Name).Append("(");
            bool first = true;
            foreach (T e in this)
            {
                if (!first) sb.Append(", ");
                sb.Append(e);
                first = false;
            }
            return sb.Append(")").ToString();
        }

        /// <summary>
        /// Copies the elements of the <see cref="ICollection"/> to an 
        /// <see cref="Array"/>, starting at a particular <see cref="Array"/> 
        /// index.
        /// </summary>
        ///
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination 
        /// of the elements copied from <see cref="ICollection"/>. The 
        /// <see cref="Array"/> must have zero-based indexing. 
        /// </param>
        /// <param name="index">
        /// The zero-based index in array at which copying begins. 
        /// </param>
        /// <exception cref="ArgumentNullException">array is null. </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// index is less than zero. 
        /// </exception>
        /// <exception cref="ArgumentException">
        /// array is multidimensional.-or- index is equal to or greater than 
        /// the length of array.
        /// -or- 
        /// The number of elements in the source <see cref="ICollection"/> 
        /// is greater than the available space from index to the end of the 
        /// destination array. 
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// The type of the source <see cref="ICollection"/> cannot be cast 
        /// automatically to the type of the destination array. 
        /// </exception>
        /// <filterpriority>2</filterpriority>
        protected virtual void CopyTo(Array array, int index)
        {
            foreach (T e in this) array.SetValue(e, index++);
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the 
        /// <see cref="T:System.Collections.ICollection"></see>.
        /// </summary>
        /// <remarks>This implementation returns <see langword="null"/>.</remarks>
        /// <returns>
        /// An object that can be used to synchronize access to the 
        /// <see cref="T:System.Collections.ICollection"></see>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        protected virtual object SyncRoot
        {
            get { return null; }
        }

        /// <summary>
        /// Gets a value indicating whether access to the <see cref="ICollection"/> 
        /// is synchronized (thread safe).
        /// </summary>
        /// <remarks>This implementaiton always return <see langword="false"/>.</remarks>
        /// <returns>
        /// true if access to the <see cref="ICollection"/> 
        /// is synchronized (thread safe); otherwise, false.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        protected virtual bool IsSynchronized
        {
            get { return false; }
        }

        /// <summary> 
        /// Adds all of the elements in the supplied <paramref name="collection"/>
        /// to this collection.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Attempts to <see cref="AddRange"/> of a collection to 
        /// itself result in <see cref="ArgumentException"/>. Further, the 
        /// behavior of this operation is undefined if the specified
        /// collection is modified while the operation is in progress.
        /// </para>
        /// <para>
        /// This implementation iterates over the specified collection, and 
        /// adds each element returned by the iterator to this collection, in turn.
        /// An exception encountered while trying to add an element may result 
        /// in only some of the elements having been successfully added when 
        /// the associated exception is thrown.
        /// </para>
        /// </remarks>
        /// <param name="collection">
        /// The collection containing the elements to be added to this collection.
        /// </param>
        /// <returns>
        /// <c>true</c> if this collection is modified, else <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="collection"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If the collection is the current collection.
        /// </exception>
        public virtual bool AddRange(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            if (collection == this)
            {
                throw new ArgumentException("Cannot add to itself.", "collection");
            }
            return DoAddRange(collection);
        }

        /// <summary>
        /// Called by <see cref="AddRange"/> after the parameter is validated
        /// to be neither <c>null</c> nor this collection itself.
        /// </summary>
        /// <param name="collection">Collection of items to be added.</param>
        /// <returns>
        /// <c>true</c> if this collection is modified, else <c>false</c>.
        /// </returns>
        protected virtual bool DoAddRange(IEnumerable<T> collection)
        {
            bool modified = false;
            foreach (T element in collection)
            {
                Add(element);
                modified = true;
            }
            return modified;
        }
    }
}