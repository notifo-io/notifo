// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Apps;
using Notifo.Identity;
using Notifo.Identity.Dynamic;
using Notifo.MongoDb.TestHelpers;
using Notifo.Shared;

namespace Notifo.MongoDb.Identity;

[Trait("Category", "TestContainer")]
[Collection(MongoFixtureCollection.Name)]
public class MongoDbConfigurationStoreTests(MongoFixture fixture) : ConfigurationStoreTests
{
    protected override async Task<IConfigurationStore<AppAuthScheme>> CreateSutAsync()
    {
        var sut = new MongoDbConfigurationStore<AppAuthScheme>(fixture.Database);

        await sut.InitializeAsync(default);
        return sut;
    }
}
