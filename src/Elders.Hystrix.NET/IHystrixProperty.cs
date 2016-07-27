// Copyright 2012 Netflix, Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Elders.Hystrix.NET.Strategy.Properties;

namespace Elders.Hystrix.NET
{
    /// <summary>
    /// Provides an interface to represent property values which are used in
    /// <see cref="Elders.Hystrix.NET.IHystrixCommandProperties"/>, <see cref="IHystrixThreadPoolProperties"/>
    /// and <see cref="IHystrixCollapserProperties"/>. This way Hystrix can consume properties
    /// without being tied to any particular backing implementation. The actual implementation
    /// is decided by the current <see cref="IHystrixPropertiesStrategy"/>.
    /// </summary>
    /// <typeparam name="T">The type of property value.</typeparam>
    /// <seealso cref="IHystrixPropertiesStrategy"/>
    /// <seealso cref="IHystrixCommandProperties"/>
    /// <seealso cref="IHystrixThreadPoolProperties"/>
    /// <seealso cref="IHystrixCollapserProperties"/>
    /// <author>Loránd Biró</author>
    public interface IHystrixProperty<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        T Get();
    }
}
