namespace WpfAnalyzers
{
    using System;
    using System.Collections.Generic;

    internal static class EnumerableExt
    {
        internal static bool TryGetAtIndex<TCollection, TItem>(this TCollection source, int index, out TItem result)
            where TCollection : IReadOnlyList<TItem>
        {
            result = default(TItem);
            if (source == null)
            {
                return false;
            }

            if (source.Count <= index)
            {
                return false;
            }

            result = source[index];
            return true;
        }

        internal static bool TryGetSingle<TCollection, TItem>(this TCollection source, out TItem result)
            where TCollection : IReadOnlyList<TItem>
        {
            if (source.Count == 1)
            {
                result = source[0];
                return true;
            }

            result = default(TItem);
            return false;
        }

        internal static bool TryGetSingle<TCollection, TItem>(this TCollection source, Func<TItem, bool> selector, out TItem result)
            where TCollection : IReadOnlyList<TItem>
        {
            foreach (var item in source)
            {
                if (selector(item))
                {
                    result = item;
                    return true;
                }
            }

            result = default(TItem);
            return false;
        }

        internal static bool TryGetFirst<TCollection, TItem>(this TCollection source, out TItem result)
            where TCollection : IReadOnlyList<TItem>
        {
            if (source.Count == 0)
            {
                result = default(TItem);
                return false;
            }

            result = source[0];
            return true;
        }

        internal static bool TryGetFirst<TCollection, TItem>(this TCollection source, Func<TItem, bool> selector, out TItem result)
            where TCollection : IReadOnlyList<TItem>
        {
            foreach (var item in source)
            {
                if (selector(item))
                {
                    result = item;
                    return true;
                }
            }

            result = default(TItem);
            return false;
        }

        internal static bool TryGetLast<TCollection, TItem>(this TCollection source, out TItem result)
            where TCollection : IReadOnlyList<TItem>
        {
            if (source.Count == 0)
            {
                result = default(TItem);
                return false;
            }

            result = source[source.Count - 1];
            return true;
        }

        internal static bool TryGetLast<TCollection, TItem>(this TCollection source, Func<TItem, bool> selector, out TItem result)
     where TCollection : IReadOnlyList<TItem>
        {
            for (var i = source.Count - 1; i >= 0; i--)
            {
                var item = source[i];
                if (selector(item))
                {
                    result = item;
                    return true;
                }
            }

            result = default(TItem);
            return false;
        }
    }
}
