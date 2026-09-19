// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.Apps;
using Notifo.Domain.Counters;
using Notifo.Domain.Integrations;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Collections;
using Notifo.Infrastructure.MongoDb;

namespace Notifo.Domain.Shared;

public abstract class AppRepositoryTests
{
    private readonly Instant now = Instant.FromUtc(2024, 1, 2, 10, 30, 0);
    private readonly string appId = Guid.NewGuid().ToString();

    protected abstract Task<IAppRepository> CreateSutAsync();

    [Fact]
    public async Task Should_insert_and_get_app()
    {
        var sut = await CreateSutAsync();

        var app = CreateApp(appId) with
        {
            ConfirmUrl = "https://confirm.io",
            Languages = ReadonlyList.Create("en", "de"),
            Integrations = new Dictionary<string, ConfiguredIntegration>
            {
                ["integration1"] = CreateIntegration(IntegrationStatus.Verified)
            }.ToReadonlyDictionary()
        };

        await sut.UpsertAsync(app);

        var (result, etag) = await sut.GetAsync(appId);

        result.Should().BeEquivalentTo(app);
        Assert.NotNull(etag);
    }

    [Fact]
    public async Task Should_return_null_if_app_not_found()
    {
        var sut = await CreateSutAsync();

        var (result, etag) = await sut.GetAsync(appId);

        Assert.Null(result);
        Assert.Null(etag);
    }

    [Fact]
    public async Task Should_update_app_with_etag()
    {
        var sut = await CreateSutAsync();

        var app = CreateApp(appId);

        await sut.UpsertAsync(app);

        var (_, etag) = await sut.GetAsync(appId);

        await sut.UpsertAsync(app with { Name = "Updated" }, etag);

        var (result, newEtag) = await sut.GetAsync(appId);

        Assert.Equal("Updated", result!.Name);
        Assert.NotEqual(etag, newEtag);
    }

    [Fact]
    public async Task Should_throw_exception_if_etag_does_not_match()
    {
        var sut = await CreateSutAsync();

        var app = CreateApp(appId);

        await sut.UpsertAsync(app);

        await Assert.ThrowsAsync<InconsistentStateException>(() => sut.UpsertAsync(app, "invalid"));
    }

    [Fact]
    public async Task Should_not_insert_deleted_app_again_with_etag()
    {
        var sut = await CreateSutAsync();

        var app = CreateApp(appId);

        await sut.UpsertAsync(app);

        var (_, etag) = await sut.GetAsync(appId);

        await sut.DeleteAsync(appId);

        await Assert.ThrowsAsync<InconsistentStateException>(() => sut.UpsertAsync(app, etag));

        var (result, _) = await sut.GetAsync(appId);

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_delete_app()
    {
        var sut = await CreateSutAsync();

        await sut.UpsertAsync(CreateApp(appId));
        await sut.DeleteAsync(appId);

        var (result, _) = await sut.GetAsync(appId);

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_query_apps_by_contributor()
    {
        var sut = await CreateSutAsync();

        var contributorId = Guid.NewGuid().ToString();

        var app1 = CreateApp(appId) with
        {
            Contributors = new Dictionary<string, string>
            {
                [contributorId] = AppRoles.Owner
            }.ToReadonlyDictionary()
        };

        var app2 = CreateApp(Guid.NewGuid().ToString()) with
        {
            Contributors = new Dictionary<string, string>
            {
                [Guid.NewGuid().ToString()] = AppRoles.Owner
            }.ToReadonlyDictionary()
        };

        await sut.UpsertAsync(app1);
        await sut.UpsertAsync(app2);

        var result = await sut.QueryAsync(contributorId);

        Assert.Equal([app1.Id], result.Select(x => x.Id));
    }

    [Fact]
    public async Task Should_return_empty_list_if_contributor_has_no_apps()
    {
        var sut = await CreateSutAsync();

        var result = await sut.QueryAsync(Guid.NewGuid().ToString());

        Assert.Empty(result);
    }

    [Fact]
    public async Task Should_get_app_by_api_key()
    {
        var sut = await CreateSutAsync();

        var apiKey = Guid.NewGuid().ToString();

        var app = CreateApp(appId) with
        {
            ApiKeys = new Dictionary<string, string>
            {
                [apiKey] = AppRoles.Admin
            }.ToReadonlyDictionary()
        };

        await sut.UpsertAsync(app);

        var (result, etag) = await sut.GetByApiKeyAsync(apiKey);

        Assert.Equal(appId, result?.Id);
        Assert.NotNull(etag);
    }

    [Fact]
    public async Task Should_return_null_if_api_key_not_found()
    {
        var sut = await CreateSutAsync();

        var (result, etag) = await sut.GetByApiKeyAsync(Guid.NewGuid().ToString());

        Assert.Null(result);
        Assert.Null(etag);
    }

    [Fact]
    public async Task Should_throw_exception_if_api_key_is_used_by_other_app()
    {
        var sut = await CreateSutAsync();

        var apiKeys = new Dictionary<string, string>
        {
            [Guid.NewGuid().ToString()] = AppRoles.Admin
        }.ToReadonlyDictionary();

        await sut.UpsertAsync(CreateApp(appId) with { ApiKeys = apiKeys });

        await Assert.ThrowsAsync<UniqueConstraintException>(() => sut.UpsertAsync(CreateApp(Guid.NewGuid().ToString()) with { ApiKeys = apiKeys }));
    }

    [Fact]
    public async Task Should_get_app_by_auth_domain()
    {
        var sut = await CreateSutAsync();

        var domain = $"{Guid.NewGuid()}.io";

        await sut.UpsertAsync(CreateApp(appId) with { AuthScheme = CreateAuthScheme(domain) });

        var (result, etag) = await sut.GetByAuthDomainAsync(domain);

        Assert.Equal(appId, result?.Id);
        Assert.Equal(domain, result?.AuthScheme?.Domain);
        Assert.NotNull(etag);
    }

    [Fact]
    public async Task Should_return_null_if_auth_domain_not_found()
    {
        var sut = await CreateSutAsync();

        var (result, etag) = await sut.GetByAuthDomainAsync($"{Guid.NewGuid()}.io");

        Assert.Null(result);
        Assert.Null(etag);
    }

    [Fact]
    public async Task Should_return_true_if_any_app_has_auth_domain()
    {
        var sut = await CreateSutAsync();

        await sut.UpsertAsync(CreateApp(appId) with { AuthScheme = CreateAuthScheme($"{Guid.NewGuid()}.io") });

        var result = await sut.AnyAuthDomainAsync();

        Assert.True(result);
    }

    [Fact]
    public async Task Should_query_apps_with_pending_integrations()
    {
        var sut = await CreateSutAsync();

        var pendingApp = CreateApp(appId) with
        {
            Integrations = new Dictionary<string, ConfiguredIntegration>
            {
                ["integration1"] = CreateIntegration(IntegrationStatus.Verified),
                ["integration2"] = CreateIntegration(IntegrationStatus.Pending)
            }.ToReadonlyDictionary()
        };

        var verifiedApp = CreateApp(Guid.NewGuid().ToString()) with
        {
            Integrations = new Dictionary<string, ConfiguredIntegration>
            {
                ["integration1"] = CreateIntegration(IntegrationStatus.Verified)
            }.ToReadonlyDictionary()
        };

        await sut.UpsertAsync(pendingApp);
        await sut.UpsertAsync(verifiedApp);

        var result = await sut.QueryWithPendingIntegrationsAsync();

        Assert.Contains(result, x => x.Id == pendingApp.Id);
        Assert.DoesNotContain(result, x => x.Id == verifiedApp.Id);
    }

    [Fact]
    public async Task Should_not_query_app_after_pending_integration_has_been_verified()
    {
        var sut = await CreateSutAsync();

        var app = CreateApp(appId) with
        {
            Integrations = new Dictionary<string, ConfiguredIntegration>
            {
                ["integration1"] = CreateIntegration(IntegrationStatus.Pending)
            }.ToReadonlyDictionary()
        };

        await sut.UpsertAsync(app);

        await sut.UpsertAsync(app with
        {
            Integrations = new Dictionary<string, ConfiguredIntegration>
            {
                ["integration1"] = CreateIntegration(IntegrationStatus.Verified)
            }.ToReadonlyDictionary()
        });

        var result = await sut.QueryWithPendingIntegrationsAsync();

        Assert.DoesNotContain(result, x => x.Id == app.Id);
    }

    [Fact]
    public async Task Should_query_all_apps()
    {
        var sut = await CreateSutAsync();

        var app1 = CreateApp(appId);
        var app2 = CreateApp(Guid.NewGuid().ToString());

        await sut.UpsertAsync(app1);
        await sut.UpsertAsync(app2);

        var result = new List<App>();

        await foreach (var app in sut.QueryAllAsync())
        {
            result.Add(app);
        }

        Assert.Contains(result, x => x.Id == app1.Id);
        Assert.Contains(result, x => x.Id == app2.Id);
    }

    [Fact]
    public async Task Should_write_counters_to_existing_app()
    {
        var sut = await CreateSutAsync();

        await sut.UpsertAsync(CreateApp(appId));

        await sut.BatchWriteAsync(
        [
            (appId, new CounterMap { ["counter1"] = 1 })
        ], default);

        await sut.BatchWriteAsync(
        [
            (appId, new CounterMap { ["counter1"] = 2, ["counter2"] = 5 })
        ], default);

        var (result, _) = await sut.GetAsync(appId);

        Assert.Equal(3, result!.Counters!["counter1"]);
        Assert.Equal(5, result!.Counters!["counter2"]);
    }

    [Fact]
    public async Task Should_not_create_app_when_writing_counters()
    {
        var sut = await CreateSutAsync();

        await sut.BatchWriteAsync(
        [
            (appId, new CounterMap { ["counter1"] = 1 })
        ], default);

        var (result, _) = await sut.GetAsync(appId);

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_ignore_empty_counters()
    {
        var sut = await CreateSutAsync();

        await sut.UpsertAsync(CreateApp(appId));

        await sut.BatchWriteAsync(
        [
            (appId, [])
        ], default);

        var (result, _) = await sut.GetAsync(appId);

        Assert.Empty(result!.Counters!);
    }

    private static AppAuthScheme CreateAuthScheme(string domain)
    {
        return new AppAuthScheme
        {
            Domain = domain,
            DisplayName = "Auth",
            Authority = "https://authority.io",
            ClientId = "clientId",
            ClientSecret = "clientSecret"
        };
    }

    private static ConfiguredIntegration CreateIntegration(IntegrationStatus status)
    {
        return new ConfiguredIntegration("Type", ReadonlyDictionary.Empty<string, string>())
        {
            Enabled = true,
            Status = status
        };
    }

    private App CreateApp(string id)
    {
        return new App(id, now)
        {
            Name = "My App",
            // The API keys have a unique index, which also includes apps without keys.
            ApiKeys = new Dictionary<string, string>
            {
                [Guid.NewGuid().ToString()] = AppRoles.Admin
            }.ToReadonlyDictionary(),
            LastUpdate = now
        };
    }
}
