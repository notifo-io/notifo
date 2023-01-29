// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Integrations;
using Notifo.Domain.Integrations.Telekom;

namespace Microsoft.Extensions.DependencyInjection;

public static class TelekomServiceExtensions
{
    public static IServiceCollection AddIntegrationTelekom(this IServiceCollection services)
    {
        services.AddSingletonAs<TelekomSmsIntegration>()
            .As<IIntegration>();

        return services;
    }
}
