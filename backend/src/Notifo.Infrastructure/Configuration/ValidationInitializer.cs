// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Notifo.Infrastructure.Initialization;

namespace Notifo.Infrastructure.Configuration
{
    public sealed class ValidationInitializer : IInitializable
    {
        private readonly IEnumerable<IErrorProvider> errorProviders;

        public int InitializationOrder => int.MinValue;

        public ValidationInitializer(IEnumerable<IErrorProvider> errorProviders)
        {
            this.errorProviders = errorProviders;
        }

        public Task InitializeAsync(CancellationToken ct = default)
        {
            var errors = errorProviders.SelectMany(x => x.GetErrors()).ToList();

            if (errors.Count > 0)
            {
                throw new ConfigurationException(errors);
            }

            return Task.CompletedTask;
        }
    }
}
