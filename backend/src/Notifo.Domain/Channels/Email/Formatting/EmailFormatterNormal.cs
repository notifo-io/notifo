// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Mjml.Net;
using Notifo.Domain.Apps;
using Notifo.Domain.Resources;
using Notifo.Domain.Users;
using Notifo.Domain.Utils;
using Notifo.Infrastructure;

namespace Notifo.Domain.Channels.Email.Formatting
{
    public sealed class EmailFormatterNormal : EmailFormatterBase, IEmailFormatter
    {
        private static readonly string DefaultBodyHtml;
        private static readonly string DefaultBodyText;
        private static readonly string DefaultSubject;
        private readonly IImageFormatter imageFormatter;

        static EmailFormatterNormal()
        {
            DefaultBodyHtml = ReadResource("DefaultHtml.mjml");
            DefaultBodyText = ReadResource("DefaultText.text");
            DefaultSubject = ReadResource("DefaultSubject.text");
        }

        public EmailFormatterNormal(IImageFormatter imageFormatter, IMjmlRenderer mjmlRenderer)
            : base(mjmlRenderer)
        {
            this.imageFormatter = imageFormatter;
        }

        public bool Accepts(EmailTemplate template)
        {
            return template.Kind == null || string.Equals(template.Kind, "Default", StringComparison.OrdinalIgnoreCase);
        }

        public async ValueTask<EmailTemplate> CreateInitialAsync(string? kind = null,
            CancellationToken ct = default)
        {
            var template = new EmailTemplate
            {
                Kind = kind,
                BodyHtml = DefaultBodyHtml,
                BodyText = DefaultBodyText,
                Subject = DefaultSubject
            };

            return await ParseAsync(template, ct);
        }

        public ValueTask<EmailTemplate> ParseAsync(EmailTemplate template,
            CancellationToken ct = default)
        {
            var errors = new List<EmailFormattingError>();

            Parse(template, errors);

            if (errors.Count > 0)
            {
                throw new EmailFormattingException(errors);
            }

            return new ValueTask<EmailTemplate>(template);
        }

        private void Parse(EmailTemplate template, List<EmailFormattingError> errors)
        {
            Format(EmailJob.ForPreview, template, new App(), new User(), errors);

            if (!string.IsNullOrWhiteSpace(template.BodyHtml))
            {
                var html = MjmlToHtml(template.BodyHtml, errors);

                var (body, error) = ParsedEmailTemplate.Create(html);

                if (error != null)
                {
                    errors.Add(new EmailFormattingError(error, EmailTemplateType.BodyHtml));
                }

                template.ParsedBodyHtml = body;
            }

            if (!string.IsNullOrWhiteSpace(template.BodyText))
            {
                var (body, error) = ParsedEmailTemplate.Create(template.BodyText);

                if (error != null)
                {
                    errors.Add(new EmailFormattingError(error, EmailTemplateType.BodyText));
                }

                template.ParsedBodyText = body;
            }

            if (errors.Count == 0 && template.ParsedBodyHtml == null && template.ParsedBodyText == null)
            {
                errors.Add(new EmailFormattingError(Texts.Email_TemplateUndefined, EmailTemplateType.General));
            }
        }

        public ValueTask<FormattedEmail> FormatAsync(List<EmailJob> jobs, EmailTemplate template, App app, User user, bool noCache = false,
            CancellationToken ct = default)
        {
            var errors = new List<EmailFormattingError>();

            var message = Format(jobs, template, app, user, errors);

            return new ValueTask<FormattedEmail>(new FormattedEmail(message, errors));
        }

        private EmailMessage Format(List<EmailJob> jobs, EmailTemplate template, App app, User user,
            List<EmailFormattingError> errors)
        {
            var properties = CreateProperties(jobs, app, user, jobs[0]);

            var message = new EmailMessage
            {
                FromEmail = template.FromEmail.OrDefault(jobs[0].FromEmail!),
                FromName = template.FromEmail.OrDefault(jobs[0].FromName!),
                ToEmail = user.EmailAddress!,
                ToName = user.FullName,
            };

            if (!string.IsNullOrWhiteSpace(template.Subject))
            {
                message.Subject = FormatSubject(template.Subject, properties)!;
            }

            if (template.ParsedBodyText != null)
            {
                message.BodyText = FormatBodyText(template.ParsedBodyText, properties, jobs, message.ToEmail, errors)!;
            }

            if (template.ParsedBodyHtml != null)
            {
                message.BodyHtml = FormatBodyHtml(template.ParsedBodyHtml, properties, jobs, message.ToEmail, errors)!;
            }

            return message;
        }

        private static string FormatSubject(string template, Dictionary<string, string?> properties)
        {
            return template.Format(properties);
        }

        private string? FormatBodyText(ParsedEmailTemplate template, Dictionary<string, string?> properties, List<EmailJob> jobs, string emailAddress,
            List<EmailFormattingError> errors)
        {
            var result = template.Format(jobs, properties, emailAddress, false, imageFormatter);

            ValidateResult(result, jobs, errors, EmailTemplateType.BodyText);

            return result;
        }

        private string? FormatBodyHtml(ParsedEmailTemplate template, Dictionary<string, string?> properties, List<EmailJob> jobs, string emailAddress,
            List<EmailFormattingError> errors)
        {
            var result = template.Format(jobs, properties, emailAddress, true, imageFormatter);

            ValidateResult(result, jobs, errors, EmailTemplateType.BodyHtml);

            return result;
        }

        private static void ValidateResult(string result, List<EmailJob> jobs,
            List<EmailFormattingError> errors, EmailTemplateType type)
        {
            if (jobs.Any(x => !result.Contains(x.Notification.Formatting.Subject, StringComparison.OrdinalIgnoreCase)))
            {
                errors.Add(new EmailFormattingError(Texts.Email_TemplateNormalInvalid, type));
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
    }
}
