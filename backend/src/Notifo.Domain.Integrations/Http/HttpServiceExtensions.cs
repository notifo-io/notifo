// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Integrations;
using Notifo.Domain.Integrations.Http;

namespace Microsoft.Extensions.DependencyInjection;

public static class HttpServiceExtensions
{
    public static IServiceCollection AddIntegrationHttp(this IServiceCollection services)
    {
        services.AddSingletonAs<HttpIntegration>()
            .As<IIntegration>();

        return services;
    }
}
