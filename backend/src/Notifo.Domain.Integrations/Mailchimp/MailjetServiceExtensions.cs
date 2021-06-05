// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Notifo.Domain.Integrations;
using Notifo.Domain.Integrations.Mailchimp;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MailchimpServiceExtensions
    {
        public static void IntegrateMailchimp(this IServiceCollection services)
        {
            services.AddHttpClient("Mailchimp", options =>
            {
                options.BaseAddress = new Uri("https://mandrillapp.com/api/1.0/");
            });

            services.AddSingletonAs<MailchimpIntegration>()
                .As<IIntegration>();
        }
    }
}
