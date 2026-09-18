// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure.Fixtures;

namespace Notifo.Identity.Fixtures;

[CollectionDefinition(Name)]
public sealed class MongoFixtureCollection : ICollectionFixture<MongoFixture>
{
    public const string Name = "Mongo";
}
