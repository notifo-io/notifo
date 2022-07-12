// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Notifo.Infrastructure.Tasks;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
#pragma warning disable RECS0082 // Parameter has the same name as a member and hides it

namespace Notifo.Domain.Counters
{
    public sealed class CounterCollector<T> : IAsyncDisposable where T : notnull
    {
        private readonly ICounterStore<T> store;
        private readonly ILogger log;
        private readonly Channel<object> inputQueue;
        private readonly Channel<Job[]> writeQueue;
        private readonly Task task;

        private sealed record Job(T Key, CounterMap Counters);

        public CounterCollector(ICounterStore<T> store, ILogger log,
            int capacity = 2000,
            int batchSize = 200,
            int batchDelay = 1000)
        {
            this.store = store;

            inputQueue = Channel.CreateBounded<object>(new BoundedChannelOptions(capacity)
            {
                AllowSynchronousContinuations = true,
                SingleReader = true,
                SingleWriter = false
            });

            writeQueue = Channel.CreateBounded<Job[]>(new BoundedChannelOptions(batchSize)
            {
                AllowSynchronousContinuations = true,
                SingleReader = true,
                SingleWriter = true
            });

            inputQueue.Batch<Job, Job[]>(writeQueue, x => x.ToArray(), batchSize, batchDelay);

            task = Task.Run(Process);

            this.log = log;
        }

        public ValueTask AddAsync(T key, CounterMap counters,
            CancellationToken ct = default)
        {
            return inputQueue.Writer.WriteAsync(new Job(key, counters), ct);
        }

        public async ValueTask DisposeAsync()
        {
            inputQueue.Writer.TryComplete();

            await task;
        }

        private async Task Process()
        {
            try
            {
                await foreach (var batch in writeQueue.Reader.ReadAllAsync())
                {
                    if (batch.Length == 0)
                    {
                        continue;
                    }

                    try
                    {
                        var commands = new List<(T, CounterMap)>();

                        foreach (var group in batch.GroupBy(x => x.Key))
                        {
                            if (group.Count() == 1)
                            {
                                commands.Add((group.Key, group.First().Counters));
                            }
                            else
                            {
                                var merged = new CounterMap();

                                foreach (var item in group)
                                {
                                    foreach (var (key, value) in item.Counters)
                                    {
                                        merged.Increment(key, value);
                                    }
                                }

                                commands.Add((group.Key, merged));
                            }
                        }

                        if (commands.Count > 0)
                        {
                            await store.BatchWriteAsync(commands, default);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, "Failed to writer counters.");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}
