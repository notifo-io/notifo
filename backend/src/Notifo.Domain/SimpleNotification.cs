// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain;

public class SimpleNotification
{
    public Guid Id { get; set; }

    public string EventId { get; set; }

    public NotificationProperties? Properties { get; set; }

    public NotificationFormatting<string> Formatting { get; set; }
}
