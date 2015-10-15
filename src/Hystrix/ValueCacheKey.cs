namespace Netflix.Hystrix
{
    using System;

    internal class ValueCacheKey
    {
        private readonly RequestCacheKey rvKey;
        private readonly string valueCacheKey;

        public ValueCacheKey(RequestCacheKey rvKey, String valueCacheKey)
        {
            this.rvKey = rvKey;
            this.valueCacheKey = valueCacheKey;
        }

        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + ((rvKey == null) ? 0 : rvKey.GetHashCode());
            result = prime * result + ((valueCacheKey == null) ? 0 : valueCacheKey.GetHashCode());
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
            ValueCacheKey other = (ValueCacheKey)obj;
            if (rvKey == null)
            {
                if (other.rvKey != null)
                    return false;
            }
            else if (!rvKey.Equals(other.rvKey))
                return false;
            if (valueCacheKey == null)
            {
                if (other.valueCacheKey != null)
                    return false;
            }
            else if (!valueCacheKey.Equals(other.valueCacheKey))
                return false;
            return true;
        }
    }
}
