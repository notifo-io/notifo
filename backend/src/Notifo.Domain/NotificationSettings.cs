// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain
{
    public sealed class NotificationSettings : Dictionary<string, NotificationSetting>
    {
        public NotificationSettings()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public NotificationSettings(IDictionary<string, NotificationSetting> dictionary)
            : base(dictionary, StringComparer.OrdinalIgnoreCase)
        {
        }

        public void OverrideBy(NotificationSettings? source)
        {
            if (source != null)
            {
                foreach (var (key, value) in source)
                {
                    if (TryGetValue(key, out var existing))
                    {
                        if (value.Send != NotificationSend.Inherit && existing.Send != NotificationSend.NotAllowed)
                        {
                            existing.Send = value.Send;
                        }

                        if (value.DelayInSeconds.HasValue)
                        {
                            existing.DelayInSeconds = value.DelayInSeconds;
                        }
                    }
                    else
                    {
                        this[key] = value;
                    }
                }
            }
        }
    }
}
