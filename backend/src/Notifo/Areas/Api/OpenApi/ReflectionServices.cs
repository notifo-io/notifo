// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Namotion.Reflection;
using NJsonSchema.Generation;
using Notifo.Infrastructure.Collections;

namespace Notifo.Areas.Api.OpenApi;

public sealed class ReflectionServices : DefaultReflectionService
{
    protected override bool IsArrayType(ContextualType contextualType)
    {
        if (contextualType.Type.IsGenericType &&
            contextualType.Type.GetGenericTypeDefinition() == typeof(ReadonlyList<>))
        {
            return true;
        }

        return base.IsArrayType(contextualType);
    }

    protected override bool IsDictionaryType(ContextualType contextualType)
    {
        if (contextualType.Type.IsGenericType &&
            contextualType.Type.GetGenericTypeDefinition() == typeof(ReadonlyDictionary<,>))
        {
            return true;
        }

        return base.IsDictionaryType(contextualType);
    }
}
