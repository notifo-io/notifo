// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations;

public enum DeliveryStatus
{
    Unknown = 0,
    Skipped = 1,
    Attempt = 2,
    Failed = 3,
    Sent = 4,
    Handled = 5
}
