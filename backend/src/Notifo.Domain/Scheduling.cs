// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Notifo.Domain
{
    public sealed class Scheduling
    {
        public SchedulingType Type { get; init; }

        public IsoDayOfWeek? NextWeekDay { get; init; }

        public LocalDate? Date { get; init; }

        public LocalTime Time { get; init; }

        public static Instant CalculateScheduleTime(Scheduling? scheduling, IClock clock, string userTimeZone)
        {
            var now = clock.GetCurrentInstant();

            if (scheduling?.Date != null)
            {
                return scheduling.GetNextByDate(scheduling.Date.Value, userTimeZone);
            }

            if (scheduling?.NextWeekDay != null)
            {
                return scheduling.GetNextByDayOfWeek(scheduling.NextWeekDay.Value, now, userTimeZone);
            }

            return now;
        }

        private Instant GetNextByDate(LocalDate date, string userTimeZone)
        {
            var zone = GetZone(userTimeZone);

            var dateTime = zone.AtStrictly(date + Time);

            return dateTime.ToInstant();
        }

        private Instant GetNextByDayOfWeek(IsoDayOfWeek dayOfWeek, Instant now, string userTimeZone)
        {
            var zone = GetZone(userTimeZone);

            var date = new ZonedDateTime(now, zone).Date;

            if (date.DayOfWeek != dayOfWeek)
            {
                date = date.Next(dayOfWeek);
            }

            var dateTime = zone.AtStrictly(date + Time);

            return dateTime.ToInstant();
        }

        private DateTimeZone GetZone(string userTimeZone)
        {
            if (Type == SchedulingType.UTC)
            {
                return DateTimeZone.Utc;
            }
            else
            {
                return DateTimeZoneProviders.Tzdb.GetZoneOrNull(userTimeZone) ?? DateTimeZone.Utc;
            }
        }
    }
}
