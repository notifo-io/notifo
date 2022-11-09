// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NJsonSchema;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace Notifo.Areas.Api.OpenApi;

public sealed class FixProcessor : IOperationProcessor
{
    private static readonly JsonSchema StringSchema = new JsonSchema { Type = JsonObjectType.String };

    public bool Process(OperationProcessorContext context)
    {
        foreach (var parameter in context.Parameters.Values)
        {
            if (parameter.IsRequired && parameter.Schema is { Type: JsonObjectType.String })
            {
                parameter.Schema = StringSchema;
            }
        }

        return true;
    }
}
