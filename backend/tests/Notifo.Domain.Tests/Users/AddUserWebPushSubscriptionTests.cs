// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Channels.WebPush;
using Notifo.Infrastructure.Collections;

namespace Notifo.Domain.Users;

public class AddUserWebPushSubscriptionTests
{
    [Fact]
    public async Task Should_not_remove_existing_subscriptions_when_new_subscription_added()
    {
        var sut = new AddUserWebPushSubscription();

        var endpoint1 = "test endpoint 1";
        var endpoint2 = "test endpoint 2";
        var endpoint3 = "test endpoint 3";

        sut.Subscription = new WebPushSubscription { Endpoint = endpoint1 };

        var user = new User("1", "1", default)
        {
            WebPushSubscriptions = new List<string>
                {
                    endpoint2,
                    endpoint3
                }
                .Select(e => new WebPushSubscription { Endpoint = e })
                .ToReadonlyList()
        };

        var updatedUser = await sut.ExecuteAsync(user, A.Fake<IServiceProvider>(), default);

        Assert.Equal(new[]
        {
            endpoint1,
            endpoint2,
            endpoint3
        }, updatedUser!.WebPushSubscriptions.Select(x => x.Endpoint).OrderBy(x => x).ToArray());
    }

    [Fact]
    public async Task Should_not_change_existing_subscription_if_subscription_added_again()
    {
        var sut = new AddUserWebPushSubscription();

        var endpoint1 = "test subscription 1";
        var endpoint2 = "test subscription 2";

        sut.Subscription = new WebPushSubscription { Endpoint = endpoint1 };

        var user = new User("1", "1", default)
        {
            WebPushSubscriptions = new List<string>
                {
                    endpoint1,
                    endpoint2
                }
                .Select(e => new WebPushSubscription { Endpoint = e })
                .ToReadonlyList()
        };

        var updatedUser = await sut.ExecuteAsync(user, A.Fake<IServiceProvider>(), default);

        Assert.Null(updatedUser);
    }
}
