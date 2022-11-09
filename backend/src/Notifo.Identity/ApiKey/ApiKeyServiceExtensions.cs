// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Utils;
using Notifo.Identity.ApiKey;

namespace Microsoft.Extensions.DependencyInjection;

public static class ApiKeyServiceExtensions
{
    public static void AddMyApiKey(this IServiceCollection services)
    {
        services.AddSingletonAs<ApiKeyGenerator>()
            .As<IApiKeyGenerator>();
    }
}
