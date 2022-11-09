// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Esprima;
using Esprima.Ast;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Notifo.Domain.Integrations;

internal static class ConditionParser
{
    private static readonly ParserOptions DefaultParserOptions = new ParserOptions { Tolerant = true };
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
    private static readonly IMemoryCache Cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

    public static Script Parse(string script)
    {
        var cacheKey = $"{typeof(ConditionEvaluator)}_Script_{script}";

        return Cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;

            var parser = new JavaScriptParser(script, DefaultParserOptions);

            return parser.ParseScript();
        });
    }
}
