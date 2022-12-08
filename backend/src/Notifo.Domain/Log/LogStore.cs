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
        Guard.NotNullOrEmpty(message.System);
        Guard.NotNullOrEmpty(message.Text);

        var (eventCode, text, system) = message;

        if (message.FormatText != null)
        {
            var argCount = message.FormatArgs?.Length ?? 0;

            var args = new object?[argCount + 2];

            args[0] = appId;
            args[1] = system;

            if (message.FormatArgs != null)
            {
                Array.Copy(message.FormatArgs, 0, args, 2, message.FormatArgs.Length);
            }

#pragma warning disable CA2254 // Template should be a static expression
            log.LogInformation(message.EventCode, message.Exception, $"User log for app {{appId}} from system {{system}}: {text}.", args);
#pragma warning restore CA2254 // Template should be a static expression
        }

        var combinedText = $"{system.ToUpperInvariant()}: {text}";

        return collector.AddAsync(appId, eventCode, combinedText, system);
    }

    public Task<IResultList<LogEntry>> QueryAsync(string appId, LogQuery query,
        CancellationToken ct = default)
    {
        return repository.QueryAsync(appId, query, ct);
    }
}
