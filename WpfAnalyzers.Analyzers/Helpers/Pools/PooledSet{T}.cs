namespace WpfAnalyzers
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;

    [DebuggerTypeProxy(typeof(PooledSetDebugView<>))]
    [DebuggerDisplay("Count = {this.Count}, refCount = {this.refCount}")]
    internal sealed class PooledSet<T> : IDisposable, IReadOnlyCollection<T>
    {
        private static readonly ConcurrentQueue<PooledSet<T>> Cache = new ConcurrentQueue<PooledSet<T>>();
        private readonly HashSet<T> inner = new HashSet<T>();

        private int refCount;

        private PooledSet()
        {
        }

        public int Count => this.inner.Count;

        public bool Add(T item)
        {
            this.ThrowIfDisposed();
            return this.inner.Add(item);
        }

        public void UnionWith(IEnumerable<T> other) => this.inner.UnionWith(other);

        public void IntersectWith(IEnumerable<T> other) => this.inner.IntersectWith(other);

        public IEnumerator<T> GetEnumerator() => this.inner.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        void IDisposable.Dispose()
        {
            if (Interlocked.Decrement(ref this.refCount) == 0)
            {
                Debug.Assert(!Cache.Contains(this), "!Cache.Contains(this)");
                this.inner.Clear();
                Cache.Enqueue(this);
            }
        }

        /// <summary>
        /// The result from this call is meant to be used in a using.
        /// </summary>
        public static PooledSet<T> Borrow()
        {
            if (Cache.TryDequeue(out var set))
            {
                Debug.Assert(set.refCount == 0, $"{nameof(Borrow)} set.refCount == {set.refCount}");
                set.refCount = 1;
                return set;
            }

            return new PooledSet<T> { refCount = 1 };
        }

        /// <summary>
        /// The result from this call is meant to be used in a using.
        /// </summary>
        internal static PooledSet<T> BorrowOrIncrementUsage(PooledSet<T> set)
        {
            if (set == null)
            {
                return Borrow();
            }

            var current = Interlocked.Increment(ref set.refCount);
            Debug.Assert(current >= 1, $"{nameof(BorrowOrIncrementUsage)} set.refCount == {current}");
            return set;
        }

        [Conditional("DEBUG")]
        private void ThrowIfDisposed()
        {
            if (this.refCount <= 0)
            {
                Debug.Assert(this.refCount == 0, $"{nameof(this.ThrowIfDisposed)} set.refCount == {this.refCount}");
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }
    }
}
