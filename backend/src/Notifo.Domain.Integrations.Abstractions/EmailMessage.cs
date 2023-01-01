// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations;

public sealed record EmailMessage
{
    public string FromEmail { get; init; }

    public string? FromName { get; init; }

    public string ToEmail { get; init; }

    public string? ToName { get; init; }

    public string Subject { get; init; }

    public string? BodyText { get; init; }

    public string? BodyHtml { get; init; }
}
