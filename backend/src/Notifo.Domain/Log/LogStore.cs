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
using Notifo.Infrastructure.Mediator;
using Notifo.Infrastructure.Tasks;

#pragma warning disable CA2254 // Template should be a static expression

namespace Notifo.Domain.Log;

public sealed class LogStore : ILogStore, IDisposable
{
    private readonly LogCollector collector;
    private readonly ILogRepository repository;
    private readonly ILogger<LogStore> log;

    public LogStore(ILogRepository repository, IMediator mediator,
        ILogger<LogStore> log, IClock clock)
    {
        this.repository = repository;

        collector = new LogCollector(repository, clock, 10, 3000)
        {
            OnNewEntries = newEntries =>
            {
                // Only create notifications for app logs.
                foreach (var entry in newEntries.Where(x => x.UserId == null))
                {
                    var notification = new FirstLogCreated
                    {
                        Entry = entry
                    };

                    mediator.PublishAsync(notification, default).Forget();
                }
            }
        };
        this.log = log;
    }

    public void Dispose()
    {
        collector.StopAsync().Wait();
    }

    public Task LogAsync(string appId, LogMessage message, bool skipDefaultLog = false)
    {
        Guard.NotNullOrEmpty(appId);
        Guard.NotNullOrEmpty(message.System);
        Guard.NotNullOrEmpty(message.Message);

        var (eventCode, text, system) = message;

        if (message.FormatText != null && !skipDefaultLog)
        {
            var args = new object[] { appId, message.System };

            LogInternal(message, $"User log for app {{appId}} from system {{system}}: {text}.", args);
        }

        return collector.AddAsync(new LogWrite(appId, null, eventCode, text, system));
    }

    public Task LogAsync(string appId, string userId, LogMessage message, bool skipDefaultLog = false)
    {
        Guard.NotNullOrEmpty(appId);
        Guard.NotNullOrEmpty(userId);
        Guard.NotNullOrEmpty(message.System);
        Guard.NotNullOrEmpty(message.Message);

        var (eventCode, text, system) = message;

        if (message.FormatText != null && !skipDefaultLog)
        {
            var args = new object[] { appId, userId, message.System };

            LogInternal(message, $"User log for app {{appId}} and user {{userId}} from system {{system}}: {text}.", args);
        }

        return collector.AddAsync(new LogWrite(appId, userId, eventCode, text, system));
    }

    public Task<IResultList<LogEntry>> QueryAsync(string appId, LogQuery query,
        CancellationToken ct = default)
    {
        return repository.QueryAsync(appId, query, ct);
    }

    private void LogInternal(LogMessage message, string text, params object[] systemArgs)
    {
        log.LogInformation(message.EventCode, message.Exception, text, systemArgs.Concat(message.FormatArgs));
    }
}
