// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure;

namespace Notifo.Domain
{
    public sealed class ChannelSettings : Dictionary<string, ChannelSetting>
    {
        public ChannelSettings()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public ChannelSettings(IDictionary<string, ChannelSetting> dictionary)
            : base(dictionary, StringComparer.OrdinalIgnoreCase)
        {
        }

        public static ChannelSettings Merged(params ChannelSettings?[] sources)
        {
            var result = new ChannelSettings();

            foreach (var source in sources)
            {
                foreach (var (channel, setting) in source.OrEmpty())
                {
                    result.GetOrAddNew(channel).OverrideBy(setting);
                }
            }

            return result;
        }
    }
}
