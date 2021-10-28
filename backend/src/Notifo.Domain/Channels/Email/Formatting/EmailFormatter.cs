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
            var stream = typeof(EmailFormatter).Assembly.GetManifestResourceStream($"Notifo.Domain.Channels.Email.Formatting.{name}")!;

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

        public async ValueTask<EmailMessage> FormatPreviewAsync(List<EmailJob> jobs, EmailTemplate template, App app, User user,
            CancellationToken ct = default)
        {
            await ParseAsync(template);

            return FormatCore(jobs, template, app, user);
        }

        public ValueTask<EmailMessage> FormatAsync(List<EmailJob> jobs, EmailTemplate template, App app, User user,
            CancellationToken ct = default)
        {
            return new ValueTask<EmailMessage>(FormatCore(jobs, template, app, user));
        }

        private EmailMessage FormatCore(List<EmailJob> jobs, EmailTemplate template, App app, User user)
        {
            var firstJob = jobs[0];

            var properties = CreateProperties(jobs, app, user, firstJob);

            var fromEmail = firstJob.FromEmail;
            var fromName = firstJob.FromName;

            if (string.IsNullOrWhiteSpace(fromEmail))
            {
                fromEmail = template.FromEmail;
            }

            if (string.IsNullOrWhiteSpace(fromName))
            {
                fromName = template.FromName;
            }

            var mailMessage = new EmailMessage
            {
                Subject = FormatSubject(template, properties),
                BodyHtml = FormatHtml(template, properties, jobs),
                BodyText = FormatText(template, properties, jobs),
                FromEmail = fromEmail!,
                FromName = fromName,
                ToEmail = user.EmailAddress!,
                ToName = user.FullName,
            };

            return mailMessage;
        }

        private static string FormatSubject(EmailTemplate template, Dictionary<string, string?> properties)
        {
            return template.Subject.Format(properties);
        }

        private string? FormatText(EmailTemplate template, Dictionary<string, string?> properties, List<EmailJob> jobs)
        {
            var parsed = template.ParsedBodyText;

            if (parsed == null)
            {
                return null;
            }

            var result = parsed.Format(jobs, properties, false, imageFormatter);

            ValidateResult(template.BodyText!, result, jobs);

            return result;
        }

        private string? FormatHtml(EmailTemplate template, Dictionary<string, string?> properties, List<EmailJob> jobs)
        {
            var parsed = template.ParsedBodyHtml;

            if (parsed == null)
            {
                return null;
            }

            var result = parsed.Format(jobs, properties, true, imageFormatter);

            ValidateResult(template.BodyText!, result, jobs);

            return result;
        }

        private static void ValidateResult(string markup, string result, List<EmailJob> jobs)
        {
            if (jobs.Any(x => !result.Contains(x.Notification.Formatting.Subject, StringComparison.OrdinalIgnoreCase)))
            {
                var errors = new[]
                {
                    new EmailFormattingError(Texts.Email_TemplateInvalid, 0)
                };

                throw new EmailFormattingException(markup, errors);
            }
        }

        private static Dictionary<string, string?> CreateProperties(List<EmailJob> jobs, App app, User user, EmailJob firstJob)
        {
            var notification = firstJob.Notification;

            var formattingProperties = new Dictionary<string, string?>(StringComparer.InvariantCulture)
            {
                ["app.name"] = app.Name,
                ["user.name"] = user.FullName,
                ["user.nameFull"] = user.FullName,
                ["user.fullName"] = user.FullName,
                ["user.email"] = user.EmailAddress,
                ["user.emailAddress"] = user.EmailAddress,
                ["notification.subject"] = notification.Formatting.Subject,
                ["notification.body"] = notification.Formatting.Body
            };

            foreach (var job in jobs)
            {
                var properties = job.Notification.Properties;

                if (properties != null)
                {
                    foreach (var (key, value) in properties)
                    {
                        formattingProperties[$"notification.custom.{key}"] = value;
                    }
                }
            }

            return formattingProperties;
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
