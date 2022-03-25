// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Apps;
using Notifo.Domain.Users;

namespace Notifo.Domain.Channels.Email.Formatting
{
    public sealed class CompositeEmailFormatter : IEmailFormatter
    {
        private readonly IEnumerable<IEmailFormatter> inner;

        public CompositeEmailFormatter(IEnumerable<IEmailFormatter> inner)
        {
            this.inner = inner;
        }

        public bool Accepts(EmailTemplate template)
        {
            return true;
        }

        public async ValueTask<EmailTemplate> CreateInitialAsync(string? type = null,
            CancellationToken ct = default)
        {
            foreach (var formatter in inner)
            {
                var template = await formatter.CreateInitialAsync(type, ct);

                if (template != null)
                {
                    return template;
                }
            }

            throw new NotSupportedException();
        }

        public ValueTask<EmailTemplate> ParseAsync(EmailTemplate input,
            CancellationToken ct = default)
        {
            foreach (var formatter in inner)
            {
                if (formatter.Accepts(input))
                {
                    return formatter.ParseAsync(input, ct);
                }
            }

            throw new NotSupportedException();
        }

        public ValueTask<FormattedEmail> FormatAsync(List<EmailJob> jobs, EmailTemplate template, App app, User user, bool noCache = false,
            CancellationToken ct = default)
        {
            foreach (var formatter in inner)
            {
                if (formatter.Accepts(template))
                {
                    return formatter.FormatAsync(jobs, template, app, user, noCache, ct);
                }
            }

            throw new NotSupportedException();
        }
    }
}
