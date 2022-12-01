// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Apps;
using Notifo.Domain.Identity;
using Notifo.Domain.Integrated;
using Notifo.Domain.Log;
using Notifo.Domain.Templates;

namespace Microsoft.Extensions.DependencyInjection;

public static class IntegratedServiceExtensions
{
    public static void AddIntegratedApp(this IServiceCollection services)
    {
        services.AddSingletonAs<IntegratedAppService>()
            .As<IIntegratedAppService>().AsSelf();

        services.AddMiddleware<IntegratedAppService, AddContributor>();
        services.AddMiddleware<IntegratedAppService, DeleteTemplate>();
        services.AddMiddleware<IntegratedAppService, FirstLogCreated>();
        services.AddMiddleware<IntegratedAppService, RemoveContributor>();
        services.AddMiddleware<IntegratedAppService, UserDeleted>();
        services.AddMiddleware<IntegratedAppService, UserRegistered>();
        services.AddMiddleware<IntegratedAppService, UserUpdated>();
    }
}
