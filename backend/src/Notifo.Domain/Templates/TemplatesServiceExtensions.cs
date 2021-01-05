// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Templates;
using Notifo.Domain.Templates.MongoDb;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TemplatesServiceExtensions
    {
        public static void AddMyTemplates(this IServiceCollection services)
        {
            services.AddSingletonAs<TemplateStore>()
                .As<ITemplateStore>();
        }

        public static void AddMyMongoTemplates(this IServiceCollection services)
        {
            services.AddSingletonAs<MongoDbTemplateRepository>()
                .As<ITemplateRepository>();
        }
    }
}
