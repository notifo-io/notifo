// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure;

namespace Notifo.Domain.Topics
{
    public sealed record TopicQuery : QueryBase
    {
        public string? Query { get; set; }

        public TopicQueryScope Scope { get; set; }
    }
}
