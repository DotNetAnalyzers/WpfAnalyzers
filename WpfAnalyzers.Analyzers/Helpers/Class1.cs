namespace WpfAnalyzers
{
    using System;
    using System.Collections.Generic;

    internal static partial class EnumerableExt
    {
        internal static bool TryGetAtIndex<T>(this IEnumerable<T> source, int index, out T result)
        {
            result = default(T);
            if (source == null)
            {
                return false;
            }

            var current = 0;
            using (var e = source.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    if (current == index)
                    {
                        result = e.Current;
                        return true;
                    }

                    current++;
                }
            }

            return false;
        }

        internal static bool TryGetSingle<T>(this IEnumerable<T> source, out T result)
        {
            result = default(T);
            if (source == null)
            {
                return false;
            }

            using (var e = source.GetEnumerator())
            {
                if (e.MoveNext())
                {
                    if (!e.MoveNext())
                    {
                        result = e.Current;
                        return true;
                    }

                    return false;
                }

                result = default(T);
                return false;
            }
        }

        internal static bool TryGetSingle<T>(this IEnumerable<T> source, Func<T, bool> selector, out T result)
        {
            result = default(T);
            if (source == null)
            {
                return false;
            }

            using (var e = source.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    result = e.Current;
                    if (selector(result))
                    {
                        while (e.MoveNext())
                        {
                            if (selector(e.Current))
                            {
                                result = default(T);
                                return false;
                            }
                        }

                        return true;
                    }
                }
            }

            result = default(T);
            return false;
        }

        internal static bool TryGetFirst<T>(this IEnumerable<T> source, out T result)
        {
            result = default(T);
            if (source == null)
            {
                return false;
            }

            using (var e = source.GetEnumerator())
            {
                if (e.MoveNext())
                {
                    result = e.Current;
                    return true;
                }

                return false;
            }
        }

        internal static bool TryGetFirst<T>(this IEnumerable<T> source, Func<T, bool> selector, out T result)
        {
            if (source == null)
            {
                result = default(T);
                return false;
            }

            using (var e = source.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    result = e.Current;
                    if (selector(result))
                    {
                        return true;
                    }
                }
            }

            result = default(T);
            return false;
        }
    }
}
