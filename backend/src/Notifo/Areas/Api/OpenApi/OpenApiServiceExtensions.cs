// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NJsonSchema;
using NJsonSchema.Generation.TypeMappers;
using NodaTime;
using Notifo.Areas.Api.OpenApi;
using Notifo.Domain;
using NSwag.Generation.Processors;

namespace Microsoft.Extensions.DependencyInjection;

public static class OpenApiServiceExtensions
{
    public static void AddMyOpenApi(this IServiceCollection services)
    {
        services.AddSingletonAs<ErrorDtoProcessor>()
            .As<IDocumentProcessor>();

        services.AddSingletonAs<CommonProcessor>()
            .As<IDocumentProcessor>();

        services.AddSingletonAs<TagXmlProcessor>()
            .As<IDocumentProcessor>();

        services.AddSingletonAs<TagByGroupNameProcessor>()
            .As<IOperationProcessor>();

        services.AddOpenApiDocument(settings =>
        {
            settings.AllowReferencesWithProperties = true;

            settings.TypeMappers = new List<ITypeMapper>
            {
                CreateStringMap<Instant>(JsonFormatStrings.DateTime),
                CreateStringMap<TopicId>()
            };

            settings.ReflectionService = new ReflectionServices();
        });
    }

    private static ITypeMapper CreateStringMap<T>(string? format = null)
    {
        return new PrimitiveTypeMapper(typeof(T), schema =>
        {
            schema.Type = JsonObjectType.String;

            schema.Format = format;
        });
    }
}
