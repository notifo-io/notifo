// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations;

public abstract class BaseMessage
{
    public Guid NotificationId { get; set; }

    public bool IsConfirmed { get; set; }

    public bool IsUpdate { get; set; }

    public bool Silent { get; set; }

    public string? TrackDeliveredUrl { get; set; }

    public string? TrackSeenUrl { get; set; }

    public string TrackingToken { get; set; }

    public string UserId { get; set; }

    public string UserLanguage { get; set; }
}
