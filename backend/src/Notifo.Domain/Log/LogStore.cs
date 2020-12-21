// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using NodaTime;
using Notifo.Domain.Log.Internal;
using Notifo.Infrastructure;

namespace Notifo.Domain.Log
{
    public sealed class LogStore : ILogStore, IDisposable
    {
        private readonly LogCollector collector;
        private readonly ILogRepository repository;

        public LogStore(ILogRepository repository, IClock clock)
        {
            this.repository = repository;

            collector = new LogCollector(repository, clock, 10, 3000);
        }

        public void Dispose()
        {
            collector.StopAsync().Wait();
        }

        public Task LogAsync(string appId, string message, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId, nameof(appId));
            Guard.NotNullOrEmpty(message, nameof(message));

            return collector.AddAsync(appId, message);
        }

        public Task<IResultList<LogEntry>> QueryAsync(string appId, LogQuery query, CancellationToken ct = default)
        {
            return repository.QueryAsync(appId, query, ct);
        }
    }
}
