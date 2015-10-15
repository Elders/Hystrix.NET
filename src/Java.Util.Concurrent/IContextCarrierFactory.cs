#region License
/*
* Copyright (C) 2002-2009 the original author or authors.
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

namespace Java.Util.Concurrent
{
    /// <summary>
    /// A context carrier factory creates instances of 
    /// <see cref="IContextCarrier"/>.
    /// </summary>
    /// <author>Kenneth Xu</author>
    public interface IContextCarrierFactory //NET_ONLY
    {
        /// <summary>
        /// Create a new instance of <see cref="IContextCarrier"/>
        /// </summary>
        /// <returns>A new instance of <see cref="IContextCarrier"/>.</returns>
        IContextCarrier CreateContextCarrier();
    }
}