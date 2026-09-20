// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure.KeyValueStore;
using Notifo.MongoDb.TestHelpers;
using Notifo.Shared;

namespace Notifo.MongoDb.Infrastructure;

[Trait("Category", "TestContainer")]
[Collection(MongoFixtureCollection.Name)]
public class MongoDbKeyValueStoreTests(MongoFixture fixture) : KeyValueStoreTests
{
    protected override async Task<IKeyValueStore> CreateSutAsync()
    {
        var sut = new MongoDbKeyValueStore(fixture.Database);

        await sut.InitializeAsync(default);
        return sut;
    }
}
