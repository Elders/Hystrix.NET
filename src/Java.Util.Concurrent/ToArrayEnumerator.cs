namespace Java.Util.Concurrent
{
    internal class ToArrayEnumerator<T> : AbstractEnumerator<T> // NET_ONLY
    {
        private readonly AbstractQueue<T> _queue;
        private T[] _array;
        private int _cursor=-1;

        public ToArrayEnumerator(AbstractQueue<T> queue)
        {
            _queue = queue;
            Initialize();
        }

        private void Initialize()
        {
            _array = _queue.ToArray();
            _cursor = -1;
        }

        protected override bool GoNext()
        {
            return ++_cursor < _array.Length;
        }

        protected override T FetchCurrent()
        {
            return _array[_cursor];
        }

        protected override void DoReset()
        {
            Initialize();
        }
    }
}