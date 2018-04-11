namespace WpfAnalyzers
{
    using System;
    using System.Collections.Concurrent;
    using System.Text;

    /// <summary>
    /// A pooled <see cref="StringBuilder"/>
    /// </summary>
    internal static class StringBuilderPool
    {
        private static readonly ConcurrentQueue<PooledStringBuilder> Cache = new ConcurrentQueue<PooledStringBuilder>();

        /// <summary>
        /// Borrow an instance.
        /// </summary>
        /// <returns>A <see cref="PooledStringBuilder"/></returns>
        internal static PooledStringBuilder Borrow()
        {
            if (Cache.TryDequeue(out var item))
            {
                return item;
            }

            return new PooledStringBuilder();
        }

        internal class PooledStringBuilder
        {
            private readonly StringBuilder inner = new StringBuilder();

            /// <summary>
            /// Adds a line of text to the inner <see cref="StringBuilder"/>.
            /// </summary>
            /// <param name="text">The text</param>
            /// <returns>This instance.</returns>
            public PooledStringBuilder AppendLine(string text)
            {
                this.inner.AppendLine(text);
                return this;
            }

            /// <summary>
            /// Adds an empty of text to the inner <see cref="StringBuilder"/>.
            /// </summary>
            /// <returns>This instance.</returns>
            public PooledStringBuilder AppendLine()
            {
                this.inner.AppendLine();
                return this;
            }

            /// <inheritdoc/>
            [Obsolete("Use Return", true)]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
            public override string ToString() => throw new InvalidOperationException("Use StringBuilderPool.Return");
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member

            /// <summary>
            /// Get the string and return this instance to the pool.
            /// </summary>
            /// <returns>The text.</returns>
            public string Return()
            {
                var text = this.inner.ToString();
                this.inner.Clear();
                Cache.Enqueue(this);
                return text;
            }
        }
    }
}
