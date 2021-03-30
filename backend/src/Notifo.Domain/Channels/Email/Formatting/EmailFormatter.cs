// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fluid;
using Mjml.AspNetCore;
using Notifo.Domain.Apps;
using Notifo.Domain.Resources;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Users;
using Notifo.Domain.Utils;
using Notifo.Infrastructure;
using Squidex.Log;

namespace Notifo.Domain.Channels.Email.Formatting
{
    public sealed class EmailFormatter : IEmailFormatter
    {
        private const string DefaultSenderEmail = "noreply@notifo.io";
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

        public Task<EmailTemplate> GetDefaultTemplateAsync()
        {
            var template = new EmailTemplate
            {
                BodyHtml = DefaultBodyHtml,
                BodyText = DefaultBodyText,
                Subject = DefaultSubject
            };

            return Task.FromResult(template);
        }

        public async Task<EmailMessage> FormatAsync(IEnumerable<UserNotification> notifications, EmailTemplate template, App app, User user, bool noCache)
        {
            var context = CreateContext(notifications, app, user);

            var mailMessage = new EmailMessage
            {
                BodyHtml = await FormatHtml(template, context, notifications, noCache),
                BodyText = FormatText(template, context, notifications, noCache),
                FromEmail = app.EmailAddress.OrDefault(DefaultSenderEmail),
                FromName = app.EmailName.OrDefault(app.Name),
                Subject = FormatSubject(template, context, noCache),
                ToEmail = user.EmailAddress,
                ToName = user.FullName
            };

            return mailMessage;
        }

        public Task<EmailMessage> FormatAsync(IEnumerable<UserNotification> notifications, App app, User user)
        {
            var first = notifications.First();

            if (app.EmailTemplates.TryGetValue(first.UserLanguage, out var template))
            {
                return FormatAsync(notifications, template, app, user, false);
            }
            else
            {
                throw new DomainException(Texts.Email_TemplateNotFound);
            }
        }

        private static string? FormatText(EmailTemplate template, TemplateContext context, IEnumerable<UserNotification> notifications, bool noCache)
        {
            var markup = template.BodyText;

            if (string.IsNullOrWhiteSpace(markup))
            {
                return null;
            }

            var result = RenderTemplate(markup, context, noCache);

            ValidateResult(markup, result, notifications);

            return result;
        }

        private static string FormatSubject(EmailTemplate template, TemplateContext context, bool noCache)
        {
            var markup = template.Subject;

            if (string.IsNullOrWhiteSpace(markup))
            {
                return string.Empty;
            }

            var result = RenderTemplate(markup, context, noCache);

            return result;
        }

        private async Task<string?> FormatHtml(EmailTemplate template, TemplateContext context, IEnumerable<UserNotification> notifications, bool noCache)
        {
            var markup = template.BodyHtml;

            if (string.IsNullOrWhiteSpace(markup))
            {
                return markup;
            }

            var result = RenderTemplate(markup, context, noCache);

            var html = await MjmlToHtmlAsync(result);

            if (string.IsNullOrWhiteSpace(html))
            {
                return html;
            }

            ValidateResult(markup, html, notifications);

            return html;
        }

        private static void ValidateResult(string markup, string result, IEnumerable<UserNotification> notifications)
        {
            if (notifications.Any(x => !result.Contains(x.Formatting.Subject)))
            {
                var errors = new[]
                {
                    new EmailFormattingError(Texts.Email_TemplateInvalid, 0)
                };

                throw new EmailFormattingException(markup, errors);
            }
        }

        private static string RenderTemplate(string template, TemplateContext context, bool noCache)
        {
            try
            {
                using (Profiler.TraceMethod<EmailFormatter>())
                {
                    var parsedTemplate = TemplateCache.Parse(template, noCache);

                    return parsedTemplate.Render(context);
                }
            }
            catch (TemplateParseException ex)
            {
                var errors = ex.Errors.Select(x => new EmailFormattingError(x)).ToList();

                throw new EmailFormattingException(template, errors);
            }
        }

        private TemplateContext CreateContext(IEnumerable<UserNotification> notifications, App app, User user)
        {
            var context = new TemplateContext();

            var emailNotifications = notifications.Select(x => EmailNotification.Create(x, user.EmailAddress, imageFormatter)).ToList();

            context.SetValue("app", app);
            context.SetValue("user", user);
            context.SetValue("notifications", emailNotifications);

            context.MemberAccessStrategy.MemberNameStrategy = MemberNameStrategies.CamelCase;
            context.MemberAccessStrategy.Register<EmailNotification>();

            return context;
        }

        private async Task<string> MjmlToHtmlAsync(string mjml)
        {
            using (Profiler.TraceMethod<EmailFormatter>())
            {
                var result = await mjmlServices.Render(mjml);

                if (result.Errors != null && result.Errors.Length > 0)
                {
                    var errors = result.Errors.Select(x => new EmailFormattingError(x.Message, x.Line)).ToList();

                    throw new EmailFormattingException(mjml, errors);
                }

                return result.Html;
            }
        }
    }
}
