using System;
using System.Collections.Generic;
using System.Threading;

namespace Open.LoggingService.Utils
{
    internal class BlockingPool<T>
    {
        private readonly Func<T> _factory;
        private readonly Queue<T> _pool;
        private static ReaderWriterLock rwl = new ReaderWriterLock();

        public BlockingPool(Func<T> factory)
        {
            _factory = factory;
            _pool = new Queue<T>();
        }

        public int Count
        {
            get { return _pool.Count; }
        }

        public void Add(T item)
        {
            Guard.NotNull(item, "item");

            rwl.AcquireWriterLock(50);
            try
            {
                _pool.Enqueue(item);
            }
            finally
            {
                rwl.ReleaseWriterLock();
            }
        }

        public T Take()
        {
            rwl.AcquireReaderLock(100);
            try
            {
                return _pool.Count > 0 ? _pool.Dequeue() : _factory();
            }
            finally
            {
                rwl.ReleaseReaderLock();
            }
        }
    }
}

