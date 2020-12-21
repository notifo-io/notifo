// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure;

namespace Notifo.Domain.Templates
{
    public sealed class TemplateQuery : QueryBase
    {
        public string? Query { get; set; }
    }
}