// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;
using Notifo.Domain.Events;
using Notifo.MongoDb.TestHelpers;
using Notifo.Shared;

namespace Notifo.MongoDb.Domain.Events;

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
