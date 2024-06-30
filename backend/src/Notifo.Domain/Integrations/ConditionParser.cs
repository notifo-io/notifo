// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Esprima.Ast;
using Jint;
using Microsoft.Extensions.Caching.Memory;
using Options = Microsoft.Extensions.Options.Options;

namespace Notifo.Domain.Integrations;

internal static class ConditionParser
{
    private static readonly ScriptPreparationOptions PreparationOptions = new ScriptPreparationOptions
    {
        ParsingOptions = new ScriptParsingOptions
        {
            Tolerant = true
        }
    };

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
    private static readonly IMemoryCache Cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

    public static Prepared<Script> Parse(string script)
    {
        var cacheKey = $"{typeof(ConditionEvaluator)}_Script_{script}";

        return Cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;

            return Engine.PrepareScript(script, options: PreparationOptions);
        })!;
    }
}
