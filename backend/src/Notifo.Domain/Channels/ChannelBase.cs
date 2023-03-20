// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Notifo.Domain.Apps;
using Notifo.Domain.Integrations;
using Notifo.Domain.Log;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Users;
using Notifo.Infrastructure.Mediator;

namespace Notifo.Domain.Channels;

public abstract class ChannelBase<T> : ICommunicationChannel
{
    public abstract string Name { get; }

    protected IAppStore AppStore { get; }

    protected IIntegrationManager IntegrationManager { get; }

    protected IMediator Mediator { get; }

    protected ILogger<T> Log { get; }

    protected ILogStore LogStore { get; }

    protected IUserNotificationStore UserNotificationStore { get; }

    protected IUserStore UserStore { get; }

    public virtual bool IsSystem => false;

    protected ChannelBase(IServiceProvider serviceProvider)
    {
        AppStore = serviceProvider.GetRequiredService<IAppStore>();
        Log = serviceProvider.GetRequiredService<ILogger<T>>();
        LogStore = serviceProvider.GetRequiredService<ILogStore>();
        Mediator = serviceProvider.GetRequiredService<IMediator>();
        UserNotificationStore = serviceProvider.GetRequiredService<IUserNotificationStore>();
        UserStore = serviceProvider.GetRequiredService<IUserStore>();
        IntegrationManager = serviceProvider.GetRequiredService<IIntegrationManager>();
    }

    public abstract Task SendAsync(UserNotification notification, ChannelContext context,
        CancellationToken ct);

    public abstract IEnumerable<SendConfiguration> GetConfigurations(UserNotification notification, ChannelContext context);

    public virtual Task HandleSeenAsync(UserNotification notification, ChannelContext context)
    {
        return Task.CompletedTask;
    }
}
