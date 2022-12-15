// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Fluid;
using Notifo.Infrastructure;
using Squidex.Caching;

#pragma warning disable MA0048 // File name must match type name
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Notifo.Domain.Utils;

public readonly record struct CachedTemplate(IFluidTemplate? Template, TemplateError? Error);

public static class LiquidCache
{
    private static readonly LRUCache<string, CachedTemplate> Cache = new LRUCache<string, CachedTemplate>(1000);
    private static readonly FluidParser Parser = new FluidParser();
    private static readonly ReaderWriterLockSlim LockObject = new ReaderWriterLockSlim();

    public static CachedTemplate Parse(string input, bool bypass = false)
    {
        Guard.NotNull(input);

        CachedTemplate result;

        if (!bypass)
        {
            LockObject.EnterWriteLock();
            try
            {
                if (Cache.TryGetValue(input, out result))
                {
                    return result;
                }
            }
            finally
            {
                LockObject.ExitWriteLock();
            }
        }

        Parser.TryParse(input, out var template, out var errorMessage);

        var error = LiquidErrorParser.Parse(errorMessage);

        result = new CachedTemplate(template, error);

        if (!bypass)
        {
            LockObject.EnterWriteLock();
            try
            {
                Cache.Set(input, result);
            }
            finally
            {
                LockObject.ExitWriteLock();
            }
        }

        return result;
    }
}
