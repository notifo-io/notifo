// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Integrations;
using Notifo.Domain.Integrations.Mailjet;

namespace Microsoft.Extensions.DependencyInjection;

public static class MailjetServiceExtensions
{
    public static IServiceCollection AddIntegrationMailjet(this IServiceCollection services)
    {
        services.AddSingletonAs<MailjetIntegration>()
            .As<IIntegration>();

        services.AddSingletonAs<MailjetEmailServerPool>()
            .AsSelf();

        return services;
    }
}
