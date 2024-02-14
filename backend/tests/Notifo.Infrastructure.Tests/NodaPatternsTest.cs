// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Notifo.Infrastructure;

public class NodaPatternsTest
{
    [Fact]
    public void Should_parse_and_format_hour_minutes()
    {
        var parsed = NodaPatterns.VariablePrecisionIso.Parse("12:15").Value;

        Assert.Equal(new LocalTime(12, 15), parsed);
        Assert.Equal("12:15:00", NodaPatterns.VariablePrecisionIso.Format(parsed));
    }

    [Fact]
    public void Should_parse_and_format_hour_minutes_seconds()
    {
        var parsed = NodaPatterns.VariablePrecisionIso.Parse("12:15:42").Value;

        Assert.Equal(new LocalTime(12, 15, 42), parsed);
        Assert.Equal("12:15:42", NodaPatterns.VariablePrecisionIso.Format(parsed));
    }

    [Fact]
    public void Should_parse_and_format_hour_minutes_seconds_milliseconds()
    {
        var parsed = NodaPatterns.VariablePrecisionIso.Parse("12:15:42.123").Value;

        Assert.Equal(new LocalTime(12, 15, 42, 123), parsed);
        Assert.Equal("12:15:42.123", NodaPatterns.VariablePrecisionIso.Format(parsed));
    }
}
