// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Utils;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class UtilsServiceExtensions
    {
        public static void AddMyUtils(this IServiceCollection services)
        {
            services.AddSingletonAs<ImageFormatter>()
                .As<IImageFormatter>();
        }
    }
}
