// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.Channels.Sms;
using Notifo.Domain.ChannelTemplates;
using Notifo.Infrastructure.Collections;
using Notifo.Infrastructure.MongoDb;

namespace Notifo.Domain.Shared;

public abstract class ChannelTemplateRepositoryTests
{
    private readonly Instant now = Instant.FromUtc(2024, 1, 2, 10, 30, 0);
    private readonly string appId = Guid.NewGuid().ToString();

    protected abstract Task<IChannelTemplateRepository<SmsTemplate>> CreateSutAsync();

    [Fact]
    public async Task Should_insert_and_get_template()
    {
        var sut = await CreateSutAsync();

        var template = CreateTemplate("template1");

        await sut.UpsertAsync(template);

        var (result, etag) = await sut.GetAsync(appId, template.Id);

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

        var template = CreateTemplate("template1");

        await sut.UpsertAsync(template);

        var (_, etag) = await sut.GetAsync(appId, template.Id);

        await sut.UpsertAsync(template with { Primary = true }, etag);

        var (result, newEtag) = await sut.GetAsync(appId, template.Id);

        Assert.True(result!.Primary);
        Assert.NotEqual(etag, newEtag);
    }

    [Fact]
    public async Task Should_throw_exception_if_etag_does_not_match()
    {
        var sut = await CreateSutAsync();

        var template = CreateTemplate("template1");

        await sut.UpsertAsync(template);

        await Assert.ThrowsAsync<InconsistentStateException>(() => sut.UpsertAsync(template, "invalid"));
    }

    [Fact]
    public async Task Should_delete_template()
    {
        var sut = await CreateSutAsync();

        var template = CreateTemplate("template1");

        await sut.UpsertAsync(template);
        await sut.DeleteAsync(appId, template.Id);

        var (result, _) = await sut.GetAsync(appId, template.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_not_delete_template_of_other_app()
    {
        var sut = await CreateSutAsync();

        var template = CreateTemplate("template1");

        await sut.UpsertAsync(template);
        await sut.DeleteAsync(Guid.NewGuid().ToString(), template.Id);

        var (result, _) = await sut.GetAsync(appId, template.Id);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task Should_query_templates_by_name()
    {
        var sut = await CreateSutAsync();

        await sut.UpsertAsync(CreateTemplate("template1", "Welcome"));
        await sut.UpsertAsync(CreateTemplate("template2", "Goodbye"));

        var result = await sut.QueryAsync(appId, new ChannelTemplateQuery { Query = "WELCOME" });

        Assert.Equal(["template1"], result.Select(x => x.Id));
    }

    [Fact]
    public async Task Should_query_templates_with_paging_and_total()
    {
        var sut = await CreateSutAsync();

        await sut.UpsertAsync(CreateTemplate("template1"));
        await sut.UpsertAsync(CreateTemplate("template2"));
        await sut.UpsertAsync(CreateTemplate("template3"));

        var result = await sut.QueryAsync(appId, new ChannelTemplateQuery { Take = 2 });

        Assert.Equal(2, result.Count);
        Assert.Equal(3, result.Total);
    }

    [Fact]
    public async Task Should_not_query_templates_of_other_app()
    {
        var sut = await CreateSutAsync();

        await sut.UpsertAsync(CreateTemplate("template1") with { AppId = Guid.NewGuid().ToString() });

        var result = await sut.QueryAsync(appId, new ChannelTemplateQuery());

        Assert.Empty(result);
    }

    [Fact]
    public async Task Should_get_best_template_by_name()
    {
        var sut = await CreateSutAsync();

        var template = CreateTemplate("template2", "Name2");

        await sut.UpsertAsync(CreateTemplate("template1", "Name1") with { Primary = true });
        await sut.UpsertAsync(template);

        var result = await sut.GetBestAsync(appId, "Name2");

        result.Should().BeEquivalentTo(template);
    }

    [Fact]
    public async Task Should_not_get_best_template_if_name_not_found()
    {
        var sut = await CreateSutAsync();

        await sut.UpsertAsync(CreateTemplate("template1", "Name1") with { Primary = true });

        var result = await sut.GetBestAsync(appId, "Unknown");

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_get_primary_template_if_name_not_defined()
    {
        var sut = await CreateSutAsync();

        var template = CreateTemplate("template2", "Name2") with { Primary = true };

        await sut.UpsertAsync(CreateTemplate("template1", "Name1"));
        await sut.UpsertAsync(template);

        var result = await sut.GetBestAsync(appId, null);

        result.Should().BeEquivalentTo(template);
    }

    [Fact]
    public async Task Should_get_single_template_if_name_not_defined_and_no_primary_template_exists()
    {
        var sut = await CreateSutAsync();

        var template = CreateTemplate("template1", "Name1");

        await sut.UpsertAsync(template);

        var result = await sut.GetBestAsync(appId, " ");

        result.Should().BeEquivalentTo(template);
    }

    [Fact]
    public async Task Should_not_get_best_template_if_multiple_templates_exist_without_primary()
    {
        var sut = await CreateSutAsync();

        await sut.UpsertAsync(CreateTemplate("template1", "Name1"));
        await sut.UpsertAsync(CreateTemplate("template2", "Name2"));

        var result = await sut.GetBestAsync(appId, null);

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_not_get_best_template_of_other_app()
    {
        var sut = await CreateSutAsync();

        await sut.UpsertAsync(CreateTemplate("template1", "Name1") with { AppId = Guid.NewGuid().ToString(), Primary = true });

        var result = await sut.GetBestAsync(appId, null);

        Assert.Null(result);
    }

    private ChannelTemplate<SmsTemplate> CreateTemplate(string id, string? name = null)
    {
        return new ChannelTemplate<SmsTemplate>(appId, id, now)
        {
            Name = name,
            LastUpdate = now,
            Languages = new Dictionary<string, SmsTemplate>
            {
                ["en"] = new SmsTemplate
                {
                    Text = "Text"
                }
            }.ToReadonlyDictionary()
        };
    }
}
