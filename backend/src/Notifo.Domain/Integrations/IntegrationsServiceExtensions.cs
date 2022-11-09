// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Integrations;

namespace Microsoft.Extensions.DependencyInjection;

public static class IntegrationsServiceExtensions
{
    public static void AddMyIntegrations(this IServiceCollection services)
    {
        services.AddSingletonAs<IntegrationManager>()
            .As<IIntegrationManager>();
    }
}
