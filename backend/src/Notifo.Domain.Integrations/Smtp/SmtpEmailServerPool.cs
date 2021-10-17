// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.Extensions.Caching.Memory;

namespace Notifo.Domain.Integrations.Smtp
{
    public sealed class SmtpEmailServerPool : CachePool<SmtpEmailServer>
    {
        public SmtpEmailServerPool(IMemoryCache memoryCache)
            : base(memoryCache)
        {
        }

        public SmtpEmailServer GetServer(SmtpOptions options)
        {
            var cacheKey = $"SMTPServer_{options.Host}_{options.Username}_{options.Password}_{options.Host}";

            var found = GetOrCreate(cacheKey, () =>
            {
                var sender = new SmtpEmailServer(options);

                return sender;
            });

            return found;
        }
    }
}
