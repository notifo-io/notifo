// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Notifo.Domain.Channels.WebPush;
using Notifo.Infrastructure.Collections;
using Xunit;

namespace Notifo.Domain.Users;

public class RemoveUserWebPushSubscriptionTests
{
    [Fact]
    public async Task Should_remove_subscription_if_endpoint_exists()
    {
        var endpoint1 = "test endpoint 1";
        var endpoint2 = "test endpoint 2";
        var endpoint3 = "test endpoint 3";

        var sut = new RemoveUserWebPushSubscription
        {
            Endpoint = endpoint1
        };

        var user = new User("1", "1", default)
        {
            WebPushSubscriptions = new List<string>
                {
                    endpoint1,
                    endpoint2,
                    endpoint3
                }
                .Select(e => new WebPushSubscription { Endpoint = e })
                .ToReadonlyList()
        };

        var updatedUser = await sut.ExecuteAsync(user, A.Fake<IServiceProvider>(), default);

        Assert.Equal(new[]
        {
            endpoint2,
            endpoint3
        }, updatedUser!.WebPushSubscriptions.Select(x => x.Endpoint).OrderBy(x => x).ToArray());
    }

    [Fact]
    public async Task Should_not_change_subscriptions_if_endpoint_not_exists()
    {
        var endpoint1 = "test endpoint 1";
        var endpoint2 = "test endpoint 2";
        var endpoint3 = "test endpoint 3";

        var sut = new RemoveUserWebPushSubscription
        {
            Endpoint = endpoint1
        };

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

        Assert.Null(updatedUser);
    }

    [Fact]
    public async Task Should_remove_single_subscription()
    {
        var endpoint1 = "test endpoint 1";

        var sut = new RemoveUserWebPushSubscription
        {
            Endpoint = endpoint1
        };

        var user = new User("1", "1", default)
        {
            WebPushSubscriptions = new List<string>
                {
                    endpoint1
                }
                .Select(e => new WebPushSubscription { Endpoint = e })
                .ToReadonlyList()
        };

        var updatedUser = await sut.ExecuteAsync(user, A.Fake<IServiceProvider>(), default);

        Assert.Empty(updatedUser!.WebPushSubscriptions);
    }

    [Fact]
    public async Task Should_remove_subscription_if_encoding_differs()
    {
        var endpoint1 = "test+endpoint+1";

        var sut = new RemoveUserWebPushSubscription
        {
            Endpoint = "test%2bendpoint%2b1"
        };

        var user = new User("1", "1", default)
        {
            WebPushSubscriptions = new List<string>
                {
                    endpoint1
                }
                .Select(e => new WebPushSubscription { Endpoint = e })
                .ToReadonlyList()
        };

        var updatedUser = await sut.ExecuteAsync(user, A.Fake<IServiceProvider>(), default);

        Assert.Empty(updatedUser!.WebPushSubscriptions);
    }
}
