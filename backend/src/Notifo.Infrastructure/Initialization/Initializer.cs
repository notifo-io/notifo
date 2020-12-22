// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Squidex.Log;

namespace Notifo.Infrastructure.Initialization
{
    public sealed class Initializer : IHostedService
    {
        private readonly IEnumerable<IInitializable> initializables;
        private readonly ISemanticLog log;

        public Initializer(IEnumerable<IInitializable> initializables, ISemanticLog log)
        {
            this.initializables = initializables;

            this.log = log;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var initializable in initializables.OrderBy(x => x.InitializationOrder))
            {
                log.LogDebug(w => w
                    .WriteProperty("action", "InitializeService")
                    .WriteProperty("status", "Started")
                    .WriteProperty("service", initializable.ToString()));

                await initializable.InitializeAsync(cancellationToken);

                log.LogInformation(w => w
                    .WriteProperty("action", "InitializeService")
                    .WriteProperty("status", "Finished")
                    .WriteProperty("service", initializable.ToString()));
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var initializable in initializables.OrderBy(x => x.InitializationOrder))
            {
                try
                {
                    log.LogDebug(w => w
                        .WriteProperty("action", "ReleaseService")
                        .WriteProperty("status", "Started")
                        .WriteProperty("service", initializable.GetType().FullName));

                    await initializable.ReleaseAsync(cancellationToken);

                    log.LogInformation(w => w
                        .WriteProperty("action", "ReleaseService")
                        .WriteProperty("status", "Finished")
                        .WriteProperty("service", initializable.ToString()));
                }
                catch (Exception ex)
                {
                    log.LogError(ex, w => w
                        .WriteProperty("action", "ReleaseService")
                        .WriteProperty("status", "Failed")
                        .WriteProperty("service", initializable.ToString()));
                }
            }
        }
    }
}
