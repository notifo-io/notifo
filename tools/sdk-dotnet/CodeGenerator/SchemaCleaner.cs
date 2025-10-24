// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using NSwag;
using Squidex.Text;

namespace CodeGenerator;

internal static class SchemaCleaner
{
    public static void AddExtensions(OpenApiDocument document)
    {
        document.Security = null;
        document.Components.SecuritySchemes.Clear();

        static void AddExtensions(OpenApiOperation operation)
        {
            operation.ExtensionData ??= new Dictionary<string, object>();
            operation.ExtensionData["x-method-name"] = operation.OperationId.Split('_')[^1].ToCamelCase();

            foreach (var parameter in operation.Parameters.ToList())
            {
                if (parameter.Kind == OpenApiParameterKind.Header)
                {
                    const string Prefix = "X-";

                    var name = parameter.Name;

                    parameter.ExtensionData ??= new Dictionary<string, object>();
                    parameter.ExtensionData["x-header-name"] = name;

                    if (name.StartsWith(Prefix, StringComparison.Ordinal))
                    {
                        name = name[Prefix.Length..];
                    }

                    parameter.Name = name.ToCamelCase();
                }

                operation.Security = null;
            }
        }

        foreach (var description in document.Operations)
        {
            AddExtensions(description.Operation);
        }

        document.ExtensionData ??= new Dictionary<string, object>();
    }
}
