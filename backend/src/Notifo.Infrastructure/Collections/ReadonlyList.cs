// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;

namespace Notifo.Infrastructure.Collections
{
    public static class ReadonlyList
    {
        private static class Empties<T>
        {
#pragma warning disable SA1401 // Fields should be private
            public static ReadonlyList<T> Instance = new ReadonlyList<T>();
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
    }
}
