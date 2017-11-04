namespace WpfAnalyzers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.CodeAnalysis;

    internal sealed class PooledHashSet<T> : IDisposable
    {
        private static readonly ConcurrentQueue<PooledHashSet<T>> Cache = new ConcurrentQueue<PooledHashSet<T>>();
        private readonly HashSet<T> inner = new HashSet<T>();

        private int refCount;

        private PooledHashSet()
        {
        }

        public int Count => this.inner.Count;

        public bool Add(T item)
        {
            this.ThrowIfDisposed();
            return this.inner.Add(item);
        }

        public void UnionWith(IEnumerable<T> other)
        {
            this.inner.UnionWith(other);
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            this.inner.IntersectWith(other);
        }

        public void Dispose()
        {
            this.refCount--;
            Debug.Assert(this.refCount >= 0, "refCount>= 0");
            if (this.refCount == 0)
            {
                this.inner.Clear();
                Cache.Enqueue(this);
            }
        }

        internal static PooledHashSet<T> Borrow()
        {
            if (!Cache.TryDequeue(out var set))
            {
                set = new PooledHashSet<T>();
            }

            set.refCount = 1;
            return set;
        }

        internal static PooledHashSet<T> Borrow(PooledHashSet<T> set)
        {
            if (set == null)
            {
                return Borrow();
            }

            set.refCount++;
            return set;
        }

        internal bool TryGetSingle(out T result)
        {
            return this.inner.TryGetSingle(out result);
        }

        public bool TryGetFirst(out T result)
        {
            return this.inner.TryGetFirst(out result);
        }

        [Conditional("DEBUG")]
        private void ThrowIfDisposed()
        {
            if (this.refCount <= 0)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }
    }
}