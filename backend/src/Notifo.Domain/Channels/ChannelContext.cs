// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Apps;
using Notifo.Domain.Users;

namespace Notifo.Domain.Channels;

public sealed class ChannelContext
{
    required public App App { get; set; }

    required public User User { get; set; }

    required public Guid ConfigurationId { get; set; }

    required public ChannelSetting Setting { get; set; }

    required public SendConfiguration Configuration { get; set; }

    required public string AppId { get; set; }

    required public string UserId { get; set; }

    required public bool IsUpdate { get; set; }
}
