// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure;

namespace Notifo.Domain.ChannelTemplates
{
    public sealed class ChannelTemplateQuery : QueryBase
    {
        public string? Query { get; set; }
    }
}
