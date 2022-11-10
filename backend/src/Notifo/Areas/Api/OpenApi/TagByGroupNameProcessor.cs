// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace Notifo.Areas.Api.OpenApi;

public sealed class TagByGroupNameProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        var groupName = context.ControllerType.GetCustomAttribute<ApiExplorerSettingsAttribute>()?.GroupName;

        if (!string.IsNullOrWhiteSpace(groupName))
        {
            context.OperationDescription.Operation.Tags = new List<string> { groupName };

            return true;
        }
        else
        {
            return false;
        }
    }
}
