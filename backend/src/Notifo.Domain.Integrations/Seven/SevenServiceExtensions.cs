// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Integrations;
using Notifo.Domain.Integrations.Seven;

namespace Microsoft.Extensions.DependencyInjection;

public static class SevenServiceExtensions
{
    public static IServiceCollection AddIntegrationSeven(this IServiceCollection services)
    {
        services.AddSingletonAs<SevenSmsIntegration>()
            .As<IIntegration>();

        services.AddSingletonAs<SevenSmsClientPool>()
            .AsSelf();

        return services;
    }
}
