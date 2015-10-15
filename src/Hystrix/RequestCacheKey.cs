namespace Netflix.Hystrix
{
    using Netflix.Hystrix.Strategy.Concurrency;

    internal class RequestCacheKey
    {
        private readonly short type; // used to differentiate between Collapser/Command if key is same between them
        private readonly string key;
        private readonly IHystrixConcurrencyStrategy concurrencyStrategy;

        public RequestCacheKey(HystrixCommandKey commandKey, IHystrixConcurrencyStrategy concurrencyStrategy)
        {
            type = 1;
            if (commandKey == null)
            {
                this.key = null;
            }
            else
            {
                this.key = commandKey.Name;
            }
            this.concurrencyStrategy = concurrencyStrategy;
        }

        public RequestCacheKey(IHystrixCollapserKey collapserKey, IHystrixConcurrencyStrategy concurrencyStrategy)
        {
            type = 2;
            if (collapserKey == null)
            {
                this.key = null;
            }
            else
            {
                this.key = collapserKey.Name;
            }
            this.concurrencyStrategy = concurrencyStrategy;
        }

        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + ((concurrencyStrategy == null) ? 0 : concurrencyStrategy.GetHashCode());
            result = prime * result + ((key == null) ? 0 : key.GetHashCode());
            result = prime * result + type;
            return result;
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;
            if (obj == null)
                return false;
            if (GetType() != obj.GetType())
                return false;
            RequestCacheKey other = (RequestCacheKey)obj;
            if (type != other.type)
                return false;
            if (key == null)
            {
                if (other.key != null)
                    return false;
            }
            else if (!key.Equals(other.key))
                return false;
            if (concurrencyStrategy == null)
            {
                if (other.concurrencyStrategy != null)
                    return false;
            }
            else if (!concurrencyStrategy.Equals(other.concurrencyStrategy))
                return false;
            return true;
        }
    }
}
