// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations;

public enum MessagingResult
{
    Unknown,
    Skipped,
    Sent,
    Delivered,
    Failed
}
