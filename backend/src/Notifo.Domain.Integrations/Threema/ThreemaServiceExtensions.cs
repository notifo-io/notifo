// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Integrations;
using Notifo.Domain.Integrations.Threema;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ThreemaServiceExtensions
    {
        public static void IntegrateThreema(this IServiceCollection services)
        {
            services.AddSingletonAs<ThreemaSimpleIntegration>()
                .As<IIntegration>();
        }
    }
}
