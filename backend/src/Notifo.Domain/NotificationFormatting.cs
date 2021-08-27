// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using Notifo.Domain.Events;
using Notifo.Domain.Utils;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Texts;

namespace Notifo.Domain
{
    public static class NotificationFormatting
    {
        public static string ToDebugString(this NotificationFormatting<LocalizedText> source)
        {
            var subject = source.Subject;

            return subject.GetOrDefault("en").OrDefault(subject.Values.FirstOrDefault() ?? string.Empty);
        }

        public static NotificationFormatting<string> SelectText(this NotificationFormatting<LocalizedText> source, string language)
        {
            return source.Transform(SelectText(language));
        }

        public static NotificationFormatting<LocalizedText> Format(this NotificationFormatting<LocalizedText> source, EventProperties properties)
        {
            return source.Transform(FormatText(properties));
        }

        public static bool HasSubject(this NotificationFormatting<string> formatting)
        {
            return !string.IsNullOrWhiteSpace(formatting.Subject);
        }

        public static bool HasSubject(this NotificationFormatting<LocalizedText> formatting)
        {
            return formatting.Subject?.Values.Any(x => !string.IsNullOrWhiteSpace(x)) == true;
        }

        private static Func<LocalizedText?, string> SelectText(string language)
        {
            return text =>
            {
                return text?.GetOrDefault(language) ?? string.Empty;
            };
        }

        private static Func<LocalizedText?, LocalizedText> FormatText(EventProperties properties)
        {
            return text =>
            {
                return text?.Format(properties!) ?? new LocalizedText();
            };
        }

        private static NotificationFormatting<TOut> Transform<TIn, TOut>(this NotificationFormatting<TIn> formatting, Func<TIn?, TOut> transform)
            where TIn : class
            where TOut : class
        {
            Guard.NotNull(transform, nameof(transform));

            var result = new NotificationFormatting<TOut>
            {
                Body = transform(formatting.Body),
                ConfirmMode = formatting.ConfirmMode,
                ConfirmText = transform(formatting.ConfirmText),
                ImageLarge = transform(formatting.ImageLarge),
                ImageSmall = transform(formatting.ImageSmall),
                LinkText = transform(formatting.LinkText),
                LinkUrl = transform(formatting.LinkUrl),
                Subject = transform(formatting.Subject)
            };

            return result;
        }
    }
}
