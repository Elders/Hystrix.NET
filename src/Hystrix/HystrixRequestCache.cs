namespace Netflix.Hystrix
{
    using System;
    using System.Collections.Concurrent;
    using Java.Util.Concurrent;
    using Netflix.Hystrix.Strategy.Concurrency;

    public class HystrixRequestCache
    {
        // the String key must be: HystrixRequestCache.prefix + concurrencyStrategy + cacheKey
        private readonly static ConcurrentDictionary<RequestCacheKey, HystrixRequestCache> caches = new ConcurrentDictionary<RequestCacheKey, HystrixRequestCache>();

        private readonly RequestCacheKey rcKey;
        private readonly IHystrixConcurrencyStrategy concurrencyStrategy;

        /**
         * A ConcurrentHashMap per 'prefix' and per request scope that is used to to dedupe requests in the same request.
         * <p>
         * Key => CommandPrefix + CacheKey : Future<?> from queue()
         */
        private static readonly HystrixRequestVariableHolder<ConcurrentDictionary<ValueCacheKey, object>> requestVariableForCache =
            new HystrixRequestVariableHolder<ConcurrentDictionary<ValueCacheKey, object>>(new DelegateRequestVariableLifecycle<ConcurrentDictionary<ValueCacheKey, object>>(() => new ConcurrentDictionary<ValueCacheKey, object>()));

        private HystrixRequestCache(RequestCacheKey rcKey, IHystrixConcurrencyStrategy concurrencyStrategy)
        {
            this.rcKey = rcKey;
            this.concurrencyStrategy = concurrencyStrategy;
        }

        public static HystrixRequestCache GetInstance(HystrixCommandKey key, IHystrixConcurrencyStrategy concurrencyStrategy)
        {
            return GetInstance(new RequestCacheKey(key, concurrencyStrategy), concurrencyStrategy);
        }

        public static HystrixRequestCache GetInstance(IHystrixCollapserKey key, IHystrixConcurrencyStrategy concurrencyStrategy)
        {
            return GetInstance(new RequestCacheKey(key, concurrencyStrategy), concurrencyStrategy);
        }

        private static HystrixRequestCache GetInstance(RequestCacheKey rcKey, IHystrixConcurrencyStrategy concurrencyStrategy)
        {
            return caches.GetOrAdd(rcKey, w =>
            {
                return new HystrixRequestCache(rcKey, concurrencyStrategy);
            });
        }

        /**
         * Retrieve a cached Future for this request scope if a matching command has already been executed/queued.
         * 
         * @return {@code Future<T>}
         */
        // suppressing warnings because we are using a raw Future since it's in a heterogeneous ConcurrentHashMap cache
        public IFuture<T> Get<T>(string cacheKey)
        {
            ValueCacheKey key = GetRequestCacheKey(cacheKey);
            if (key != null)
            {
                /* look for the stored value */
                object result = null;
                requestVariableForCache.Get(concurrencyStrategy).TryGetValue(key, out result);
                return (IFuture<T>)result;
            }
            return null;
        }



        /**
         * Put the Future in the cache if it does not already exist.
         * <p>
         * If this method returns a non-null value then another thread won the race and it should be returned instead of proceeding with execution of the new Future.
         * 
         * @param cacheKey
         *            key as defined by {@link HystrixCommand#getCacheKey()}
         * @param f
         *            Future to be cached
         * 
         * @return null if nothing else was in the cache (or this {@link HystrixCommand} does not have a cacheKey) or previous value if another thread beat us to adding to the cache
         */
        // suppressing warnings because we are using a raw Future since it's in a heterogeneous ConcurrentHashMap cache
        public IFuture<T> PutIfAbsent<T>(string cacheKey, IFuture<T> f)
        {
            ValueCacheKey key = GetRequestCacheKey(cacheKey);
            if (key != null)
            {
                IFuture<T> set = (IFuture<T>)requestVariableForCache.Get(concurrencyStrategy).GetOrAdd(key, f);
                if (set == f) // we win, this means the previos value was null
                {
                    return null;
                }
                else
                {
                    return set;
                }
            }
            // we either set it in the cache or do not have a cache key
            return null;
        }

        /**
         * Clear the cache for a given cacheKey.
         * 
         * @param cacheKey
         *            key as defined by {@link HystrixCommand#getCacheKey()}
         */
        public void Clear(String cacheKey)
        {
            ValueCacheKey key = GetRequestCacheKey(cacheKey);
            if (key != null)
            {
                /* remove this cache key */
                object future;
                requestVariableForCache.Get(concurrencyStrategy).TryRemove(key, out future);
            }
        }

        /**
         * Request CacheKey: HystrixRequestCache.prefix + concurrencyStrategy + HystrixCommand.getCacheKey (as injected via get/put to this class)
         * <p>
         * We prefix with {@link HystrixCommandKey} or {@link HystrixCollapserKey} since the cache is heterogeneous and we don't want to accidentally return cached Futures from different
         * types.
         * 
         * @return ValueCacheKey
         */
        private ValueCacheKey GetRequestCacheKey(string cacheKey)
        {
            if (cacheKey != null)
            {
                /* create the cache key we will use to retrieve/store that include the type key prefix */
                return new ValueCacheKey(rcKey, cacheKey);
            }
            return null;
        }
    }
}
