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

namespace Netflix.Hystrix.Strategy.Properties
{
    using System;
    using Netflix.Hystrix.ThreadPool;

    /// <summary>
    /// Interface providing factory methods for properties used by various components of Hystrix.
    /// </summary>
    public interface IHystrixPropertiesStrategy
    {
        IHystrixCommandProperties GetCommandProperties(HystrixCommandKey commandKey, HystrixCommandPropertiesSetter setter);
        string GetCommandPropertiesCacheKey(HystrixCommandKey commandKey, HystrixCommandPropertiesSetter setter);

        IHystrixThreadPoolProperties GetThreadPoolProperties(HystrixThreadPoolKey threadPoolKey, HystrixThreadPoolPropertiesSetter setter);
        string GetThreadPoolPropertiesCacheKey(HystrixThreadPoolKey threadPoolKey, HystrixThreadPoolPropertiesSetter setter);

        IHystrixCollapserProperties GetCollapserProperties(IHystrixCollapserKey collapserKey, HystrixCollapserPropertiesSetter setter);
        string GetCollapserPropertiesCacheKey(IHystrixCollapserKey collapserKey, HystrixCollapserPropertiesSetter setter);
    }
}
