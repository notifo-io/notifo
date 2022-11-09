// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Channels.Email;

public sealed class EmailMessage
{
    public string FromEmail { get; set; }

    public string? FromName { get; set; }

    public string ToEmail { get; init; }

    public string? ToName { get; init; }

    public string Subject { get; init; }

    public string? BodyText { get; init; }

    public string? BodyHtml { get; init; }
}
