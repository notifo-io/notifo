// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;
using Squidex.Hosting;

namespace Notifo.Infrastructure
{
    public sealed class LanguagesInitializer : IInitializable
    {
        private readonly LanguagesOptions options;

        public LanguagesInitializer(IOptions<LanguagesOptions> options)
        {
            Guard.NotNull(options, nameof(options));

            this.options = options.Value;
        }

        public Task InitializeAsync(
            CancellationToken ct)
        {
            foreach (var (key, value) in options)
            {
                if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value))
                {
                    Language.AddLanguage(key, value);
                }
            }

            return Task.CompletedTask;
        }
    }
}
