using System.Collections.Generic;
using System.Threading;

namespace Open.LoggingService.Utils
{
    class BlockingQueue<T>
    {
        private readonly Queue<T> _queue = new Queue<T>();
        private readonly object _queueLock = new object();
        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(false);

        public T Take()
        {
            lock (_queueLock)
            {
                if (_queue.Count > 0)
                    return _queue.Dequeue();
            }
            _resetEvent.WaitOne();

            return Take();
        }

        public void Add(T obj)
        {
            lock (_queueLock)
            {
                if (_queue.Count > 4 * 1024) return;
                _queue.Enqueue(obj);
                _resetEvent.Set();
            }
        }

        public int Count
        {
            get
            {
                lock (_queueLock)
                {
                    return _queue.Count;
                }
            }
        }
    }
}