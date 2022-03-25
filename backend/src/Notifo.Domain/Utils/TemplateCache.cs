// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using Fluid;
using Notifo.Domain.Apps;
using Notifo.Domain.Users;
using Notifo.Infrastructure;
using Squidex.Caching;

namespace Notifo.Domain.Utils
{
    public static class TemplateCache
    {
        private static readonly LRUCache<string, (FluidTemplate? Template, string[]? Error)> Cache = new LRUCache<string, (FluidTemplate?, string[]?)>(5000);

        static TemplateCache()
        {
            TemplateContext.GlobalMemberAccessStrategy.MemberNameStrategy = MemberNameStrategies.CamelCase;

            TemplateContext.GlobalMemberAccessStrategy.Register<User>();
            TemplateContext.GlobalMemberAccessStrategy.Register<App>();
        }

        public static FluidTemplate Parse(string input, bool bypass = false)
        {
            Guard.NotNull(input, nameof(input));

            if (!bypass)
            {
                lock (Cache)
                {
                    if (Cache.TryGetValue(input, out var temp))
                    {
                        if (temp.Error != null)
                        {
                            throw new TemplateParseException(input, temp.Error);
                        }

                        if (temp.Template != null)
                        {
                            return temp.Template;
                        }
                    }
                }
            }

            if (FluidTemplate.TryParse(input, out var result, out var errors))
            {
                if (!bypass)
                {
                    lock (Cache)
                    {
                        Cache.Set(input, (result, null));
                    }
                }

                return result;
            }
            else
            {
                if (!bypass)
                {
                    lock (Cache)
                    {
                        Cache.Set(input, (null, errors.ToArray()));
                    }
                }

                throw new TemplateParseException(input, errors);
            }
        }
    }
}
