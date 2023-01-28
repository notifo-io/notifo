// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Integrations;
using Notifo.Domain.Integrations.OpenNotifications;

namespace Microsoft.Extensions.DependencyInjection;

public static class OpenNotificationsServiceExtensions
{
    public static void IntegrateOpenNotifications(this IServiceCollection services)
    {
        services.AddSingletonAs<OpenNotificationsRegistry>()
            .As<IIntegrationRegistry>();
    }
}
