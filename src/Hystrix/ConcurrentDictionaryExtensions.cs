namespace Netflix.Hystrix
{
    using System;
    using System.Collections.Concurrent;

    internal static class ConcurrentDictionaryExtensions
    {
        public static TValue GetOrDefault<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value = default(TValue);
            dictionary.TryGetValue(key, out value);
            return value;
        }

        public static TValue GetOrAdd<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> valueFactory, Action<TValue> onValueCreated)
        {
            // attempt to retrieve from dictionary first
            TValue value;
            if (dictionary.TryGetValue(key, out value))
            {
                return value;
            }

            // it doesn't exist so we need to create it
            value = valueFactory(key);
            // attempt to store it (race other threads)
            if (dictionary.TryAdd(key, value))
            {
                // we won the thread-race to store the instance we created so initialize it
                onValueCreated(value);
                // done registering, return instance that got cached
                return value;
            }
            else
            {
                // we lost so return existing and let the one we created be garbage collected
                // without calling onValueCreated() on it
                return dictionary[key];
            }
        }
    }
}
