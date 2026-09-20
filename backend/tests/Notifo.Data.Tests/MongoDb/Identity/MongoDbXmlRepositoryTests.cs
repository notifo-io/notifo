// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.DataProtection.Repositories;
using Notifo.Identity;
using Notifo.MongoDb.TestHelpers;
using Notifo.Shared;

namespace Notifo.MongoDb.Identity;

[Trait("Category", "TestContainer")]
[Collection(MongoFixtureCollection.Name)]
public class MongoDbXmlRepositoryTests(MongoFixture fixture) : XmlRepositoryTests
{
    protected override Task<IXmlRepository> CreateSutAsync()
    {
        // The repository initializes itself in the constructor.
        var sut = new MongoDbXmlRepository(fixture.Database);

        return Task.FromResult<IXmlRepository>(sut);
    }
}
