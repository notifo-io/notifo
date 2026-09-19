// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.Integrations;
using Notifo.Domain.Templates;
using Notifo.Infrastructure.MongoDb;
using Notifo.Infrastructure.Texts;

namespace Notifo.Domain.Shared;

public abstract class TemplateRepositoryTests
{
    private readonly Instant now = Instant.FromUtc(2024, 1, 2, 10, 30, 0);
    private readonly string appId = Guid.NewGuid().ToString();

    protected abstract Task<ITemplateRepository> CreateSutAsync();

    [Fact]
    public async Task Should_insert_and_get_template()
    {
        var sut = await CreateSutAsync();

        var template = CreateTemplate("welcome");

        await sut.UpsertAsync(template);

        var (result, etag) = await sut.GetAsync(appId, template.Code);

        result.Should().BeEquivalentTo(template);
        Assert.NotNull(etag);
    }

    [Fact]
    public async Task Should_return_null_if_template_not_found()
    {
        var sut = await CreateSutAsync();

        var (result, etag) = await sut.GetAsync(appId, "unknown");

        Assert.Null(result);
        Assert.Null(etag);
    }

    [Fact]
    public async Task Should_update_template_with_etag()
    {
        var sut = await CreateSutAsync();

        var template = CreateTemplate("welcome");

        await sut.UpsertAsync(template);

        var (_, etag) = await sut.GetAsync(appId, template.Code);

        await sut.UpsertAsync(template with { IsAutoCreated = true }, etag);

        var (result, newEtag) = await sut.GetAsync(appId, template.Code);

        Assert.True(result!.IsAutoCreated);
        Assert.NotEqual(etag, newEtag);
    }

    [Fact]
    public async Task Should_throw_exception_if_etag_does_not_match()
    {
        var sut = await CreateSutAsync();

        var template = CreateTemplate("welcome");

        await sut.UpsertAsync(template);

        await Assert.ThrowsAsync<InconsistentStateException>(() => sut.UpsertAsync(template, "invalid"));
    }

    [Fact]
    public async Task Should_delete_template()
    {
        var sut = await CreateSutAsync();

        var template = CreateTemplate("welcome");

        await sut.UpsertAsync(template);
        await sut.DeleteAsync(appId, template.Code);

        var (result, _) = await sut.GetAsync(appId, template.Code);

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_not_delete_template_of_other_app()
    {
        var sut = await CreateSutAsync();

        var template = CreateTemplate("welcome");

        await sut.UpsertAsync(template);
        await sut.DeleteAsync(Guid.NewGuid().ToString(), template.Code);

        var (result, _) = await sut.GetAsync(appId, template.Code);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task Should_query_templates_by_code()
    {
        var sut = await CreateSutAsync();

        await sut.UpsertAsync(CreateTemplate("welcome"));
        await sut.UpsertAsync(CreateTemplate("goodbye"));

        var result = await sut.QueryAsync(appId, new TemplateQuery { Query = "WELCOME" });

        Assert.Equal(["welcome"], result.Select(x => x.Code));
    }

    [Fact]
    public async Task Should_query_templates_with_paging_and_total()
    {
        var sut = await CreateSutAsync();

        await sut.UpsertAsync(CreateTemplate("template1"));
        await sut.UpsertAsync(CreateTemplate("template2"));
        await sut.UpsertAsync(CreateTemplate("template3"));

        var result = await sut.QueryAsync(appId, new TemplateQuery { Take = 2 });

        Assert.Equal(2, result.Count);
        Assert.Equal(3, result.Total);
    }

    [Fact]
    public async Task Should_not_query_templates_of_other_app()
    {
        var sut = await CreateSutAsync();

        await sut.UpsertAsync(CreateTemplate("welcome") with { AppId = Guid.NewGuid().ToString() });

        var result = await sut.QueryAsync(appId, new TemplateQuery());

        Assert.Empty(result);
    }

    private Template CreateTemplate(string code)
    {
        return new Template(appId, code, now)
        {
            LastUpdate = now,
            Formatting = new NotificationFormatting<LocalizedText>
            {
                Subject = new LocalizedText
                {
                    ["en"] = "Subject"
                }
            },
            Settings = new ChannelSettings
            {
                [Providers.Email] = new ChannelSetting
                {
                    Send = ChannelSend.Send
                }
            }
        };
    }
}
