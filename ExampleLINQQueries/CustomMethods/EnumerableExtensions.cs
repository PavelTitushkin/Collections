namespace ExampleLINQQueries.CustomMetods
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> LazyFilter<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            foreach (var item in source)
            {
                if (predicate(item))
                {
                    yield return item;
                }
            }
        }

        public static IEnumerable<TResult> LazyMap<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            foreach (var item in source)
            {
                yield return selector(item);
            }
        }

        public static IEnumerable<T> LazyOrderBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
            where TKey : IComparable<TKey>
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            var sortedList = MergeSort(new List<T>(source), keySelector);
            foreach (var item in sortedList)
            {
                yield return item;
            }
        }

        public static List<T> CustomToList<T>(this IEnumerable<T> source) 
        {
            var result = new List<T>();
            foreach (var item in source)
            {
                result.Add(item);
            }

            return result;
        }

        private static List<T> MergeSort<T, TKey>(List<T> list, Func<T, TKey> keySelector)
            where TKey : IComparable<TKey>
        {
            if (list.Count <= 1)
            {
                return list;
            }

            var middle = list.Count / 2;
            var left = list.GetRange(0, middle);
            var right = list.GetRange(middle, list.Count - middle);

            left = MergeSort(left, keySelector);
            right = MergeSort(right, keySelector);

            return Merge(left, right, keySelector);
        }

        private static List<T> Merge<T, TKey>(List<T> left, List<T> right, Func<T, TKey> keySelector)
            where TKey : IComparable<TKey>
        {
            var result = new List<T>();

            while (left.Count > 0 && right.Count > 0)
            {
                if (keySelector(left[0]).CompareTo(keySelector(right[0])) <= 0)
                {
                    result.Add(left[0]);
                    left.RemoveAt(0);
                }
                else
                {
                    result.Add(right[0]);
                    right.RemoveAt(0);
                }
            }

            result.AddRange(left);
            result.AddRange(right);

            return result;
        }
    }
}
