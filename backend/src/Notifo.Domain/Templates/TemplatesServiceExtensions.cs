// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Templates;

namespace Microsoft.Extensions.DependencyInjection;

public static class TemplatesServiceExtensions
{
    public static void AddMyTemplates(this IServiceCollection services)
    {
        services.AddSingletonAs<TemplateStore>()
            .As<ITemplateStore>();

        services.AddRequestHandler<TemplateStore, TemplateCommand, Template?>();
    }
}
