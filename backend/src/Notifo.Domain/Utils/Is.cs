// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.CodeAnalysis;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Texts;

namespace Notifo.Domain.Utils
{
    public static class Is
    {
        public static bool Changed([NotNullWhen(true)] string? value, string? other)
        {
            return value != null && !string.Equals(value, other, StringComparison.Ordinal);
        }

        public static bool Changed<T>([NotNullWhen(true)] T[]? value, IReadOnlyList<T>? other)
        {
            return value?.Length > 0 && !value.EqualsList(other);
        }

        public static bool Changed([NotNullWhen(true)] LocalizedText? value, LocalizedText? other)
        {
            return value != null && !value.Equals(other);
        }

        public static bool Changed<T>([NotNullWhen(true)] T? value, T other) where T : struct
        {
            return value != null && !Equals(value, other);
        }

        public static bool Changed<T>([NotNullWhen(true)] T? value, T other) where T : class
        {
            return value != null && !Equals(value, other);
        }

        public static bool Changed<T>([NotNullWhen(true)] T? value, T? other) where T : struct
        {
            return !Equals(value, other);
        }
    }
}
