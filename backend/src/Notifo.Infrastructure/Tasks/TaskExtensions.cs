// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Channels;

namespace Notifo.Infrastructure.Tasks
{
    public static class TaskExtensions
    {
        private static readonly Action<Task> IgnoreTaskContinuation = t => { var ignored = t.Exception; };

        public static void Forget(this Task task)
        {
            if (task.IsCompleted)
            {
#pragma warning disable IDE0059 // Unnecessary assignment of a value
                var ignored = task.Exception;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
            }
            else
            {
                task.ContinueWith(
                    IgnoreTaskContinuation,
                    default,
                    TaskContinuationOptions.OnlyOnFaulted |
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
            }
        }

        public static Func<TInput, TOutput> ToDefault<TInput, TOutput>(this Action<TInput> action)
        {
            Guard.NotNull(action);

            return x =>
            {
                action(x);

                return default!;
            };
        }

        public static Func<TInput, Task<TOutput>> ToDefault<TInput, TOutput>(this Func<TInput, Task> action)
        {
            Guard.NotNull(action);

            return async x =>
            {
                await action(x);

                return default!;
            };
        }

        public static Func<TInput, Task<TOutput>> ToAsync<TInput, TOutput>(this Func<TInput, TOutput> action)
        {
            Guard.NotNull(action);

            return x =>
            {
                var result = action(x);

                return Task.FromResult(result);
            };
        }

        public static Func<TInput, Task> ToAsync<TInput>(this Action<TInput> action)
        {
            return x =>
            {
                action(x);

                return Task.CompletedTask;
            };
        }

        public static async Task<T> WithCancellation<T>(this Task<T> task,
            CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

            await using (cancellationToken.Register(state =>
            {
                ((TaskCompletionSource<object>)state!).TrySetResult(null!);
            },
            tcs))
            {
                var resultTask = await Task.WhenAny(task, tcs.Task);
                if (resultTask == tcs.Task)
                {
                    throw new OperationCanceledException(cancellationToken);
                }

                return await task;
            }
        }

        public static void Batch<TIn, TOut>(this Channel<object> source, Channel<TOut> target, Func<IReadOnlyList<TIn>, TOut> converter, int batchSize, int timeout,
            CancellationToken ct = default)
        {
            Task.Run(async () =>
            {
                var batch = new List<TIn>(batchSize);

                var force = new object();

                await using var timer = new Timer(_ => source.Writer.TryWrite(force));

                async Task TrySendAsync()
                {
                    if (batch.Count > 0)
                    {
                        await target.Writer.WriteAsync(converter(batch), ct);
                        batch.Clear();
                    }
                }

                await foreach (var item in source.Reader.ReadAllAsync(ct))
                {
                    if (ReferenceEquals(item, force))
                    {
                        await TrySendAsync();
                    }
                    else if (item is TIn typed)
                    {
                        timer.Change(timeout, Timeout.Infinite);

                        batch.Add(typed);

                        if (batch.Count >= batchSize)
                        {
                            await TrySendAsync();
                        }
                    }
                }

                await TrySendAsync();
            }, ct).ContinueWith(x => target.Writer.TryComplete(x.Exception));
        }
    }
}
