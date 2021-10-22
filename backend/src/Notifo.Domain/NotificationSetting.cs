// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Notifo.Domain
{
    public sealed class NotificationSetting
    {
        public NotificationSend Send { get; set; }

        public int? DelayInSeconds { get; set; }

        public string? Template { get; set; }

        public NotificationProperties? Properties { get; set; }

        public bool ShouldSend
        {
            get => Send == NotificationSend.Send;
        }

        public Duration DelayDuration
        {
            get => Duration.FromSeconds(DelayInSeconds ?? 0);
        }
    }
}
