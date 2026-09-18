// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.Log;

namespace Notifo.Domain.Shared;

public abstract class LogRepositoryTests
{
    private readonly Instant now = Instant.FromUtc(2024, 1, 2, 10, 30, 0);
    private readonly string appId = Guid.NewGuid().ToString();

    protected abstract Task<ILogRepository> CreateSutAsync();

    [Fact]
    public async Task Should_create_entries_and_return_them_as_new()
    {
        var sut = await CreateSutAsync();

        var result = await sut.BatchWriteAsync(
        [
            (new LogWrite(appId, null, 1, "Message1", "System1"), 2, now),
            (new LogWrite(appId, "user1", 2, "Message2", "System2"), 3, now)
        ]);

        Assert.Equal(2, result.Total);

        var entry1 = result.Single(x => x.Message == "Message1");

        Assert.Equal(appId, entry1.AppId);
        Assert.Null(entry1.UserId);
        Assert.Equal(1, entry1.EventCode);
        Assert.Equal("System1", entry1.System);
        Assert.Equal(2, entry1.Count);
        Assert.Equal(now, entry1.FirstSeen);
        Assert.Equal(now, entry1.LastSeen);

        var entry2 = result.Single(x => x.Message == "Message2");

        Assert.Equal("user1", entry2.UserId);
        Assert.Equal(2, entry2.EventCode);
        Assert.Equal("System2", entry2.System);
        Assert.Equal(3, entry2.Count);
    }

    [Fact]
    public async Task Should_update_existing_entry_and_not_return_it_as_new()
    {
        var sut = await CreateSutAsync();

        var write = new LogWrite(appId, null, 1, "Message", "System");

        await sut.BatchWriteAsync(
        [
            (write, 2, now)
        ]);

        var later = now.Plus(Duration.FromMinutes(5));

        var result = await sut.BatchWriteAsync(
        [
            (write, 3, later)
        ]);

        Assert.Empty(result);

        var entry = (await sut.QueryAsync(appId, new LogQuery())).Single();

        Assert.Equal(5, entry.Count);
        Assert.Equal(now, entry.FirstSeen);
        Assert.Equal(later, entry.LastSeen);
    }

    [Fact]
    public async Task Should_merge_same_entries_within_one_batch()
    {
        var sut = await CreateSutAsync();

        var write = new LogWrite(appId, null, 1, "Message", "System");

        var result = await sut.BatchWriteAsync(
        [
            (write, 2, now),
            (write, 3, now)
        ]);

        var entry = Assert.Single(result);

        Assert.Equal(5, entry.Count);
    }

    [Fact]
    public async Task Should_derive_system_from_message_if_not_set()
    {
        var sut = await CreateSutAsync();

        var result = await sut.BatchWriteAsync(
        [
            (new LogWrite(appId, null, 1, "Email: Failed to send", string.Empty), 1, now)
        ]);

        Assert.Equal("Email", result.Single().System);
    }

    [Fact]
    public async Task Should_query_app_entries_without_user_by_default()
    {
        var sut = await CreateSutAsync();

        await sut.BatchWriteAsync(
        [
            (new LogWrite(appId, null, 1, "AppMessage", "System"), 1, now),
            (new LogWrite(appId, "user1", 1, "UserMessage", "System"), 1, now)
        ]);

        var result = await sut.QueryAsync(appId, new LogQuery());

        Assert.Equal(["AppMessage"], result.Select(x => x.Message));
    }

    [Fact]
    public async Task Should_query_entries_by_user()
    {
        var sut = await CreateSutAsync();

        await sut.BatchWriteAsync(
        [
            (new LogWrite(appId, null, 1, "AppMessage", "System"), 1, now),
            (new LogWrite(appId, "user1", 1, "UserMessage1", "System"), 1, now),
            (new LogWrite(appId, "user2", 1, "UserMessage2", "System"), 1, now)
        ]);

        var result = await sut.QueryAsync(appId, new LogQuery { UserId = "user1" });

        Assert.Equal(["UserMessage1"], result.Select(x => x.Message));
    }

    [Fact]
    public async Task Should_query_entries_by_text()
    {
        var sut = await CreateSutAsync();

        await sut.BatchWriteAsync(
        [
            (new LogWrite(appId, null, 1, "Failed to send email", "System"), 1, now),
            (new LogWrite(appId, null, 1, "Failed to send sms", "System"), 1, now)
        ]);

        var result = await sut.QueryAsync(appId, new LogQuery { Query = "EMAIL" });

        Assert.Equal(["Failed to send email"], result.Select(x => x.Message));
    }

    [Fact]
    public async Task Should_query_entries_by_systems()
    {
        var sut = await CreateSutAsync();

        await sut.BatchWriteAsync(
        [
            (new LogWrite(appId, null, 1, "Message1", "Email"), 1, now),
            (new LogWrite(appId, null, 1, "Message2", "Sms"), 1, now),
            (new LogWrite(appId, null, 1, "Message3", "WebPush"), 1, now)
        ]);

        var result = await sut.QueryAsync(appId, new LogQuery { Systems = ["Email", "Sms"] });

        Assert.Equal(["Message1", "Message2"], result.Select(x => x.Message).Order());
    }

    [Fact]
    public async Task Should_query_entries_by_systems_derived_from_message()
    {
        var sut = await CreateSutAsync();

        await sut.BatchWriteAsync(
        [
            (new LogWrite(appId, null, 1, "Email: Failed to send", string.Empty), 1, now),
            (new LogWrite(appId, null, 1, "Sms: Failed to send", string.Empty), 1, now)
        ]);

        var result = await sut.QueryAsync(appId, new LogQuery { Systems = ["Email"] });

        Assert.Equal(["Email: Failed to send"], result.Select(x => x.Message));
    }

    [Fact]
    public async Task Should_query_entries_by_event_code()
    {
        var sut = await CreateSutAsync();

        await sut.BatchWriteAsync(
        [
            (new LogWrite(appId, null, 1, "Message1", "System"), 1, now),
            (new LogWrite(appId, null, 2, "Message2", "System"), 1, now)
        ]);

        var result = await sut.QueryAsync(appId, new LogQuery { EventCode = 2 });

        Assert.Equal(["Message2"], result.Select(x => x.Message));
    }

    [Fact]
    public async Task Should_query_entries_sorted_by_last_seen()
    {
        var sut = await CreateSutAsync();

        await sut.BatchWriteAsync(
        [
            (new LogWrite(appId, null, 1, "Message1", "System"), 1, now.Plus(Duration.FromMinutes(1))),
            (new LogWrite(appId, null, 1, "Message2", "System"), 1, now.Plus(Duration.FromMinutes(3))),
            (new LogWrite(appId, null, 1, "Message3", "System"), 1, now.Plus(Duration.FromMinutes(2)))
        ]);

        var result = await sut.QueryAsync(appId, new LogQuery());

        Assert.Equal(["Message2", "Message3", "Message1"], result.Select(x => x.Message));
    }

    [Fact]
    public async Task Should_query_entries_with_paging_and_total()
    {
        var sut = await CreateSutAsync();

        await sut.BatchWriteAsync(
        [
            (new LogWrite(appId, null, 1, "Message1", "System"), 1, now),
            (new LogWrite(appId, null, 1, "Message2", "System"), 1, now),
            (new LogWrite(appId, null, 1, "Message3", "System"), 1, now)
        ]);

        var result = await sut.QueryAsync(appId, new LogQuery { Take = 2 });

        Assert.Equal(2, result.Count);
        Assert.Equal(3, result.Total);
    }

    [Fact]
    public async Task Should_not_query_entries_of_other_app()
    {
        var sut = await CreateSutAsync();

        await sut.BatchWriteAsync(
        [
            (new LogWrite(Guid.NewGuid().ToString(), null, 1, "Message", "System"), 1, now)
        ]);

        var result = await sut.QueryAsync(appId, new LogQuery());

        Assert.Empty(result);
    }
}
