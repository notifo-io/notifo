// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Fluid;
using Notifo.Domain.Apps;
using Notifo.Domain.Resources;
using Notifo.Domain.Users;
using Notifo.Domain.Utils;
using Notifo.Infrastructure;

namespace Notifo.Domain.Channels.Email.Formatting;

public sealed class EmailFormatterLiquid : IEmailFormatter
{
    private static readonly string DefaultBodyHtml;
    private static readonly string DefaultBodyText;
    private static readonly string DefaultSubject;
    private readonly IImageFormatter imageFormatter;
    private readonly IEmailUrl emailUrl;

    static EmailFormatterLiquid()
    {
        static string ReadResource(string name)
        {
            var stream = typeof(EmailFormatterLiquid).Assembly.GetManifestResourceStream($"Notifo.Domain.Channels.Email.Formatting.{name}")!;

            using (stream)
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        DefaultBodyHtml = ReadResource("DefaultHtml.liquid.mjml");
        DefaultBodyText = ReadResource("DefaultText.liquid.text");
        DefaultSubject = ReadResource("DefaultSubject.text");
    }

    public EmailFormatterLiquid(IImageFormatter imageFormatter, IEmailUrl emailUrl)
    {
        this.imageFormatter = imageFormatter;
        this.emailUrl = emailUrl;
    }

    public ValueTask<EmailTemplate> CreateInitialAsync(
        CancellationToken ct = default)
    {
        var template = new EmailTemplate
        {
            BodyHtml = DefaultBodyHtml,
            BodyText = DefaultBodyText,
            Subject = DefaultSubject
        };

        return new ValueTask<EmailTemplate>(template);
    }

    public ValueTask ParseAsync(EmailTemplate input, bool strict,
        CancellationToken ct = default)
    {
        var context = EmailContext.Create(PreviewData.Jobs, PreviewData.App, PreviewData.User, imageFormatter, emailUrl);

        Format(input, context, true, strict);

        if (context.Errors?.Count > 0)
        {
            throw new EmailFormattingException(context.Errors);
        }

        return default;
    }

    public ValueTask<FormattedEmail> FormatAsync(EmailTemplate input, IReadOnlyList<EmailJob> jobs, App app, User user, bool noCache = false,
        CancellationToken ct = default)
    {
        var context = EmailContext.Create(jobs, app, user, imageFormatter, emailUrl);

        var message = Format(input, context, noCache, false);

        return new ValueTask<FormattedEmail>(new FormattedEmail(message, context.Errors));
    }

    private static EmailMessage Format(EmailTemplate template, EmailContext context, bool noCache, bool strict)
    {
        var subject = string.Empty;

        if (!string.IsNullOrWhiteSpace(template.Subject))
        {
            subject = FormatSubject(template.Subject, context)!;
        }

        string? bodyText = null;

        if (!string.IsNullOrWhiteSpace(template.BodyText))
        {
            bodyText = FormatText(template.BodyText, context);
        }

        string? bodyHtml = null;

        if (!string.IsNullOrWhiteSpace(template.BodyHtml))
        {
            bodyHtml = FormatBodyHtml(template.BodyHtml, context, noCache, strict)!;
        }

        var firstJob = context.Jobs[0];

        var message = new EmailMessage
        {
            BodyHtml = bodyHtml,
            BodyText = bodyText,
            FromEmail = template.FromEmail.OrDefault(firstJob.FromEmail!),
            FromName = template.FromEmail.OrDefault(firstJob.FromName!),
            Subject = subject,
            ToEmail = context.User.EmailAddress!,
            ToName = context.User.FullName
        };

        if (string.IsNullOrWhiteSpace(bodyHtml) && string.IsNullOrWhiteSpace(bodyText))
        {
            context.AddError(EmailTemplateType.General, Texts.Email_TemplateUndefined);
        }

        return message;
    }

    private static string? FormatSubject(string template, EmailContext context)
    {
        return RenderTemplate(template, context, EmailTemplateType.Subject, false);
    }

    private static string? FormatText(string template, EmailContext context)
    {
        var result = RenderTemplate(template, context, EmailTemplateType.BodyText, false);

        context.ValidateTemplate(result, EmailTemplateType.BodyText);

        return result;
    }

    private static string? FormatBodyHtml(string template, EmailContext context, bool noCache, bool strict)
    {
        var result = RenderTemplate(template, context, EmailTemplateType.BodyHtml, noCache);

        context.ValidateTemplate(result, EmailTemplateType.BodyHtml);

        var (rendered, errors) = InternalMjmlRenderer.Render(result, strict, true);

        foreach (var error in errors.OrEmpty())
        {
            context.AddError(EmailTemplateType.BodyHtml, error);
        }

        return AddTrackingLinks(rendered, context);
    }

    private static string? RenderTemplate(string template, EmailContext context, EmailTemplateType type, bool noCache)
    {
        var (rendered, errors) = InternalLiquidRenderer.RenderLiquid(template, context, noCache);

        foreach (var error in errors.OrEmpty())
        {
            context.AddError(type, error);
        }

        return rendered;
    }

    private static string? AddTrackingLinks(string? html, EmailContext context)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return html!;
        }

        foreach (var job in context.Jobs)
        {
            if (!string.IsNullOrEmpty(job.Notification.TrackSeenUrl))
            {
                var trackingLink = job.Notification.HtmlTrackingLink(job.ConfigurationId);

                html = html.Replace("</body>", $"{trackingLink}</body>", StringComparison.OrdinalIgnoreCase);
            }
        }

        return html;
    }
}
