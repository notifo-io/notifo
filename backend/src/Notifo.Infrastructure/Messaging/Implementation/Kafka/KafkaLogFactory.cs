// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Notifo.Infrastructure.Messaging.Implementation.Kafka
{
    public static class KafkaLogFactory<TKey, TValue>
    {
        public static Action<IConsumer<TKey, TValue>, string> ConsumerStats(ILogger log)
        {
            return (producer, message) =>
            {
                Log(log, message);
            };
        }

        public static Action<IConsumer<TKey, TValue>, Error> ConsumerError(ILogger log)
        {
            return (producer, message) =>
            {
                Log(log, message);
            };
        }

        public static Action<IConsumer<TKey, TValue>, LogMessage> ConsumerLog(ILogger log)
        {
            return (producer, message) =>
            {
                Log(log, message);
            };
        }

        public static Action<IProducer<TKey, TValue>, string> ProducerStats(ILogger log)
        {
            return (producer, message) =>
            {
                Log(log, message);
            };
        }

        public static Action<IProducer<TKey, TValue>, Error> ProducerError(ILogger log)
        {
            return (producer, message) =>
            {
                Log(log, message);
            };
        }

        public static Action<IProducer<TKey, TValue>, LogMessage> ProducerLog(ILogger log)
        {
            return (producer, message) =>
            {
                Log(log, message);
            };
        }

        private static void Log(ILogger log, string stats)
        {
            log.LogInformation("Kafka stastics received: {stats}.", stats);
        }

        private static void Log(ILogger log, Error error)
        {
            log.LogInformation("Kafka error with code {code} happened: {details}.", error.Code, error.Reason);
        }

        private static void Log(ILogger log, LogMessage message)
        {
            var level = GetLogLevel(message.Level);

            if (log.IsEnabled(level))
            {
                return;
            }

            log.Log(level, "Kafka log recieved from system {system}: {message}.", message.Name, message.Message);
        }

        private static LogLevel GetLogLevel(SyslogLevel level)
        {
            switch (level)
            {
                case SyslogLevel.Emergency:
                    return LogLevel.Critical;
                case SyslogLevel.Alert:
                    return LogLevel.Critical;
                case SyslogLevel.Critical:
                    return LogLevel.Critical;
                case SyslogLevel.Error:
                    return LogLevel.Error;
                case SyslogLevel.Warning:
                    return LogLevel.Warning;
                case SyslogLevel.Notice:
                    return LogLevel.Information;
                case SyslogLevel.Info:
                    return LogLevel.Information;
                case SyslogLevel.Debug:
                    return LogLevel.Debug;
                default:
                    return LogLevel.Debug;
            }
        }
    }
}
