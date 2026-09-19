// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Shared;
using Notifo.Infrastructure.Fixtures;

namespace Notifo.Domain.Users.MongoDb;

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
