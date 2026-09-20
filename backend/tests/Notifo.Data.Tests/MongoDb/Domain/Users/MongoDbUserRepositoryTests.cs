// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Users;
using Notifo.MongoDb.TestHelpers;
using Notifo.Shared;

namespace Notifo.MongoDb.Domain.Users;

[Trait("Category", "TestContainer")]
[Collection(MongoFixtureCollection.Name)]
public class MongoDbUserRepositoryTests(MongoFixture fixture) : UserRepositoryTests
{
    protected override async Task<IUserRepository> CreateSutAsync()
    {
        var sut = new MongoDbUserRepository(fixture.Database);

        await sut.InitializeAsync(default);
        return sut;
    }
}
