// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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
            var errors = new List<EmailFormattingError>();

            await ParseCoreAsync(template, errors);

            if (errors.Count > 0)
            {
                throw new EmailFormattingException(errors);
            }

            return template;
        }

        private async ValueTask ParseCoreAsync(EmailTemplate template, List<EmailFormattingError> errors)
        {
            if (!string.IsNullOrWhiteSpace(template.BodyHtml))
            {
                var html = await MjmlToHtmlAsync(template.BodyHtml, errors);

                var (body, error) = ParsedTemplate.Create(html);

                if (error != null)
                {
                    errors.Add(new EmailFormattingError(error, EmailTemplateType.BodyHtml));
                }

                template.ParsedBodyHtml = body;
            }

            if (!string.IsNullOrWhiteSpace(template.BodyText))
            {
                var (body, error) = ParsedTemplate.Create(template.BodyText);

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

        public async ValueTask<EmailPreview> FormatPreviewAsync(List<EmailJob> jobs, EmailTemplate template, App app, User user,
            CancellationToken ct = default)
        {
            var result = new EmailPreview
            {
                Errors = new List<EmailFormattingError>()
            };

            await ParseCoreAsync(template, result.Errors);

            result.Message = FormatCore(jobs, template, app, user, result.Errors);

            return result;
        }

        public ValueTask<EmailMessage> FormatAsync(List<EmailJob> jobs, EmailTemplate template, App app, User user,
            CancellationToken ct = default)
        {
            var errors = new List<EmailFormattingError>();

            var message = FormatCore(jobs, template, app, user, errors);

            if (errors.Count > 0)
            {
                throw new EmailFormattingException(errors);
            }

            return new ValueTask<EmailMessage>(message);
        }

        private EmailMessage FormatCore(List<EmailJob> jobs, EmailTemplate template, App app, User user,
            List<EmailFormattingError> errors)
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
                BodyHtml = FormatHtml(template, properties, jobs, errors),
                BodyText = FormatText(template, properties, jobs, errors),
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

        private string? FormatText(EmailTemplate template, Dictionary<string, string?> properties, List<EmailJob> jobs,
            List<EmailFormattingError> errors)
        {
            var parsed = template.ParsedBodyText;

            if (parsed == null)
            {
                return null;
            }

            var result = parsed.Format(jobs, properties, false, imageFormatter);

            ValidateResult(result, jobs, errors, EmailTemplateType.BodyText);

            return result;
        }

        private string? FormatHtml(EmailTemplate template, Dictionary<string, string?> properties, List<EmailJob> jobs,
            List<EmailFormattingError> errors)
        {
            var parsed = template.ParsedBodyHtml;

            if (parsed == null)
            {
                return null;
            }

            var result = parsed.Format(jobs, properties, true, imageFormatter);

            ValidateResult(result, jobs, errors, EmailTemplateType.BodyHtml);

            return result;
        }

        private static void ValidateResult(string result, List<EmailJob> jobs,
            List<EmailFormattingError> errors, EmailTemplateType template)
        {
            if (jobs.Any(x => !result.Contains(x.Notification.Formatting.Subject, StringComparison.OrdinalIgnoreCase)))
            {
                errors.Add(new EmailFormattingError(Texts.Email_TemplateInvalid, template));
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

        private async Task<string> MjmlToHtmlAsync(string mjml,
            List<EmailFormattingError> errors)
        {
            using (Telemetry.Activities.StartActivity("MongoDbAppRepository/MjmlToHtmlAsync"))
            {
                var result = await mjmlServices.Render(mjml);

                if (result?.Errors.Length > 0)
                {
                    errors.AddRange(result.Errors.Select(x => new EmailFormattingError(x.Message, EmailTemplateType.BodyHtml, x.Line)));
                }

                return result!.Html;
            }
        }
    }
}
