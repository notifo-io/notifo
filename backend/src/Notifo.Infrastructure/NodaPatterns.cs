// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using NodaTime.Text;

namespace Notifo.Infrastructure;

public static class NodaPatterns
{
    public static readonly LocalTimePattern HourIsoPatternImpl =
        LocalTimePattern.CreateWithInvariantCulture("HH");

    public static readonly LocalTimePattern HourMinuteIsoPatternImpl = 
        LocalTimePattern.CreateWithInvariantCulture("HH':'mm");

    public static readonly IPattern<LocalTime> VariablePrecisionIso = new CompositePatternBuilder<LocalTime>
    {
        { LocalTimePattern.ExtendedIso, time => true },
        { HourMinuteIsoPatternImpl, time => false },
        { HourIsoPatternImpl, time => false },
    }.Build();
}
