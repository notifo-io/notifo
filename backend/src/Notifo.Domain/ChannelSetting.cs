// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain;

public sealed record ChannelSetting
{
    public ChannelSend Send { get; set; }

    public ChannelCondition Condition { get; set; }

    public int? DelayInSeconds { get; set; }

    public string? Template { get; set; }

    public NotificationProperties? Properties { get; set; }

    public void OverrideBy(ChannelSetting? source)
    {
        if (source == null || Send == ChannelSend.NotAllowed)
        {
            return;
        }

        if (source.Send != ChannelSend.Inherit)
        {
            Send = source.Send;
        }

        if (source.Condition != ChannelCondition.Inherit)
        {
            Condition = source.Condition;
        }

        if (source.DelayInSeconds.HasValue)
        {
            DelayInSeconds = source.DelayInSeconds;
        }

        if (!string.IsNullOrWhiteSpace(source.Template))
        {
            Template = source.Template;
        }

        if (source.Properties?.Count > 0)
        {
            Properties ??= new NotificationProperties();

            foreach (var (key, value) in source.Properties)
            {
                Properties[key] = value;
            }
        }
    }
}
