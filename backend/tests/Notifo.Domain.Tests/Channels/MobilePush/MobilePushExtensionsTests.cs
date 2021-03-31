// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using NodaTime;
using Xunit;

namespace Notifo.Domain.Channels.MobilePush
{
    public class MobilePushExtensionsTests
    {
        [Fact]
        public void Should_wakeup_now_when_not_waked_up_before()
        {
            var now = SystemClock.Instance.GetCurrentInstant();

            var simulatedClock = A.Fake<IClock>();

            A.CallTo(() => simulatedClock.GetCurrentInstant())
                .Returns(now);

            var token = new MobilePushToken
            {
                LastWakeup = default
            };

            var result = token.GetNextWakeupTime(simulatedClock);

            Assert.Equal(now, result);
        }

        [Fact]
        public void Should_wakeup_now_when_not_waked_up_for_a_while()
        {
            var now = SystemClock.Instance.GetCurrentInstant();

            var simulatedClock = A.Fake<IClock>();

            A.CallTo(() => simulatedClock.GetCurrentInstant())
                .Returns(now);

            var token = new MobilePushToken
            {
                LastWakeup = now.Minus(Duration.FromMinutes(35))
            };

            var result = token.GetNextWakeupTime(simulatedClock);

            Assert.Equal(now, result);
        }

        [Fact]
        public void Should_wakeup_as_soon_as_possible()
        {
            var now = SystemClock.Instance.GetCurrentInstant();

            var simulatedClock = A.Fake<IClock>();

            A.CallTo(() => simulatedClock.GetCurrentInstant())
                .Returns(now);

            var token = new MobilePushToken
            {
                LastWakeup = now.Minus(Duration.FromMinutes(25))
            };

            var result = token.GetNextWakeupTime(simulatedClock);

            Assert.Equal(now.Plus(Duration.FromMinutes(5)), result);
        }

        [Fact]
        public void Should_not_wakeup_if_already_scheduled()
        {
            var now = SystemClock.Instance.GetCurrentInstant();

            var simulatedClock = A.Fake<IClock>();

            A.CallTo(() => simulatedClock.GetCurrentInstant())
                .Returns(now);

            var token = new MobilePushToken
            {
                LastWakeup = now.Plus(Duration.FromMinutes(25))
            };

            var result = token.GetNextWakeupTime(simulatedClock);

            Assert.Null(result);
        }
    }
}
