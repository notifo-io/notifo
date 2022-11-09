// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Channels.Email.Formatting;

namespace Notifo.Domain.Channels.Email;

public sealed record EmailTemplate
{
    public string Subject { get; init; }

    public string BodyHtml { get; init; }

    public string? BodyText { get; init; }

    public string? FromEmail { get; init; }

    public string? FromName { get; init; }

    public string? Kind { get; init; }

    public ParsedEmailTemplate? ParsedBodyText { get; init; }

    public ParsedEmailTemplate? ParsedBodyHtml { get; init; }
}
