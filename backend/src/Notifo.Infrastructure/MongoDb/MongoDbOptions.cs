// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Notifo.Infrastructure.Configuration;

namespace Notifo.Infrastructure.MongoDb
{
    public sealed class MongoDbOptions : IValidatableOptions
    {
        public string ConnectionString { get; set; }

        public string DatabaseName { get; set; }

        public IEnumerable<ConfigurationError> Validate()
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                yield return new ConfigurationError("Value is required.", nameof(ConnectionString));
            }

            if (string.IsNullOrWhiteSpace(DatabaseName))
            {
                yield return new ConfigurationError("Value is required.", nameof(DatabaseName));
            }
        }
    }
}
