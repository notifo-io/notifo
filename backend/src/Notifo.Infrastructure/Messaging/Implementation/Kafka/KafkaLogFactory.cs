// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Confluent.Kafka;
using Squidex.Log;

namespace Notifo.Infrastructure.Messaging.Implementation.Kafka
{
    public static class KafkaLogFactory<TKey, TValue>
    {
        public static Action<IConsumer<TKey, TValue>, string> ConsumerStats(ISemanticLog log)
        {
            return (producer, message) =>
            {
                Log(log, message);
            };
        }

        public static Action<IConsumer<TKey, TValue>, Error> ConsumerError(ISemanticLog log)
        {
            return (producer, message) =>
            {
                Log(log, message);
            };
        }

        public static Action<IConsumer<TKey, TValue>, LogMessage> ConsumerLog(ISemanticLog log)
        {
            return (producer, message) =>
            {
                Log(log, message);
            };
        }

        public static Action<IProducer<TKey, TValue>, string> ProducerStats(ISemanticLog log)
        {
            return (producer, message) =>
            {
                Log(log, message);
            };
        }

        public static Action<IProducer<TKey, TValue>, Error> ProducerError(ISemanticLog log)
        {
            return (producer, message) =>
            {
                Log(log, message);
            };
        }

        public static Action<IProducer<TKey, TValue>, LogMessage> ProducerLog(ISemanticLog log)
        {
            return (producer, message) =>
            {
                Log(log, message);
            };
        }

        private static void Log(ISemanticLog log, string stats)
        {
            log.LogInformation(w => w
                .WriteProperty("system", "Kafka")
                .WriteProperty("statistics", stats));
        }

        private static void Log(ISemanticLog log, Error error)
        {
            log.LogInformation(w => w
                .WriteProperty("system", "Kafka")
                .WriteProperty("action", "Failed")
                .WriteProperty("errorCode", (int)error.Code)
                .WriteProperty("errorCodeText", error.Code.ToString())
                .WriteProperty("errorReason", error.Reason));
        }

        private static void Log(ISemanticLog log, LogMessage message)
        {
            var level = GetLogLevel(message.Level);

            log.Log(level, null, w => w
                .WriteProperty("system", "Kafka")
                .WriteProperty("name", message.Name)
                .WriteProperty("message", message.Message));
        }

        private static SemanticLogLevel GetLogLevel(SyslogLevel level)
        {
            switch (level)
            {
                case SyslogLevel.Emergency:
                    return SemanticLogLevel.Fatal;
                case SyslogLevel.Alert:
                    return SemanticLogLevel.Fatal;
                case SyslogLevel.Critical:
                    return SemanticLogLevel.Fatal;
                case SyslogLevel.Error:
                    return SemanticLogLevel.Error;
                case SyslogLevel.Warning:
                    return SemanticLogLevel.Warning;
                case SyslogLevel.Notice:
                    return SemanticLogLevel.Information;
                case SyslogLevel.Info:
                    return SemanticLogLevel.Information;
                case SyslogLevel.Debug:
                    return SemanticLogLevel.Debug;
                default:
                    return SemanticLogLevel.Debug;
            }
        }
    }
}
