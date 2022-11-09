// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Apps;
using Notifo.Domain.Users;
using Notifo.Infrastructure;

namespace Notifo.Domain.Channels.Email.Formatting;

public sealed class CompositeEmailFormatter : IEmailFormatter
{
    private readonly IEnumerable<IEmailFormatter> inner;

    public CompositeEmailFormatter(IEnumerable<IEmailFormatter> inner)
    {
        this.inner = inner;
    }

    public bool Accepts(string? kind)
    {
        return true;
    }

    public async ValueTask<EmailTemplate> CreateInitialAsync(string? kind = null,
        CancellationToken ct = default)
    {
        foreach (var formatter in inner)
        {
            if (formatter.Accepts(kind))
            {
                var template = await formatter.CreateInitialAsync(kind, ct);

                if (template != null)
                {
                    return template;
                }
            }
        }

        ThrowHelper.NotSupportedException();
        return default!;
    }

    public ValueTask<EmailTemplate> ParseAsync(EmailTemplate input, bool strict,
        CancellationToken ct = default)
    {
        foreach (var formatter in inner)
        {
            if (formatter.Accepts(input.Kind))
            {
                return formatter.ParseAsync(input, strict, ct);
            }
        }

        ThrowHelper.NotSupportedException();
        return default!;
    }

    public ValueTask<FormattedEmail> FormatAsync(EmailTemplate input, IReadOnlyList<EmailJob> jobs, App app, User user, bool noCache = false,
        CancellationToken ct = default)
    {
        foreach (var formatter in inner)
        {
            if (formatter.Accepts(input.Kind))
            {
                return formatter.FormatAsync(input, jobs, app, user, noCache, ct);
            }
        }

        ThrowHelper.NotSupportedException();
        return default;
    }
}
