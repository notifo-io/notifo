// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Integrations;
using Notifo.Domain.Integrations.Smtp;

namespace Microsoft.Extensions.DependencyInjection;

public static class SmtpServiceExtensions
{
    public static void IntegrateSmtp(this IServiceCollection services)
    {
        services.AddSingletonAs<SmtpIntegration>()
            .As<IIntegration>();

        services.AddSingletonAs<SmtpEmailServerPool>()
            .AsSelf();
    }
}
