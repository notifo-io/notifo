// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Fluid;
using Mjml.Net;
using Notifo.Domain.Apps;
using Notifo.Domain.Resources;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Users;
using Notifo.Domain.Utils;
using Notifo.Infrastructure;

namespace Notifo.Domain.Channels.Email.Formatting
{
    public sealed class EmailFormatterLiquid : EmailFormatterBase, IEmailFormatter
    {
        private static readonly TemplateOptions Options = new TemplateOptions();
        private static readonly string DefaultBodyHtml;
        private static readonly string DefaultBodyText;
        private static readonly string DefaultSubject;
        private readonly IImageFormatter imageFormatter;
        private readonly IEmailUrl emailUrl;

        static EmailFormatterLiquid()
        {
            Options.MemberAccessStrategy.IgnoreCasing = true;
            Options.MemberAccessStrategy.Register<App>();
            Options.MemberAccessStrategy.Register<EmailNotification>();
            Options.MemberAccessStrategy.Register<EmailNotification[]>();
            Options.MemberAccessStrategy.Register<User>();

            DefaultBodyHtml = ReadResource("DefaultHtml.liquid.mjml");
            DefaultBodyText = ReadResource("DefaultText.liquid.text");
            DefaultSubject = ReadResource("DefaultSubject.text");
        }

        public EmailFormatterLiquid(IImageFormatter imageFormatter, IEmailUrl emailUrl, IMjmlRenderer mjmlRenderer)
            : base(mjmlRenderer)
        {
            this.imageFormatter = imageFormatter;
            this.emailUrl = emailUrl;
        }

        public bool Accepts(string? kind)
        {
            return string.Equals(kind, "Liquid", StringComparison.OrdinalIgnoreCase);
        }

        public ValueTask<EmailTemplate> CreateInitialAsync(string? kind = null,
            CancellationToken ct = default)
        {
            var template = new EmailTemplate
            {
                Kind = kind,
                BodyHtml = DefaultBodyHtml,
                BodyText = DefaultBodyText,
                Subject = DefaultSubject
            };

            return new ValueTask<EmailTemplate>(template);
        }

        public async ValueTask<EmailTemplate> ParseAsync(EmailTemplate template,
            CancellationToken ct = default)
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
            await FormatAsync(EmailJob.ForPreview, template, new App("1", default), new User("1", "1", default));

            if (string.IsNullOrWhiteSpace(template.BodyHtml) && string.IsNullOrWhiteSpace(template.BodyText))
            {
                errors.Add(new EmailFormattingError(Texts.Email_TemplateUndefined, EmailTemplateType.General));
            }
        }

        public ValueTask<FormattedEmail> FormatAsync(List<EmailJob> jobs, EmailTemplate template, App app, User user, bool noCache = false,
            CancellationToken ct = default)
        {
            var errors = new List<EmailFormattingError>();

            var message = Format(jobs, template, app, user, errors, noCache);

            return new ValueTask<FormattedEmail>(new FormattedEmail(message, errors));
        }

        private EmailMessage Format(List<EmailJob> jobs, EmailTemplate template, App app, User user,
            List<EmailFormattingError> errors, bool noCache)
        {
            var context = CreateContext(jobs.Select(x => x.Notification), app, user);

            var subject = string.Empty;

            if (!string.IsNullOrWhiteSpace(template.Subject))
            {
                subject = FormatSubject(template.Subject, context, errors)!;
            }

            string? bodyText = null;

            if (!string.IsNullOrWhiteSpace(template.BodyText))
            {
                bodyText = FormatText(template.BodyText, context, jobs, errors)!;
            }

            string? bodyHtml = null;

            if (!string.IsNullOrWhiteSpace(template.BodyHtml))
            {
                bodyHtml = FormatBodyHtml(template.BodyHtml, context, jobs, user.EmailAddress!, errors, noCache)!;
            }

            var message = new EmailMessage
            {
                BodyHtml = bodyHtml,
                BodyText = bodyText,
                FromEmail = template.FromEmail.OrDefault(jobs[0].FromEmail!),
                FromName = template.FromEmail.OrDefault(jobs[0].FromName!),
                Subject = subject,
                ToEmail = user.EmailAddress!,
                ToName = user.FullName
            };

            return message;
        }

        private static string? FormatSubject(string template, TemplateContext context,
            List<EmailFormattingError> errors)
        {
            return RenderTemplate(template, context, errors, EmailTemplateType.Subject, false);
        }

        private static string? FormatText(string template, TemplateContext context, List<EmailJob> jobs,
            List<EmailFormattingError> errors)
        {
            var result = RenderTemplate(template, context, errors, EmailTemplateType.BodyText, false);

            ValidateResult(result, jobs, errors, EmailTemplateType.BodyText);

            return result;
        }

        private string? FormatBodyHtml(string template, TemplateContext context, List<EmailJob> jobs, string emailAddress,
            List<EmailFormattingError> errors, bool noCache)
        {
            var result = RenderTemplate(template, context, errors, EmailTemplateType.BodyHtml, noCache);

            ValidateResult(result, jobs, errors, EmailTemplateType.BodyHtml);

            return AddTrackingLinks(MjmlToHtml(result, errors), emailAddress, jobs);
        }

        private static void ValidateResult(string? result, List<EmailJob> jobs,
            List<EmailFormattingError> errors, EmailTemplateType type)
        {
            if (result == null)
            {
                return;
            }

            if (jobs.Any(x => !result.Contains(x.Notification.Formatting.Subject, StringComparison.OrdinalIgnoreCase)))
            {
                errors.Add(new EmailFormattingError(Texts.Email_TemplateLiquidInvalid, type));
            }
        }

        private static string? RenderTemplate(string template, TemplateContext context,
            List<EmailFormattingError> errors, EmailTemplateType type, bool noCache)
        {
            var (fluidTemplate, error) = TemplateCache.Parse(template, noCache);

            if (error != null)
            {
                errors.Add(new EmailFormattingError(error.Message, type, error.Line, error.Column));
            }

            return fluidTemplate?.Render(context);
        }

        private TemplateContext CreateContext(IEnumerable<BaseUserNotification> notifications, App app, User user)
        {
            var context = new TemplateContext(Options);

            var emailNotifications = notifications.Select(x => new EmailNotification(x, user.EmailAddress, imageFormatter)).ToArray();

            context.SetValue("app", app);
            context.SetValue("user", user);
            context.SetValue("notifications", emailNotifications);
            context.SetValue("preferencesUrl", emailUrl.EmailPreferences(user.ApiKey, user.PreferredLanguage));

            return context;
        }

        private static string? AddTrackingLinks(string? html, string emailAddress, List<EmailJob> jobs)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return html!;
            }

            foreach (var job in jobs)
            {
                if (!string.IsNullOrEmpty(job.Notification.TrackSeenUrl))
                {
                    var trackingLink = ChannelExtensions.HtmlTrackingLink(job.Notification, emailAddress);

                    html = html.Replace("</body>", $"{trackingLink}</body>", StringComparison.OrdinalIgnoreCase);
                }
            }

            return html;
        }
    }
}
