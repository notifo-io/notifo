// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Notifo.Infrastructure.Messaging.RabbitMq
{
    public sealed class RabbitMqOptions
    {
        public Uri Uri { get; set; }

        public int MaxDegreeOfParallelism { get; set; } = Environment.ProcessorCount;
    }
}
