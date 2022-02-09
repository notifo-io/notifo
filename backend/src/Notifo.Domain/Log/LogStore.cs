// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using NodaTime;
using Notifo.Domain.Log.Internal;
using Notifo.Infrastructure;

namespace Notifo.Domain.Log
{
    public sealed class LogStore : ILogStore, IDisposable
    {
        private readonly LogCollector collector;
        private readonly ILogRepository repository;
        private readonly ILogger<LogStore> log;

        public LogStore(ILogRepository repository,
            ILogger<LogStore> log, IClock clock)
        {
            this.repository = repository;

            this.log = log;

            collector = new LogCollector(repository, clock, 10, 3000);
        }

        public void Dispose()
        {
            collector.StopAsync().Wait();
        }

        public Task LogAsync(string appId, string system, string message)
        {
            Guard.NotNullOrEmpty(system);
            Guard.NotNullOrEmpty(message);

            log.LogInformation("User log for app {appId} from system {system}: {message}.", appId, system, message);

            return collector.AddAsync(appId, $"{system.ToUpperInvariant()}: {message}");
        }

        public Task LogAsync(string appId, string message)
        {
            Guard.NotNullOrEmpty(appId);
            Guard.NotNullOrEmpty(message);

            log.LogInformation("User log for app {appId}: {message}.", appId, message);

            return collector.AddAsync(appId, message);
        }

        public Task<IResultList<LogEntry>> QueryAsync(string appId, LogQuery query,
            CancellationToken ct = default)
        {
            return repository.QueryAsync(appId, query, ct);
        }
    }
}
