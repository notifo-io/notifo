// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using Notifo.Domain.UserNotifications;
using Notifo.EntityFramework.TestHelpers;
using Notifo.Shared;

namespace Notifo.EntityFramework.Domain.UserNotifications;

public abstract class EFUserNotificationRepositoryTests<TContext>(ISqlFixture<TContext> fixture) : UserNotificationRepositoryTests where TContext : DbContext
{
    protected override Task<IUserNotificationRepository> CreateSutAsync()
    {
        var sut = CreateSut(new UserNotificationsOptions { MaxItemsPerUser = 100 });

        return Task.FromResult<IUserNotificationRepository>(sut);
    }

    [Fact]
    public async Task Should_cleanup_notifications_after_retention_time()
    {
        // Other stores use a time to live index, therefore the cleanup is only implemented for entity framework.
        var sut = CreateSut(new UserNotificationsOptions { MaxItemsPerUser = 100, RetentionTime = TimeSpan.FromDays(20) });

        var oldNotification = CreateNotification(UserId1, Now.Minus(Duration.FromDays(40)));
        var newNotification = CreateNotification(UserId1, Now);

        await sut.InsertAsync(oldNotification);
        await sut.InsertAsync(newNotification);

        await sut.CleanupAsync(default);

        var result = await sut.QueryAsync(AppId, UserId1, new UserNotificationQuery { Take = 100 });

        Assert.Equal([newNotification.Id], result.Select(x => x.Id));
    }

    private EFUserNotificationRepository<TContext> CreateSut(UserNotificationsOptions options)
    {
        return new EFUserNotificationRepository<TContext>(fixture.DbContextFactory, Options.Create(options), SystemClock.Instance, A.Fake<ILogger<EFUserNotificationRepository<TContext>>>());
    }
}
