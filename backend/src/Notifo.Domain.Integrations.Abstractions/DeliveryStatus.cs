// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations;

public enum DeliveryStatus
{
    /// <summary>
    /// Nothing has happened yet to send out the notification.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The notification has not been send and it has not been tried, because some precondition is not fulfilled.
    /// </summary>
    Skipped = 1,

    /// <summary>
    /// The sender or channel has started the process of sending the notification.
    /// </summary>
    Attempt = 2,

    /// <summary>
    /// The notification was not sent because of a failure.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// The notification was sent, but is waiting for a final confirmation.
    /// </summary>
    Sent = 4,

    /// <summary>
    /// The notification has been delivered and handled.
    /// </summary>
    Handled = 5
}
