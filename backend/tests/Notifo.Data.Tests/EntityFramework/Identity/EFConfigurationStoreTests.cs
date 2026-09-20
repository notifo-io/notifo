// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using NodaTime;
using Notifo.Domain.Apps;
using Notifo.EntityFramework.TestHelpers;
using Notifo.Identity;
using Notifo.Identity.Dynamic;
using Notifo.Shared;

namespace Notifo.EntityFramework.Identity;

public abstract class EFConfigurationStoreTests<TContext>(ISqlFixture<TContext> fixture) : ConfigurationStoreTests where TContext : DbContext
{
    protected override Task<IConfigurationStore<AppAuthScheme>> CreateSutAsync()
    {
        var sut = CreateSut(SystemClock.Instance);

        return Task.FromResult<IConfigurationStore<AppAuthScheme>>(sut);
    }

    [Fact]
    public async Task Should_not_get_expired_value()
    {
        // Other stores use a time to live index and return the value until it is actually deleted.
        var clock = A.Fake<IClock>();

        var now = SystemClock.Instance.GetCurrentInstant();

        A.CallTo(() => clock.GetCurrentInstant())
            .Returns(now);

        var sut = CreateSut(clock);

        var key = Guid.NewGuid().ToString();

        await sut.SetAsync(key, CreateScheme(), TimeSpan.FromMinutes(10));

        A.CallTo(() => clock.GetCurrentInstant())
            .Returns(now.Plus(Duration.FromMinutes(11)));

        var result = await sut.GetAsync(key);

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_cleanup_expired_values()
    {
        var clock = A.Fake<IClock>();

        var now = SystemClock.Instance.GetCurrentInstant();

        A.CallTo(() => clock.GetCurrentInstant())
            .Returns(now);

        var sut = CreateSut(clock);

        var expiredKey = Guid.NewGuid().ToString();
        var validKey = Guid.NewGuid().ToString();

        await sut.SetAsync(expiredKey, CreateScheme(), TimeSpan.FromMinutes(10));
        await sut.SetAsync(validKey, CreateScheme(), TimeSpan.FromMinutes(30));

        A.CallTo(() => clock.GetCurrentInstant())
            .Returns(now.Plus(Duration.FromMinutes(20)));

        await sut.CleanupAsync(default);

        // Query the deleted value with the original time, so that it is not filtered out by the expiration date.
        A.CallTo(() => clock.GetCurrentInstant())
            .Returns(now);

        Assert.Null(await sut.GetAsync(expiredKey));
        Assert.NotNull(await sut.GetAsync(validKey));
    }

    private static AppAuthScheme CreateScheme()
    {
        return new AppAuthScheme
        {
            Domain = "domain.io",
            DisplayName = "Auth",
            Authority = "https://authority.io",
            ClientId = "clientId",
            ClientSecret = "clientSecret"
        };
    }

    private EFConfigurationStore<TContext, AppAuthScheme> CreateSut(IClock clock)
    {
        return new EFConfigurationStore<TContext, AppAuthScheme>(fixture.DbContextFactory, TestJson.Options, clock);
    }
}
