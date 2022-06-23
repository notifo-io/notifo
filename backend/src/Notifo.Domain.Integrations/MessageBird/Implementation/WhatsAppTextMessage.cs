// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Notifo.Domain.Integrations.MessageBird.Implementation
{
    public sealed record WhatsAppTextMessage(
        string From,
        string To,
        string Message,
        string? ReportUrl)
    {
    }
}
