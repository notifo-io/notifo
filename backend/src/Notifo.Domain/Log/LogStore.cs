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
                foreach (var entry in newEntries)
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

    public Task LogAsync(string appId, LogMessage message)
    {
        Guard.NotNullOrEmpty(appId);
        Guard.NotNullOrEmpty(message.System);
        Guard.NotNullOrEmpty(message.Message);

        var (eventCode, text, system) = message;

        if (message.FormatText != null)
        {
            var argCount = message.FormatArgs?.Length ?? 0;

            var args = new object?[argCount + 2];

            args[0] = appId;
            args[1] = message.System;

            if (message.FormatArgs != null)
            {
                Array.Copy(message.FormatArgs, 0, args, 2, message.FormatArgs.Length);
            }

            log.LogInformation(message.EventCode, message.Exception, $"User log for app {{appId}} from system {{system}}: {text}.", args);
        }

        return collector.AddAsync(new LogWrite(appId, null, eventCode, text, system));
    }

    public Task LogAsync(string appId, string userId, LogMessage message)
    {
        Guard.NotNullOrEmpty(appId);
        Guard.NotNullOrEmpty(userId);
        Guard.NotNullOrEmpty(message.System);
        Guard.NotNullOrEmpty(message.Message);

        var (eventCode, text, system) = message;

        if (message.FormatText != null)
        {
            var argCount = message.FormatArgs?.Length ?? 0;

            var args = new object?[argCount + 3];

            args[0] = appId;
            args[1] = userId;
            args[2] = message.System;

            if (message.FormatArgs != null)
            {
                Array.Copy(message.FormatArgs, 0, args, 3, message.FormatArgs.Length);
            }

            log.LogInformation(message.EventCode, message.Exception, $"User log for app {{appId}} and user {{userId}} from system {{system}}: {message.Message}.", args);
        }

        return collector.AddAsync(new LogWrite(appId, userId, eventCode, text, system));
    }

    public Task<IResultList<LogEntry>> QueryAsync(string appId, LogQuery query,
        CancellationToken ct = default)
    {
        return repository.QueryAsync(appId, query, ct);
    }
}
