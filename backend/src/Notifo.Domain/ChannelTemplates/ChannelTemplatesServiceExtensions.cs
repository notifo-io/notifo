// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.ChannelTemplates;
using Notifo.Domain.ChannelTemplates.MongoDb;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ChannelTemplatesServiceExtensions
    {
        public static void AddChannelTemplates<T>(this IServiceCollection services) where T : class
        {
            services.AddSingletonAs<MongoDbChannelTemplateRepository<T>>()
                .As<IChannelTemplateRepository<T>>();

            services.AddSingletonAs<ChannelTemplateStore<T>>()
                .As<IChannelTemplateStore<T>>();
        }
    }
}
