// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Channels.Sms;
using Notifo.Domain.ChannelTemplates;
using Notifo.MongoDb.TestHelpers;
using Notifo.Shared;

namespace Notifo.MongoDb.Domain.ChannelTemplates;

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
