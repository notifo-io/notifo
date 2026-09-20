// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Apps;
using Notifo.Identity.Dynamic;

namespace Notifo.Shared;

public abstract class ConfigurationStoreTests
{
    private readonly string key = Guid.NewGuid().ToString();

    protected abstract Task<IConfigurationStore<AppAuthScheme>> CreateSutAsync();

    [Fact]
    public async Task Should_return_null_if_key_not_found()
    {
        var sut = await CreateSutAsync();

        var result = await sut.GetAsync(key);

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_set_and_get_value()
    {
        var sut = await CreateSutAsync();

        var value = CreateScheme("domain1.io");

        await sut.SetAsync(key, value, TimeSpan.FromMinutes(10));

        var result = await sut.GetAsync(key);

        result.Should().BeEquivalentTo(value);
    }

    [Fact]
    public async Task Should_overwrite_value()
    {
        var sut = await CreateSutAsync();

        await sut.SetAsync(key, CreateScheme("domain1.io"), TimeSpan.FromMinutes(10));
        await sut.SetAsync(key, CreateScheme("domain2.io"), TimeSpan.FromMinutes(10));

        var result = await sut.GetAsync(key);

        Assert.Equal("domain2.io", result?.Domain);
    }

    private static AppAuthScheme CreateScheme(string domain)
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
}
