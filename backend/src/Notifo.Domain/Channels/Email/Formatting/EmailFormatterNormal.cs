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
    public sealed partial class EmailFormatterNormal : EmailFormatterBase, IEmailFormatter
    {
        private static readonly string DefaultBodyHtml;
        private static readonly string DefaultBodyText;
        private static readonly string DefaultSubject;
        private readonly IImageFormatter imageFormatter;
        private readonly IEmailUrl emailUrl;

        static EmailFormatterNormal()
        {
            DefaultBodyHtml = ReadResource("DefaultHtml.mjml");
            DefaultBodyText = ReadResource("DefaultText.text");
            DefaultSubject = ReadResource("DefaultSubject.text");
        }

        public EmailFormatterNormal(IImageFormatter imageFormatter, IEmailUrl emailUrl, IMjmlRenderer mjmlRenderer)
            : base(mjmlRenderer)
        {
            this.imageFormatter = imageFormatter;
            this.emailUrl = emailUrl;
        }

        public bool Accepts(string? kind)
        {
            return kind == null || string.Equals(kind, "Default", StringComparison.OrdinalIgnoreCase);
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

            return await ParseAsync(template, false, ct);
        }

        public ValueTask<EmailTemplate> ParseAsync(EmailTemplate input, bool strict,
            CancellationToken ct = default)
        {
            var context = Context.Create(PreviewData.Jobs, PreviewData.App, PreviewData.User, emailUrl);

            var parsed = Parse(input, context, strict);

            if (context.Errors?.Count > 0)
            {
                throw new EmailFormattingException(context.Errors);
            }

            return new ValueTask<EmailTemplate>(parsed);
        }

        public ValueTask<FormattedEmail> FormatAsync(EmailTemplate input, IReadOnlyList<EmailJob> jobs, App app, User user, bool noCache = false,
            CancellationToken ct = default)
        {
            var context = Context.Create(jobs, app, user, emailUrl);

            var message = Format(input, context);

            return new ValueTask<FormattedEmail>(new FormattedEmail(message, context.Errors));
        }

        private EmailTemplate Parse(EmailTemplate template, Context context, bool strict)
        {
            if (!string.IsNullOrWhiteSpace(template.BodyHtml))
            {
                var (html, mjmlErrors) = MjmlToHtml(template.BodyHtml, strict);

                foreach (var mjmlError in mjmlErrors.OrEmpty())
                {
                    context.AddError(mjmlError.Error, EmailTemplateType.BodyHtml, mjmlError.Line, mjmlError.Column);
                }

                var (body, error) = ParsedEmailTemplate.Create(html);

                if (error != null)
                {
                    context.AddError(error, EmailTemplateType.BodyHtml);
                }

                template = template with { ParsedBodyHtml = body };
            }

            if (!string.IsNullOrWhiteSpace(template.BodyText))
            {
                var (body, error) = ParsedEmailTemplate.Create(template.BodyText);

                if (error != null)
                {
                    context.AddError(error, EmailTemplateType.BodyText);
                }

                template = template with { ParsedBodyText = body };
            }

            Format(template, context);

            return template;
        }

        private EmailMessage Format(EmailTemplate template, Context context)
        {
            var subject = string.Empty;

            if (!string.IsNullOrWhiteSpace(template.Subject))
            {
                subject = template.Subject.Format(context.Properties);
            }

            string? bodyText = null;

            if (template.ParsedBodyText != null)
            {
                bodyText = FormatBodyText(template.ParsedBodyText, context)!;
            }

            string? bodyHtml = null;

            if (template.ParsedBodyHtml != null)
            {
                bodyHtml = FormatBodyHtml(template.ParsedBodyHtml, context)!;
            }

            var firstJob = context.Jobs[0];

            var message = new EmailMessage
            {
                BodyHtml = bodyHtml,
                BodyText = bodyText,
                FromEmail = template.FromEmail.OrDefault(firstJob.FromEmail!),
                FromName = template.FromEmail.OrDefault(firstJob.FromName!),
                Subject = subject,
                ToEmail = context.EmailAddress!,
                ToName = context.User.FullName
            };

            if (string.IsNullOrWhiteSpace(bodyHtml) && string.IsNullOrWhiteSpace(bodyText))
            {
                context.AddError(Texts.Email_TemplateUndefined, EmailTemplateType.General);
            }

            return message;
        }

        private string? FormatBodyText(ParsedEmailTemplate template, Context context)
        {
            var result = template.Format(context.Jobs, context.Properties, context.EmailAddress!, false, imageFormatter);

            context.ValidateTemplate(result, EmailTemplateType.BodyText);

            return result;
        }

        private string? FormatBodyHtml(ParsedEmailTemplate template, Context context)
        {
            var result = template.Format(context.Jobs, context.Properties, context.EmailAddress!, true, imageFormatter);

            context.ValidateTemplate(result, EmailTemplateType.BodyHtml);

            return result;
        }
    }
}
