// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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

        public static NotificationFormatting<LocalizedText> Format(this NotificationFormatting<LocalizedText> source, NotificationProperties properties)
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

        private static Func<LocalizedText?, LocalizedText> FormatText(NotificationProperties properties)
        {
            return text =>
            {
                return text?.Format(properties!) ?? new LocalizedText();
            };
        }

        public static NotificationFormatting<string> Clone(this NotificationFormatting<string> source)
        {
            return new NotificationFormatting<string>
            {
                Body = source.Body,
                ConfirmMode = source.ConfirmMode,
                ConfirmText = source.ConfirmText,
                ImageLarge = source.ImageLarge,
                ImageSmall = source.ImageSmall,
                LinkText = source.LinkText,
                LinkUrl = source.LinkUrl,
                Subject = source.Subject
            };
        }

        public static NotificationFormatting<LocalizedText> MergedWith(this NotificationFormatting<LocalizedText> source, NotificationFormatting<LocalizedText>? other)
        {
            return new NotificationFormatting<LocalizedText>
            {
                Body = Merged(source.Body, other?.Body),
                ConfirmMode = source.ConfirmMode ?? other?.ConfirmMode,
                ConfirmText = Merged(source.ConfirmText, other?.ConfirmText),
                ImageLarge = Merged(source.ImageLarge, other?.ImageLarge),
                ImageSmall = Merged(source.ImageSmall, other?.ImageSmall),
                LinkText = Merged(source.LinkText, other?.LinkText),
                LinkUrl = Merged(source.LinkUrl, other?.LinkUrl),
                Subject = Merged(source.Subject, other?.Subject)!
            };
        }

        public static NotificationFormatting<LocalizedText> Clone(this NotificationFormatting<LocalizedText> source)
        {
            return new NotificationFormatting<LocalizedText>
            {
                Body = source.Body?.Clone(),
                ConfirmMode = source.ConfirmMode,
                ConfirmText = source.ConfirmText?.Clone(),
                ImageLarge = source.ImageLarge?.Clone(),
                ImageSmall = source.ImageSmall?.Clone(),
                LinkText = source.LinkText?.Clone(),
                LinkUrl = source.LinkUrl?.Clone(),
                Subject = source.Subject.Clone()
            };
        }

        public static NotificationFormatting<TOut> Transform<TIn, TOut>(this NotificationFormatting<TIn> formatting, Func<TIn?, TOut> transform) where TIn : class where TOut : class
        {
            Guard.NotNull(transform);

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

        private static LocalizedText? Merged(LocalizedText? source, LocalizedText? other)
        {
            if (source != null && other != null)
            {
                var result = new LocalizedText(source);

                foreach (var (key, value) in source)
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        result[key] = value;
                    }
                }

                return result;
            }

            return other ?? source;
        }
    }
}
