// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure;

public static class ResultList
{
    private sealed class Impl<T> : List<T>, IResultList<T>
    {
        public static readonly Impl<T> EmptyInstance = new Impl<T>(Enumerable.Empty<T>(), 0);

        public long Total { get; }

        public Impl(IEnumerable<T> items, long total)
            : base(items)
        {
            Total = total;
        }

        public Impl(IEnumerable<T> items)
            : base(items)
        {
            Total = Count;
        }
    }

    public static IResultList<T> Create<T>(long total, IEnumerable<T> items)
    {
        return new Impl<T>(items, total);
    }

    public static IResultList<T> Create<T>(IEnumerable<T> items)
    {
        return new Impl<T>(items);
    }

    public static IResultList<T> CreateFrom<T>(long total, params T[] items)
    {
        return new Impl<T>(items, total);
    }

    public static IResultList<T> Empty<T>()
    {
        return Impl<T>.EmptyInstance;
    }
}
