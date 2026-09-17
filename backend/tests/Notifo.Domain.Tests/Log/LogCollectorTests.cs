// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.Log.Internal;
using Notifo.Infrastructure;

namespace Notifo.Domain.Log;

public class LogCollectorTests
{
    private readonly ILogRepository repository = A.Fake<ILogRepository>();
    private readonly List<LogWrite> writes = [];

    public LogCollectorTests()
    {
        A.CallTo(() => repository.BatchWriteAsync(A<IEnumerable<(LogWrite Write, int Count, Instant Now)>>._, A<CancellationToken>._))
            .Invokes(x =>
            {
                var updates = x.GetArgument<IEnumerable<(LogWrite Write, int Count, Instant Now)>>(0)!;

                writes.AddRange(updates.Select(u => u.Write));
            })
            .Returns(ResultList.Empty<LogEntry>());
    }

    [Fact]
    public async Task Should_write_remaining_entries_on_stop()
    {
        var sut = CreateSut(capacity: 100);

        await sut.AddAsync(CreateWrite("message1"));
        await sut.StopAsync();

        Assert.Equal(["message1"], writes.Select(x => x.Message));
    }

    [Fact]
    public async Task Should_write_failed_entries_again()
    {
        var sut = CreateSut(capacity: 1);

        A.CallTo(() => repository.BatchWriteAsync(A<IEnumerable<(LogWrite Write, int Count, Instant Now)>>._, A<CancellationToken>._))
            .Throws(new InvalidOperationException()).Once();

        await sut.AddAsync(CreateWrite("message1"));
        await sut.AddAsync(CreateWrite("message2"));

        await sut.StopAsync();

        Assert.Equal(["message1", "message2"], writes.Select(x => x.Message).Order());
    }

    private LogCollector CreateSut(int capacity)
    {
        return new LogCollector(repository, SystemClock.Instance, 60_000, capacity);
    }

    private static LogWrite CreateWrite(string message)
    {
        return new LogWrite("app", "user", 0, message, "System");
    }
}
