// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers;

public sealed class SchedulingDto
{
    /// <summary>
    /// The scheduling type.
    /// </summary>
    public SchedulingType Type { get; set; }

    /// <summary>
    /// To schedule the event at the next day of the week.
    /// </summary>
    public IsoDayOfWeek? NextWeekDay { get; set; }

    /// <summary>
    /// The scheduling date.
    /// </summary>
    public LocalDate? Date { get; set; }

    /// <summary>
    /// The scheduling time.
    /// </summary>
    public LocalTime Time { get; set; }

    /// <summary>
    /// The delay relative to the server time.
    /// </summary>
    public int? DelayInSeconds { get; init; }

    public static SchedulingDto FromDomainObject(Scheduling source)
    {
        var result = SimpleMapper.Map(source, new SchedulingDto());

        return result;
    }

    public Scheduling ToDomainObject()
    {
        var result = SimpleMapper.Map(this, new Scheduling());

        return result;
    }
}
