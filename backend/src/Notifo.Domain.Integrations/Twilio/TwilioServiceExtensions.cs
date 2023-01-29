﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Integrations;
using Notifo.Domain.Integrations.Twilio;

namespace Microsoft.Extensions.DependencyInjection;

public static class TwilioServiceExtensions
{
    public static IServiceCollection AddIntegrationTwilio(this IServiceCollection services)
    {
        services.AddSingletonAs<TwilioSmsIntegration>()
            .As<IIntegration>();

        return services;
    }
}
