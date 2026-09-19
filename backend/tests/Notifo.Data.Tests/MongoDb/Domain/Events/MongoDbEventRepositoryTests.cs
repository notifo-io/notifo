// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;
using Notifo.Domain.Shared;
using Notifo.Infrastructure.Fixtures;

namespace Notifo.Domain.Events.MongoDb;

[Trait("Category", "TestContainer")]
[Collection(MongoFixtureCollection.Name)]
public class MongoDbEventRepositoryTests(MongoFixture fixture) : EventRepositoryTests
{
    protected override async Task<IEventRepository> CreateSutAsync()
    {
        var sut = new MongoDbEventRepository(fixture.Database, Options.Create(new EventsOptions()));

        await sut.InitializeAsync(default);
        return sut;
    }
}
