// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.ObjectPool;
using Notifo.Domain.Resources;
using Notifo.Domain.Utils;
using Notifo.Infrastructure;

namespace Notifo.Domain.Channels.Email.Formatting
{
    public sealed class ParsedTemplate
    {
        private const string NotificationsPlaceholder = "<<<<Notifications>>>>";
        private const string ItemDefault = "NOTIFICATION";
        private const string ItemWithButton = "NOTIFICATION WITH BUTTON";
        private const string ItemWithButtonAndImage = "NOTIFICATION WITH BUTTON AND IMAGE";
        private const string ItemWithImage = "NOTIFICATION WITH IMAGE";
        private static readonly ObjectPool<StringBuilder> Pool = ObjectPool.Create(new StringBuilderPooledObjectPolicy());

        public string Text { get; set; }

        public Dictionary<string, string> ItemTemplates { get; set; } = new Dictionary<string, string>();

        public string Format(List<EmailJob> jobs, Dictionary<string, string?> properties, bool asHtml, IImageFormatter imageFormatter)
        {
            var notificationProperties = new Dictionary<string, string?>();

            var stringBuilder = Pool.Get();
            try
            {
                var text = Text.Format(properties);

                jobs.Foreach((job, index) =>
                {
                    var notification = job.Notification;

                    var formatting = notification.Formatting;

                    var inner = string.Empty;

                    var hasButton = !string.IsNullOrWhiteSpace(formatting.ConfirmText) && !string.IsNullOrWhiteSpace(job.Notification.ConfirmUrl);
                    var hasImage = !string.IsNullOrWhiteSpace(formatting.ImageSmall) || !string.IsNullOrWhiteSpace(formatting.ImageLarge);

                    if (hasButton && hasImage)
                    {
                        ItemTemplates.TryGetValue(ItemWithButtonAndImage, out inner);
                    }

                    if (hasButton && string.IsNullOrWhiteSpace(inner))
                    {
                        ItemTemplates.TryGetValue(ItemWithButton, out inner);
                    }

                    if (hasImage && string.IsNullOrWhiteSpace(inner))
                    {
                        ItemTemplates.TryGetValue(ItemWithImage, out inner);
                    }

                    if (string.IsNullOrWhiteSpace(inner))
                    {
                        inner = ItemTemplates[ItemDefault];
                    }

                    notificationProperties.Clear();
                    notificationProperties["notification.body"] = notification.Body(asHtml);
                    notificationProperties["notification.confirmText"] = notification.ConfirmText();
                    notificationProperties["notification.confirmUrl"] = notification.ConfirmUrl();
                    notificationProperties["notification.imageLarge"] = notification.ImageLarge(imageFormatter, "EmailLarge");
                    notificationProperties["notification.imageSmall"] = notification.ImageSmall(imageFormatter, "EmailSmall");
                    notificationProperties["notification.subject"] = notification.Subject(asHtml);

                    inner = inner.Format(notificationProperties);

                    stringBuilder.AppendLine(inner);

                    if (!string.IsNullOrEmpty(notification.TrackSeenUrl) && asHtml)
                    {
                        var trackingLink = $"<img height=\"0\" width=\"0\" style=\"width: 0px; height: 0px; position: absolute; visibility: hidden;\" src=\"{notification.TrackSeenUrl}\" />";

                        stringBuilder.Append(trackingLink);
                    }
                });

                return text.Replace(NotificationsPlaceholder, stringBuilder.ToString(), StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                Pool.Return(stringBuilder);
            }
        }

        public static (ParsedTemplate? Template, string? Error) Create(string? body)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                return default;
            }

            var result = new ParsedTemplate();

            result.Prepare(body);

            if (!result.ItemTemplates.ContainsKey(ItemDefault))
            {
                return (null, Texts.Email_TemplateNoItem);
            }

            return (result, null);
        }

        private void Prepare(string template)
        {
            while (true)
            {
                var (newTemplate, type, item) = Extract(template);

                if (item == null || type == null)
                {
                    break;
                }

                ItemTemplates[type.ToUpperInvariant()] = item;

                template = newTemplate;
            }

            Text = template;
        }

        private static (string Template, string? Type, string? Inner) Extract(string template)
        {
            var span = template.AsSpan();

            var start = Regex.Match(template, "<!--[\\s]*START:(?<Type>.*)-->[\r\n]*", RegexOptions.IgnoreCase);

            if (!start.Success)
            {
                return (template, null, null);
            }

            var type = start.Groups["Type"].Value.Trim();

            var startOuter = start.Index;
            var startInner = startOuter + start.Length;

#pragma warning disable MA0023 // Add RegexOptions.ExplicitCapture
            var end = new Regex($"<!--[\\s]*END:[\\s]*{type}[\\s]*-->[\r\n]*", RegexOptions.IgnoreCase).Match(template, startOuter);
#pragma warning restore MA0023 // Add RegexOptions.ExplicitCapture

            if (!end.Success)
            {
                return (template, null, null);
            }

            var endInner = end.Index;
            var endOuter = endInner + end.Length;

            var stringBuilder = Pool.Get();
            try
            {
                var replacement = template.Contains(NotificationsPlaceholder, StringComparison.OrdinalIgnoreCase) ? string.Empty : NotificationsPlaceholder;

                stringBuilder.Append(span[..startOuter]);
                stringBuilder.Append(replacement);
                stringBuilder.Append(span[endOuter..]);

                var inner = template[startInner..endInner];

                return (stringBuilder.ToString(), type, inner);
            }
            finally
            {
                Pool.Return(stringBuilder);
            }
        }
    }
}
