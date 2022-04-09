// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain
{
    public sealed record ChannelSetting
    {
        public ChannelSend Send { get; init; }

        public ChannelCondition Condition { get; init; }

        public int? DelayInSeconds { get; init; }

        public string? Template { get; init; }

        public NotificationProperties? Properties { get; init; }

        public ChannelSetting OverrideBy(ChannelSetting? source)
        {
            if (source == null || Send == ChannelSend.NotAllowed)
            {
                return this;
            }

            var properties = Properties;

            if (source.Properties?.Count > 0)
            {
                properties = Properties != null ? new NotificationProperties(Properties) : new NotificationProperties();

                foreach (var (key, value) in source.Properties)
                {
                    properties[key] = value;
                }
            }

            return new ChannelSetting
            {
                Condition =
                    source.Condition != ChannelCondition.Inherit ?
                    source.Condition :
                    Condition,
                DelayInSeconds = source.DelayInSeconds ?? DelayInSeconds,
                Send =
                    source.Send != ChannelSend.Inherit ?
                    source.Send :
                    Send,
                Template =
                    source.Template != null && !string.IsNullOrWhiteSpace(source.Template) ?
                    source.Template :
                    Template,
                Properties = properties
            };
        }
    }
}
