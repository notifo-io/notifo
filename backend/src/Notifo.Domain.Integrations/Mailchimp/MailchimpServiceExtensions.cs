// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Integrations;
using Notifo.Domain.Integrations.Mailchimp;

namespace Microsoft.Extensions.DependencyInjection;

public static class MailchimpServiceExtensions
{
    public static IServiceCollection AddIntegrationMailchimp(this IServiceCollection services)
    {
        services.AddHttpClient("Mailchimp", options =>
        {
            options.BaseAddress = new Uri("https://mandrillapp.com/api/1.0/");
        });

        services.AddSingletonAs<MailchimpIntegration>()
            .As<IIntegration>();

        return services;
    }
}
