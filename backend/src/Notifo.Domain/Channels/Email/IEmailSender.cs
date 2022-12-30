// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Channels.Email;

#pragma warning disable MA0048 // File name must match type name

public interface IEmailSender
{
    string Name { get; }

    Task SendAsync(EmailRequest request,
        CancellationToken ct = default);
}

public sealed record EmailRequest
{
    public string FromEmail { get; init; }

    public string? FromName { get; init; }

    public string ToEmail { get; init; }

    public string? ToName { get; init; }

    public string Subject { get; init; }

    public string? BodyText { get; init; }

    public string? BodyHtml { get; init; }
}
