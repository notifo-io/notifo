// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Notifo.Domain.Integrations.MessageBird
{
    public sealed record MessageBirdVoiceMessage(
        string To,
        string Body,
        string Language = "en-us",
        string? Reference = null,
        string? ReportUrl = null)
    {
    }
}
