// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Notifo.Domain.Channels.Sms;
using Notifo.Domain.ChannelTemplates;
using Notifo.EntityFramework.TestHelpers;
using Notifo.Shared;

namespace Notifo.EntityFramework.Domain.ChannelTemplates;

public abstract class EFChannelTemplateRepositoryTests<TContext>(ISqlFixture<TContext> fixture) : ChannelTemplateRepositoryTests where TContext : DbContext
{
    protected override Task<IChannelTemplateRepository<SmsTemplate>> CreateSutAsync()
    {
        var sut = new EFChannelTemplateRepository<TContext, SmsTemplate>(fixture.DbContextFactory, TestJson.Options);

        return Task.FromResult<IChannelTemplateRepository<SmsTemplate>>(sut);
    }
}
