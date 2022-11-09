// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using NodaTime;
using Xunit;

namespace Notifo.Domain;

public sealed class SchedulingTests
{
    private readonly IClock clock = A.Fake<IClock>();
    private readonly Instant now;

    public SchedulingTests()
    {
        now = Instant.FromUtc(2010, 2, 10, 14, 30, 10);

        A.CallTo(() => clock.GetCurrentInstant())
            .Returns(now);
    }

    [Fact]
    public void Should_return_now_for_null_scheduling()
    {
        Scheduling? sut = null;

        var actual = Scheduling.CalculateScheduleTime(sut, clock, "Europe/Berlin");

        Assert.Equal(now, actual);
    }

    [Fact]
    public void Should_return_now_for_undefined_scheduling()
    {
        var sut = new Scheduling();

        var actual = Scheduling.CalculateScheduleTime(sut, clock, "Europe/Berlin");

        Assert.Equal(now, actual);
    }

    [Fact]
    public void Should_return_concrete_utc_datetime()
    {
        var time = new LocalTime(15, 13, 12);

        var sut = new Scheduling
        {
            Type = SchedulingType.UTC,
            Time = time,
            Date = new LocalDate(2010, 2, 14)
        };

        var actual = Scheduling.CalculateScheduleTime(sut, clock, "Europe/Berlin");

        // Global time is UTC time.
        var expected = Instant.FromUtc(2010, 2, 14, 15, 13, 12);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_return_concrete_user_datetime()
    {
        var time = new LocalTime(15, 13, 12);

        var sut = new Scheduling
        {
            Type = SchedulingType.UserTime,
            Time = time,
            Date = new LocalDate(2010, 2, 14)
        };

        var actual = Scheduling.CalculateScheduleTime(sut, clock, "Europe/Berlin");

        // Berlin is UTC+1 in winter.
        var expected = Instant.FromUtc(2010, 2, 14, 14, 13, 12);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_return_weekday_utc_datetime()
    {
        var time = new LocalTime(15, 13, 12);

        var sut = new Scheduling
        {
            Type = SchedulingType.UTC,
            Time = time,
            NextWeekDay = IsoDayOfWeek.Friday
        };

        var actual = Scheduling.CalculateScheduleTime(sut, clock, "Europe/Berlin");

        // Global time is UTC time.
        var expected = Instant.FromUtc(2010, 2, 12, 15, 13, 12);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_return_weekday_utc_datetime_if_day_is_the_same_day()
    {
        var time = new LocalTime(15, 13, 12);

        var sut = new Scheduling
        {
            Type = SchedulingType.UTC,
            Time = time,
            NextWeekDay = IsoDayOfWeek.Wednesday
        };

        var actual = Scheduling.CalculateScheduleTime(sut, clock, "Europe/Berlin");

        // Global time is UTC time.
        var expected = Instant.FromUtc(2010, 2, 10, 15, 13, 12);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_return_weekday_user_datetime()
    {
        var time = new LocalTime(15, 13, 12);

        var sut = new Scheduling
        {
            Type = SchedulingType.UserTime,
            Time = time,
            NextWeekDay = IsoDayOfWeek.Friday
        };

        var actual = Scheduling.CalculateScheduleTime(sut, clock, "Europe/Berlin");

        // Berlin is UTC+1 in winter.
        var expected = Instant.FromUtc(2010, 2, 12, 14, 13, 12);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_return_weekday_user_datetime_if_day_is_the_same_day()
    {
        var time = new LocalTime(15, 13, 12);

        var sut = new Scheduling
        {
            Type = SchedulingType.UserTime,
            Time = time,
            NextWeekDay = IsoDayOfWeek.Wednesday
        };

        var actual = Scheduling.CalculateScheduleTime(sut, clock, "Europe/Berlin");

        // Berlin is UTC+1 in winter.
        var expected = Instant.FromUtc(2010, 2, 10, 14, 13, 12);

        Assert.Equal(expected, actual);
    }
}
