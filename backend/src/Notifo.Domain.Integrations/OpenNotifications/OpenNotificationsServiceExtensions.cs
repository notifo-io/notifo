// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Notifo.Domain.Integrations;
using Notifo.Domain.Integrations.OpenNotifications;
using OpenNotifications;

namespace Microsoft.Extensions.DependencyInjection;

public static class OpenNotificationsServiceExtensions
{
    public static IServiceCollection AddIntegrationOpenNotifications(this IServiceCollection services, IConfiguration config)
    {
        var options = config.GetSection("os").Get<OpenNotificationsOptions>();

        if (options?.Services != null)
        {
            foreach (var service in options.Services)
            {
                if (service?.IsValid() == true)
                {
                    services.AddOpenNotifications(service.Name, new Uri(service.Url));
                }
            }
        }

        services.AddSingletonAs<OpenNotificationsRegistry>()
            .As<IIntegrationRegistry>();

        return services;
    }
}
