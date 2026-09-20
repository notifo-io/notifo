// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.Log;
using Notifo.MongoDb.TestHelpers;
using Notifo.Shared;

namespace Notifo.MongoDb.Domain.Log;

[Trait("Category", "TestContainer")]
[Collection(MongoFixtureCollection.Name)]
public class MongoDbLogRepositoryTests(MongoFixture fixture) : LogRepositoryTests
{
    private readonly Instant now = Instant.FromUtc(2024, 1, 2, 10, 30, 0);
    private readonly string appId = Guid.NewGuid().ToString();

    protected override async Task<ILogRepository> CreateSutAsync()
    {
        var sut = new MongoDbLogRepository(fixture.Database);

        await sut.InitializeAsync(default);
        return sut;
    }

    [Fact]
    public async Task Should_derive_system_from_message_of_old_entry()
    {
        var sut = await CreateSutAsync();

        // Older entries have no system, but the message is prefixed with the system.
        var result = await sut.BatchWriteAsync(
        [
            (new LogWrite(appId, null, 1, "Email: Failed to send", string.Empty), 1, now)
        ]);

        Assert.Equal("Email", result.Single().System);
    }

    [Fact]
    public async Task Should_query_old_entries_by_systems_derived_from_message()
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
}
