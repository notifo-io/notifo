// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mjml.AspNetCore;
using Notifo.Domain.Apps;
using Notifo.Domain.Resources;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Users;
using Notifo.Domain.Utils;
using Notifo.Infrastructure;

namespace Notifo.Domain.Channels.Email.Formatting
{
    public sealed class EmailFormatter : IEmailFormatter
    {
        private static readonly string DefaultBodyHtml;
        private static readonly string DefaultBodyText;
        private static readonly string DefaultSubject;
        private readonly IImageFormatter imageFormatter;
        private readonly IMjmlServices mjmlServices;

        static EmailFormatter()
        {
            DefaultBodyHtml = ReadResource("DefaultHtml.mjml");
            DefaultBodyText = ReadResource("DefaultText.text");

            DefaultSubject = ReadResource("DefaultSubject.text");
        }

        public EmailFormatter(IImageFormatter imageFormatter, IMjmlServices mjmlServices)
        {
            this.imageFormatter = imageFormatter;

            this.mjmlServices = mjmlServices;
        }

        private static string ReadResource(string name)
        {
            var stream = typeof(EmailFormatter).Assembly.GetManifestResourceStream($"Notifo.Domain.Channels.Email.{name}")!;

            using (stream)
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public async ValueTask<EmailTemplate> CreateInitialAsync()
        {
            var template = new EmailTemplate
            {
                BodyHtml = DefaultBodyHtml,
                BodyText = DefaultBodyText,
                Subject = DefaultSubject
            };

            await ParseAsync(template);

            return template;
        }

        public async ValueTask<EmailTemplate> ParseAsync(EmailTemplate template)
        {
            if (!string.IsNullOrWhiteSpace(template.BodyHtml))
            {
                var html = await MjmlToHtmlAsync(template.BodyHtml);

                template.ParsedBodyHtml = ParsedTemplate.Create(html);
            }

            if (!string.IsNullOrWhiteSpace(template.BodyText))
            {
                template.ParsedBodyText = ParsedTemplate.Create(template.BodyText);
            }

            if (template.ParsedBodyHtml == null && template.ParsedBodyText == null)
            {
                throw new DomainException(Texts.Email_TemplateUndefined);
            }

            return template;
        }

        public async ValueTask<EmailMessage> FormatPreviewAsync(IEnumerable<BaseUserNotification> notifications, EmailTemplate template, App app, User user,
            CancellationToken ct = default)
        {
            await ParseAsync(template);

            return FormatCore(notifications, template, app, user);
        }

        public ValueTask<EmailMessage> FormatAsync(IEnumerable<BaseUserNotification> notifications, EmailTemplate template, App app, User user,
            CancellationToken ct = default)
        {
            return new ValueTask<EmailMessage>(FormatCore(notifications, template, app, user));
        }

        private EmailMessage FormatCore(IEnumerable<BaseUserNotification> notifications, EmailTemplate template, App app, User user)
        {
            var properties = CreateProperties(notifications, app, user);

            var mailMessage = new EmailMessage
            {
                Subject = FormatSubject(template, properties),
                BodyHtml = FormatHtml(template, properties, notifications),
                BodyText = FormatText(template, properties, notifications),
                ToEmail = user.EmailAddress,
                ToName = user.FullName
            };

            return mailMessage;
        }

        private static string FormatSubject(EmailTemplate template, Dictionary<string, string?> properties)
        {
            return template.Subject.Format(properties);
        }

        private string? FormatText(EmailTemplate template, Dictionary<string, string?> properties, IEnumerable<BaseUserNotification> notifications)
        {
            var parsed = template.ParsedBodyText;

            if (parsed == null)
            {
                return null;
            }

            var result = parsed.Format(notifications, properties, false, imageFormatter);

            ValidateResult(template.BodyText!, result, notifications);

            return result;
        }

        private string? FormatHtml(EmailTemplate template, Dictionary<string, string?> properties, IEnumerable<BaseUserNotification> notifications)
        {
            var parsed = template.ParsedBodyHtml;

            if (parsed == null)
            {
                return null;
            }

            var result = parsed.Format(notifications, properties, true, imageFormatter);

            ValidateResult(template.BodyText!, result, notifications);

            return result;
        }

        private static void ValidateResult(string markup, string result, IEnumerable<BaseUserNotification> notifications)
        {
            if (notifications.Any(x => !result.Contains(x.Formatting.Subject, StringComparison.OrdinalIgnoreCase)))
            {
                var errors = new[]
                {
                    new EmailFormattingError(Texts.Email_TemplateInvalid, 0)
                };

                throw new EmailFormattingException(markup, errors);
            }
        }

        private static Dictionary<string, string?> CreateProperties(IEnumerable<BaseUserNotification> notifications, App app, User user)
        {
            var firstNotification = notifications.First();

            var properties = new Dictionary<string, string?>
            {
                ["app.name"] = app.Name,
                ["user.name"] = user.FullName,
                ["user.nameFull"] = user.FullName,
                ["user.fullName"] = user.FullName,
                ["user.email"] = user.EmailAddress,
                ["user.emailAddress"] = user.EmailAddress,
                ["notification.subject"] = firstNotification.Formatting.Subject,
                ["notification.body"] = firstNotification.Formatting.Body
            };

            return properties;
        }

        private async Task<string> MjmlToHtmlAsync(string mjml)
        {
            using (Telemetry.Activities.StartActivity("MongoDbAppRepository/MjmlToHtmlAsync"))
            {
                var result = await mjmlServices.Render(mjml);

                if (result?.Errors.Length > 0)
                {
                    var errors = result.Errors.Select(x => new EmailFormattingError(x.Message, x.Line)).ToList();

                    throw new EmailFormattingException(mjml, errors);
                }

                return result!.Html;
            }
        }
    }
}
