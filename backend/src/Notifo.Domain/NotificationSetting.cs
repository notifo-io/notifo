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

        public bool ShouldSend
        {
            get { return Send == NotificationSend.Send; }
        }

        public Duration DelayDuration
        {
            get { return Duration.FromSeconds(DelayInSeconds ?? 0); }
        }
    }
}
