// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Notifo.Domain.Channels
{
    public interface IChannelJob
    {
        Guid NotificationId { get; }

        Duration Delay { get; }

        ChannelCondition Condition { get; }

        string Configuration { get; }

        bool IsUpdate => false;
    }
}
