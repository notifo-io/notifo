// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.Log.Internal;
using Notifo.Infrastructure;
using Squidex.Log;

namespace Notifo.Domain.Log
{
    public sealed class LogStore : ILogStore, IDisposable
    {
        private readonly LogCollector collector;
        private readonly ILogRepository repository;
        private readonly ISemanticLog log;

        public LogStore(ILogRepository repository, ISemanticLog log, IClock clock)
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

            log.LogInformation(w => w
                .WriteProperty("action", "UserLog")
                .WriteProperty("appId", appId)
                .WriteProperty("system", system)
                .WriteProperty("message", message));

            return collector.AddAsync(appId, $"{system.ToUpperInvariant()}: {message}");
        }

        public Task LogAsync(string appId, string message)
        {
            Guard.NotNullOrEmpty(appId);
            Guard.NotNullOrEmpty(message);

            log.LogInformation(w => w
                .WriteProperty("action", "UserLog")
                .WriteProperty("appId", appId)
                .WriteProperty("system", string.Empty)
                .WriteProperty("message", message));

            return collector.AddAsync(appId, message);
        }

        public Task<IResultList<LogEntry>> QueryAsync(string appId, LogQuery query,
            CancellationToken ct = default)
        {
            return repository.QueryAsync(appId, query, ct);
        }
    }
}
