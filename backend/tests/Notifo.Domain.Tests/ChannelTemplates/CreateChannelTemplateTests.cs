// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Notifo.Infrastructure;

namespace Notifo.Domain.ChannelTemplates;

public class CreateChannelTemplateTests
{
    private readonly IChannelTemplateRepository<string> repository = A.Fake<IChannelTemplateRepository<string>>();
    private readonly IServiceProvider serviceProvider;
    private readonly CreateChannelTemplate<string> sut = new CreateChannelTemplate<string> { AppId = "app" };

    public CreateChannelTemplateTests()
    {
        serviceProvider =
            new ServiceCollection()
                .AddSingleton(repository)
                .AddSingleton(A.Fake<IChannelTemplateFactory<string>>())
                .BuildServiceProvider();
    }

    [Fact]
    public async Task Should_make_first_template_primary()
    {
        A.CallTo(() => repository.QueryAsync("app", A<ChannelTemplateQuery>._, A<CancellationToken>._))
            .Returns(ResultList.Empty<ChannelTemplate<string>>());

        var template = await sut.ExecuteAsync(CreateTemplate(), serviceProvider, default);

        Assert.True(template!.Primary);
    }

    [Fact]
    public async Task Should_not_make_other_template_primary()
    {
        A.CallTo(() => repository.QueryAsync("app", A<ChannelTemplateQuery>._, A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, CreateTemplate()));

        var template = await sut.ExecuteAsync(CreateTemplate(), serviceProvider, default);

        Assert.False(template!.Primary);
    }

    private static ChannelTemplate<string> CreateTemplate()
    {
        return new ChannelTemplate<string>("app", Guid.NewGuid().ToString(), SystemClock.Instance.GetCurrentInstant());
    }
}
