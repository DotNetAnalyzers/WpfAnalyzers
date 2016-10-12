namespace WpfAnalyzers
{
    using System.Collections.Generic;
    using System.Collections.Immutable;

    internal static class EnumerableExt
    {
        internal static bool TryGetSingle<T>(this IEnumerable<T> source, out T result)
        {
            result = default(T);
            if (source == null)
            {
                return false;
            }

            bool foundOne = false;
            foreach (var item in source)
            {
                if (foundOne)
                {
                    return false;
                }

                result = item;
                foundOne = true;
            }

            return foundOne;
        }

        internal static bool TryGetSingle<T>(this IReadOnlyList<T> source, out T result)
        {
            if (source.Count == 1)
            {
                result = source[0];
                return true;
            }

            result = default(T);
            return false;
        }

        internal static bool TryGetFirst<T>(this IReadOnlyList<T> source, out T result)
        {
            if (source.Count == 0)
            {
                result = default(T);
                return false;
            }

            result = source[0];
            return true;
        }

        internal static bool TryGetLast<T>(this IReadOnlyList<T> source, out T result)
        {
            if (source.Count == 0)
            {
                result = default(T);
                return false;
            }

            result = source[source.Count - 1];
            return true;
        }

        internal static bool TryGetSingle<T>(this ImmutableArray<T> source, out T result)
        {
            if (source.Length == 1)
            {
                result = source[0];
                return true;
            }

            result = default(T);
            return false;
        }

        internal static bool TryGetFirst<T>(this ImmutableArray<T> source, out T result)
        {
            if (source.Length == 0)
            {
                result = default(T);
                return false;
            }

            result = source[0];
            return true;
        }

        internal static bool TryGetLast<T>(this ImmutableArray<T> source, out T result)
        {
            if (source.Length == 0)
            {
                result = default(T);
                return false;
            }

            result = source[source.Length - 1];
            return true;
        }
    }
}
