// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Channels.Sms;
using Notifo.Domain.Shared;
using Notifo.Infrastructure.Fixtures;

namespace Notifo.Domain.ChannelTemplates.MongoDb;

[Trait("Category", "TestContainer")]
[Collection(MongoFixtureCollection.Name)]
public class MongoDbChannelTemplateRepositoryTests(MongoFixture fixture) : ChannelTemplateRepositoryTests
{
    protected override async Task<IChannelTemplateRepository<SmsTemplate>> CreateSutAsync()
    {
        var sut = new MongoDbChannelTemplateRepository<SmsTemplate>(fixture.Database);

        await sut.InitializeAsync(default);
        return sut;
    }
}
