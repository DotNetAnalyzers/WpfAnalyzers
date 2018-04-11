namespace WpfAnalyzers
{
    using System;
    using System.Diagnostics;
    using System.Linq;

    internal class PooledSetDebugView<T>
    {
        private readonly PooledSet<T> set;

        public PooledSetDebugView(PooledSet<T> set)
        {
            this.set = set ?? throw new ArgumentNullException(nameof(set));
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items => this.set.ToArray();
    }
}
