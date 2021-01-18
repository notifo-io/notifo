// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Notifo.Domain.Utils;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class UtilsServiceExtensions
    {
        public static void AddMyUtils(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingletonAs<ImageFormatter>()
                .As<IImageFormatter>();
        }
    }
}
