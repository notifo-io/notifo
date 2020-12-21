// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain
{
    public sealed class NotificationSetting
    {
        public bool? Send { get; set; }

        public int? DelayInSeconds { get; set; }

        public bool ShouldSend
        {
            get { return Send == true; }
        }

        public int DelayInSecondsOrZero
        {
            get { return DelayInSeconds ?? 0; }
        }
    }
}
