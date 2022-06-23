// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure.Collections
{
    public static class ReadonlyDictionary
    {
        private static class Empties<TKey, TValue> where TKey : notnull
        {
            public static readonly ReadonlyDictionary<TKey, TValue> Instance = new ReadonlyDictionary<TKey, TValue>();
        }

        public static ReadonlyDictionary<TKey, TValue> Empty<TKey, TValue>() where TKey : notnull
        {
            return Empties<TKey, TValue>.Instance;
        }

        public static ReadonlyDictionary<TKey, TValue> ToReadonlyDictionary<TKey, TValue>(this Dictionary<TKey, TValue> source) where TKey : notnull
        {
            if (source.Count == 0)
            {
                return Empty<TKey, TValue>();
            }

            return new ReadonlyDictionary<TKey, TValue>(source);
        }

        public static ReadonlyDictionary<TKey, TValue> ToReadonlyDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source) where TKey : notnull
        {
            return new ReadonlyDictionary<TKey, TValue>(new Dictionary<TKey, TValue>(source));
        }

        public static ReadonlyDictionary<TKey, TValue> ToReadonlyDictionary<TKey, TValue>(this IEnumerable<TValue> source,
            Func<TValue, TKey> keySelector) where TKey : notnull
        {
            var inner = source.ToDictionary(keySelector);

            if (inner.Count == 0)
            {
                return Empty<TKey, TValue>();
            }

            return new ReadonlyDictionary<TKey, TValue>(inner);
        }

        public static ReadonlyDictionary<TKey, TValue> ToReadonlyDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TValue> elementSelector) where TKey : notnull
        {
            var inner = source.ToDictionary(keySelector, elementSelector);

            if (inner.Count == 0)
            {
                return Empty<TKey, TValue>();
            }

            return new ReadonlyDictionary<TKey, TValue>(inner);
        }

        public static Dictionary<TKey, TValue> ToMutable<TKey, TValue>(this ReadonlyDictionary<TKey, TValue>? source) where TKey : notnull
        {
            if (source == null)
            {
                return new Dictionary<TKey, TValue>();
            }

            return new Dictionary<TKey, TValue>(source);
        }

        public static ReadonlyDictionary<TKey, TValue> Set<TKey, TValue>(this ReadonlyDictionary<TKey, TValue>? source, TKey key, TValue value) where TKey : notnull
        {
            var mutable = source.ToMutable();

            mutable[key] = value;

            return mutable.ToReadonlyDictionary();
        }

        public static ReadonlyDictionary<TKey, TValue> Remove<TKey, TValue>(this ReadonlyDictionary<TKey, TValue>? source, TKey key) where TKey : notnull
        {
            var mutable = source.ToMutable();

            mutable.Remove(key);

            return mutable.ToReadonlyDictionary();
        }
    }
}
