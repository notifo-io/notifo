// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Integrations;
using Notifo.Domain.Integrations.Firebase;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FirebaseServiceExtensions
    {
        public static void AddIntegrationFirebase(this IServiceCollection services)
        {
            services.AddSingletonAs<FirebaseIntegration>()
                .As<IIntegration>();
        }
    }
}
