using System.Collections.Generic;

namespace System.Linq
{
    static class Enumerable
    {
        public static bool Any<TSource>(IEnumerable<TSource> source)
        {
            if (source == null)
                throw ThrowHelper.ArgumentNull("source");
            using (IEnumerator<TSource> enumerator = source.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public static TSource First<TSource>(IEnumerable<TSource> source)
        {
            if (source == null)
                throw ThrowHelper.ArgumentNull("source");
            IList<TSource> list = source as IList<TSource>;
            if (list == null)
            {
                using (IEnumerator<TSource> enumerator = source.GetEnumerator())
                {
                    if (!enumerator.MoveNext())
                    {
                        throw ThrowHelper.NoElements();
                    }
                    else
                    {
                        return enumerator.Current;
                    }
                }
            }
            else if (list.Count > 0)
            {
                return list[0];
            }
            throw ThrowHelper.NoElements();
        }

        public static TSource FirstOrDefault<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
                throw ThrowHelper.ArgumentNull("source");
            if (predicate == null)
                throw ThrowHelper.ArgumentNull("predicate");
            foreach (TSource item in source)
            {
                if (predicate(item))
                    return item;
            }
            return default(TSource);
        }

        public static IEnumerable<TSource> OrderBy<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            TSource[] array = new List<TSource>(source).ToArray();
            Array.Sort(array, delegate(TSource t1, TSource t2)
            {
                return Comparer<TKey>.Default.Compare(keySelector(t1), keySelector(t2));
            });
            return array;
        }

        public static IEnumerable<TSource> OrderByThenBy<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TKey> keySelectorThen)
        {
            TSource[] array = new List<TSource>(source).ToArray();
            Array.Sort(array, delegate(TSource t1, TSource t2)
            {
                Int32 ret = Comparer<TKey>.Default.Compare(keySelector(t1), keySelector(t2));
                if (ret == 0)
                    ret = Comparer<TKey>.Default.Compare(keySelectorThen(t1), keySelectorThen(t2));
                return ret;
            });
            return array;
        }

        public static IEnumerable<int> Range(int start, int count)
        {
            long num = (long)start + (long)count - (long)1;
            if (count < 0 || num > (long)2147483647)
                throw ThrowHelper.ArgumentOutOfRange("count");

            for (int i = 0; i < count; i++)
            {
                yield return start + i;
            }
        }

        public static IEnumerable<TResult> Select<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            if (source == null)
                throw ThrowHelper.ArgumentNull("source");
            if (selector == null)
                throw ThrowHelper.ArgumentNull("selector");
            foreach (TSource item in source)
            {
                yield return selector(item);
            }
        }

        public static IEnumerable<TResult> Select<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, int, TResult> selector)
        {
            if (source == null)
                throw ThrowHelper.ArgumentNull("source");
            if (selector == null)
                throw ThrowHelper.ArgumentNull("selector");
            return Enumerable.SelectIterator<TSource, TResult>(source, selector);
        }

        private static IEnumerable<TResult> SelectIterator<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, int, TResult> selector)
        {
            int num = -1;
            foreach (TSource tSource in source)
            {
                num++;
                yield return selector(tSource, num);
            }
        }

        public static IEnumerable<TSource> Skip<TSource>(IEnumerable<TSource> source, int count)
        {
            if (source == null)
                throw ThrowHelper.ArgumentNull("source");
            return Enumerable.SkipIterator<TSource>(source, count);
        }

        private static IEnumerable<TSource> SkipIterator<TSource>(IEnumerable<TSource> source, int count)
        {
            using (IEnumerator<TSource> enumerator = source.GetEnumerator())
            {
                while (count > 0 && enumerator.MoveNext())
                {
                    count--;
                }
                if (count > 0)
                {
                    yield break;
                }
                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current;
                }
            }
        }

        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            return ToDictionary<TSource, TKey, TElement>(source, keySelector, elementSelector, null);
        }

        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            if (source == null)
                throw ThrowHelper.ArgumentNull("source");
            if (keySelector == null)
                throw ThrowHelper.ArgumentNull("keySelector");
            if (elementSelector == null)
                throw ThrowHelper.ArgumentNull("elementSelector");
            Dictionary<TKey, TElement> tKeys = new Dictionary<TKey, TElement>(comparer);
            foreach (TSource tSource in source)
            {
                tKeys.Add(keySelector(tSource), elementSelector(tSource));
            }
            return tKeys;
        }

        public static List<TSource> ToList<TSource>(IEnumerable<TSource> source)
        {
            if (source == null)
                throw ThrowHelper.ArgumentNull("source");
            return new List<TSource>(source);
        }

        public static TSource[] ToArray<TSource>(IEnumerable<TSource> source)
        {
            if (source == null)
                throw ThrowHelper.ArgumentNull("source");
            TSource[] array = source as TSource[];
            if (array != null)
                return array;
            return new List<TSource>(source).ToArray();
        }

        public static IEnumerable<TSource> Where<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
                throw ThrowHelper.ArgumentNull("source");
            if (predicate == null)
                throw ThrowHelper.ArgumentNull("predicate");
            foreach (TSource item in source)
            {
                if (predicate(item))
                    yield return item;
            }
        }
    }
}
