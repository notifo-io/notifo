// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure;

namespace Microsoft.Extensions.DependencyInjection;

public static class InfrastructureServiceExtensions
{
    public static void AddMyInfrastructure(this IServiceCollection services)
    {
        services.AddSingletonAs<Randomizer>()
            .AsSelf();
    }
}
