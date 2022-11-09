// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace Notifo.Areas.Api.OpenApi;

public sealed class CommonProcessor : IDocumentProcessor
{
    public void Process(DocumentProcessorContext context)
    {
        context.Document.BasePath = "/api";

        var logo = new
        {
            url = "/logo.svg"
        };

        context.Document.Info.Version = "1.0.0";
        context.Document.Info.ExtensionData = new Dictionary<string, object>
        {
            ["x-logo"] = logo
        };
    }
}
