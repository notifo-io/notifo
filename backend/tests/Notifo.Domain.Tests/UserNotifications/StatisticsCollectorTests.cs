// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.Integrations;
using Notifo.Domain.UserNotifications.Internal;

namespace Notifo.Domain.UserNotifications;

public class StatisticsCollectorTests
{
    private readonly IUserNotificationRepository repository = A.Fake<IUserNotificationRepository>();
    private readonly List<(TrackingToken Token, DeliveryResult Result)[]> writes = [];
    private readonly StatisticsCollector sut;

    public StatisticsCollectorTests()
    {
        A.CallTo(() => repository.BatchWriteAsync(A<(TrackingToken, DeliveryResult)[]>._, A<Instant>._, A<CancellationToken>._))
            .Invokes(x => writes.Add(x.GetArgument<(TrackingToken, DeliveryResult)[]>(0)!));

        sut = new StatisticsCollector(repository, SystemClock.Instance, 60_000, capacity: 1);
    }

    [Fact]
    public async Task Should_write_failed_updates_again_before_new_updates()
    {
        var token1 = new TrackingToken(Guid.NewGuid());
        var token2 = new TrackingToken(Guid.NewGuid());

        A.CallTo(() => repository.BatchWriteAsync(A<(TrackingToken, DeliveryResult)[]>._, A<Instant>._, A<CancellationToken>._))
            .Throws(new InvalidOperationException()).Once();

        await sut.AddAsync(token1, DeliveryResult.Sent);
        await sut.AddAsync(token2, DeliveryResult.Handled);

        await sut.StopAsync();

        Assert.Equal([token1, token2], writes[^1].Select(x => x.Token));
    }

    [Fact]
    public async Task Should_write_remaining_updates_on_stop()
    {
        var collector = new StatisticsCollector(repository, SystemClock.Instance, 60_000);

        var token = new TrackingToken(Guid.NewGuid());

        await collector.AddAsync(token, DeliveryResult.Sent);
        await collector.StopAsync();

        Assert.Equal([token], writes.Single().Select(x => x.Token));
    }
}
