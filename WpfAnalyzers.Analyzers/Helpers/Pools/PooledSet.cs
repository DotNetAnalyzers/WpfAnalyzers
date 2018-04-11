namespace WpfAnalyzers
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;

    internal sealed class PooledSet<T> : IDisposable, IReadOnlyCollection<T>
    {
        private static readonly ConcurrentQueue<PooledSet<T>> Cache = new ConcurrentQueue<PooledSet<T>>();
        private readonly HashSet<T> inner = new HashSet<T>();

        private int refCount;

        private PooledSet()
        {
        }

        public int Count => this.inner.Count;

        private HashSet<T> Inner
        {
            get
            {
                this.ThrowIfDisposed();
                return this.inner;
            }
        }

        public bool Add(T item) => this.Inner.Add(item);

        public void UnionWith(IEnumerable<T> other) => this.Inner.UnionWith(other);

        public void IntersectWith(IEnumerable<T> other) => this.Inner.IntersectWith(other);

        public IEnumerator<T> GetEnumerator() => this.Inner.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this.Inner).GetEnumerator();

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

        internal static PooledSet<T> Borrow()
        {
            if (!Cache.TryDequeue(out var set))
            {
                set = new PooledSet<T>();
            }

            set.refCount = 1;
            return set;
        }

        internal static PooledSet<T> Borrow(PooledSet<T> set)
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
            return this.Inner.TryGetSingle(out result);
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
