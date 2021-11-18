﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure
{
    public static class ResultList
    {
        private sealed class Impl<T> : List<T>, IResultList<T>
        {
            public long Total { get; }

            public Impl(IEnumerable<T> items, long total)
                : base(items)
            {
                Total = total;
            }
        }

        public static IResultList<T> Create<T>(long total, IEnumerable<T> items)
        {
            return new Impl<T>(items, total);
        }

        public static IResultList<T> CreateFrom<T>(long total, params T[] items)
        {
            return new Impl<T>(items, total);
        }
    }
}
