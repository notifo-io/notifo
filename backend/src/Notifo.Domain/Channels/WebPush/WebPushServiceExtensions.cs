// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Notifo.Domain.Channels;
using Notifo.Domain.Channels.WebPush;
using Notifo.Infrastructure.Scheduling;

namespace Microsoft.Extensions.DependencyInjection;

public static class WebPushServiceExtensions
{
    public static void AddMyWebPushChannel(this IServiceCollection services, IConfiguration config)
    {
        services.ConfigureAndValidate<WebPushOptions>(config, "webPush");

        services.AddSingletonAs<WebPushChannel>()
            .As<ICommunicationChannel>().As<IWebPushService>().As<IScheduleHandler<WebPushJob>>();

        services.AddScheduler<WebPushJob>(Providers.WebPush);
    }
}
