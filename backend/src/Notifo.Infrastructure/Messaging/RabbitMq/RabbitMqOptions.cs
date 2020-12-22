// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Notifo.Infrastructure.Configuration;

namespace Notifo.Infrastructure.Messaging.RabbitMq
{
    public sealed class RabbitMqOptions : IValidatableOptions
    {
        public Uri Uri { get; set; }

        public int MaxDegreeOfParallelism { get; set; } = Environment.ProcessorCount;

        public IEnumerable<ConfigurationError> Validate()
        {
            if (Uri == null)
            {
                yield return new ConfigurationError("Value is required.", nameof(Uri));
            }
        }
    }
}
