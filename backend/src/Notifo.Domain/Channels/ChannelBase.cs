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

public abstract class ChannelBase<T>(IServiceProvider serviceProvider) : ICommunicationChannel
{
    public abstract string Name { get; }

    protected IAppStore AppStore { get; } = serviceProvider.GetRequiredService<IAppStore>();

    protected IIntegrationManager IntegrationManager { get; } = serviceProvider.GetRequiredService<IIntegrationManager>();

    protected IMediator Mediator { get; } = serviceProvider.GetRequiredService<IMediator>();

    protected ILogger<T> Log { get; } = serviceProvider.GetRequiredService<ILogger<T>>();

    protected ILogStore LogStore { get; } = serviceProvider.GetRequiredService<ILogStore>();

    protected IUserNotificationStore UserNotificationStore { get; } = serviceProvider.GetRequiredService<IUserNotificationStore>();

    protected IUserStore UserStore { get; } = serviceProvider.GetRequiredService<IUserStore>();

    public virtual bool IsSystem => false;

    public abstract Task SendAsync(UserNotification notification, ChannelContext context,
        CancellationToken ct);

    public abstract IEnumerable<SendConfiguration> GetConfigurations(UserNotification notification, ChannelContext context);

    public virtual Task HandleSeenAsync(UserNotification notification, ChannelContext context)
    {
        return Task.CompletedTask;
    }
}
