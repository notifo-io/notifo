// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure;

namespace Notifo.Domain.Topics
{
    public sealed class TopicQuery : QueryBase
    {
        public string? Query { get; set; }

        public TopicQueryScope Scope { get; set; }
    }
}
