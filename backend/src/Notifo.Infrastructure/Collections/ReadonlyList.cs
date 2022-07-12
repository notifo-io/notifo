// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure.Collections
{
    public static class ReadonlyList
    {
        private static class Empties<T>
        {
#pragma warning disable SA1401 // Fields should be private
            public static readonly ReadonlyList<T> Instance = new ReadonlyList<T>();
#pragma warning restore SA1401 // Fields should be private
        }

        public static ReadonlyList<T> Empty<T>()
        {
            return Empties<T>.Instance;
        }

        public static ReadonlyList<T> Create<T>(params T[]? items)
        {
            if (items == null || items.Length == 0)
            {
                return Empty<T>();
            }

            return new ReadonlyList<T>(items.ToList());
        }

        public static ReadonlyList<T> ToReadonlyList<T>(this IEnumerable<T> source)
        {
            var inner = new List<T>(source);

            if (inner.Count == 0)
            {
                return Empty<T>();
            }

            return new ReadonlyList<T>(inner);
        }

        public static List<T> ToMutable<T>(this ReadonlyList<T>? source)
        {
            if (source == null)
            {
                return new List<T>();
            }

            return new List<T>(source);
        }

        public static ReadonlyList<T> Set<T>(this ReadonlyList<T>? source, T item)
        {
            var mutable = source.ToMutable();

            mutable.Add(item);

            return mutable.ToReadonlyList();
        }

        public static ReadonlyList<T> Remove<T>(this ReadonlyList<T>? source, T item)
        {
            var mutable = source.ToMutable();

            mutable.Remove(item);

            return mutable.ToReadonlyList();
        }

        public static ReadonlyList<T> RemoveAll<T>(this ReadonlyList<T>? source, Predicate<T> match)
        {
            var mutable = source.ToMutable();

            mutable.RemoveAll(match);

            return mutable.ToReadonlyList();
        }
    }
}
