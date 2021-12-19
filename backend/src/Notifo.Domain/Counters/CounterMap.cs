// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable CS0162
#pragma warning disable RECS0092 // Convert field to readonly
#pragma warning disable RECS0065 // Expression is always 'true' or always 'false'

namespace Notifo.Domain.Counters
{
    public sealed class CounterMap : Dictionary<string, long>
    {
        private const bool SupportPending = false;

        private SpinLock slimLock = new SpinLock(false);

        public const string NotificationsHandled = "notifications_handled";

        public const string NotificationsFailed = "notifications_failed";

        public const string NotificationsAttempt = "notifications_attempt";

        public static string ChannelHandled(string channel) => $"{channel}_handled";

        public static string ChannelFailed(string channel) => $"{channel}_failed";

        public static string ChannelAttmept(string channel) => $"{channel}_attempt";

        public static string ChannelSkipped(string channel) => $"{channel}_skipped";

        public CounterMap()
        {
        }

        public CounterMap(CounterMap source)
            : base(source)
        {
        }

        public static void Cleanup(IEnumerable<CounterMap?> counterMaps)
        {
            foreach (var counters in counterMaps)
            {
                Cleanup(counters);
            }
        }

        public static void Cleanup(CounterMap? counters)
        {
            if (counters == null)
            {
                return;
            }

            foreach (var (key, value) in counters.ToList())
            {
                if (value <= 0 || (!SupportPending && key.Contains("pending", StringComparison.OrdinalIgnoreCase)))
                {
                    counters.Remove(key);
                }
            }
        }

        public static CounterMap ForNotification(ProcessStatus status, int count = 1)
        {
            var result = new CounterMap();

            switch (status)
            {
                case ProcessStatus.Attempt when SupportPending:
                    result.Increment(NotificationsAttempt, count);
                    break;
                case ProcessStatus.Handled:
                    result.Increment(NotificationsHandled, count);
                    break;
                case ProcessStatus.Failed:
                    result.Increment(NotificationsFailed, count);
                    break;
            }

            return result;
        }

        public static CounterMap ForChannel(string channel, ProcessStatus status, int count = 1)
        {
            var result = new CounterMap();

            switch (status)
            {
                case ProcessStatus.Attempt when SupportPending:
                    result.Increment(ChannelAttmept(channel), count);
                    break;
                case ProcessStatus.Skipped:
                    result.Increment(ChannelSkipped(channel), count);
                    break;
                case ProcessStatus.Handled:
                    result.Increment(ChannelHandled(channel), count);
                    break;
                case ProcessStatus.Failed:
                    result.Increment(ChannelFailed(channel), count);
                    break;
            }

            return result;
        }

        public CounterMap Increment(string key, long value = 1)
        {
            var newValue = value;

            if (TryGetValue(key, out var current))
            {
                newValue += current;
            }

            this[key] = newValue;

            return this;
        }
    }
}
