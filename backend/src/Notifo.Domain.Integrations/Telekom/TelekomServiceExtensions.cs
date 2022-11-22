// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Integrations;
using Notifo.Domain.Integrations.Telekom;

namespace Microsoft.Extensions.DependencyInjection;

public static class TelekomServiceExtensions
{
    public static void IntegrateTelekom(this IServiceCollection services)
    {
        services.AddSingletonAs<TelekomIntegration>()
            .As<IIntegration>();
    }
}
