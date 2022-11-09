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
using Notifo.Domain.Users;
using Notifo.Domain.Utils;
using Notifo.Infrastructure;

namespace Notifo.Domain.Channels.Email.Formatting;

public sealed partial class EmailFormatterLiquid : EmailFormatterBase, IEmailFormatter
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

    public ValueTask<EmailTemplate> ParseAsync(EmailTemplate input, bool strict,
        CancellationToken ct = default)
    {
        var context = Context.Create(PreviewData.Jobs, PreviewData.App, PreviewData.User, imageFormatter, emailUrl);

        Format(input, context, true, strict);

        if (context.Errors?.Count > 0)
        {
            throw new EmailFormattingException(context.Errors);
        }

        return new ValueTask<EmailTemplate>(input);
    }

    public ValueTask<FormattedEmail> FormatAsync(EmailTemplate input, IReadOnlyList<EmailJob> jobs, App app, User user, bool noCache = false,
        CancellationToken ct = default)
    {
        var context = Context.Create(jobs, app, user, imageFormatter, emailUrl);

        var message = Format(input, context, noCache, false);

        return new ValueTask<FormattedEmail>(new FormattedEmail(message, context.Errors));
    }

    private EmailMessage Format(EmailTemplate template, Context context, bool noCache, bool strict)
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
            context.AddError(Texts.Email_TemplateUndefined, EmailTemplateType.General);
        }

        return message;
    }

    private static string? FormatSubject(string template, Context context)
    {
        return RenderTemplate(template, context, EmailTemplateType.Subject, false);
    }

    private static string? FormatText(string template, Context context)
    {
        var result = RenderTemplate(template, context, EmailTemplateType.BodyText, false);

        context.ValidateTemplate(result, EmailTemplateType.BodyText);

        return result;
    }

    private string? FormatBodyHtml(string template, Context context, bool noCache, bool strict)
    {
        var result = RenderTemplate(template, context, EmailTemplateType.BodyHtml, noCache);

        context.ValidateTemplate(result, EmailTemplateType.BodyHtml);

        var (html, mjmlErrors) = MjmlToHtml(result, strict);

        foreach (var mjmlError in mjmlErrors.OrEmpty())
        {
            context.AddError(mjmlError.Error, EmailTemplateType.BodyHtml, mjmlError.Line, mjmlError.Column);
        }

        return AddTrackingLinks(html, context);
    }

    private static string? RenderTemplate(string template, Context context, EmailTemplateType type, bool noCache)
    {
        var (fluidTemplate, error) = TemplateCache.Parse(template, noCache);

        if (error != null)
        {
            context.AddError(error.Message, type, error.Line, error.Column);
        }

        return fluidTemplate?.Render(context.TemplateContext);
    }

    private static string? AddTrackingLinks(string? html, Context context)
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
