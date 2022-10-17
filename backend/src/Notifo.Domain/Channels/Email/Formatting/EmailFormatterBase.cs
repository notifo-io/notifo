// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Mjml.Net;
using Notifo.Infrastructure;

namespace Notifo.Domain.Channels.Email.Formatting
{
    public abstract class EmailFormatterBase
    {
        private static readonly MjmlOptions DefaultOptions = new MjmlOptions
        {
            KeepComments = true
        };
        private readonly IMjmlRenderer mjmlRenderer;

        protected EmailFormatterBase(IMjmlRenderer mjmlRenderer)
        {
            this.mjmlRenderer = mjmlRenderer;
        }

        protected static string ReadResource(string name)
        {
            var stream = typeof(EmailFormatterNormal).Assembly.GetManifestResourceStream($"Notifo.Domain.Channels.Email.Formatting.{name}")!;

            using (stream)
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        protected string MjmlToHtml(string? mjml, List<EmailFormattingError> errors)
        {
            if (string.IsNullOrWhiteSpace(mjml))
            {
                return mjml!;
            }

            using (Telemetry.Activities.StartActivity("MongoDbAppRepository/MjmlToHtmlAsync"))
            {
                var fixedHjml = mjmlRenderer.FixXML(mjml, DefaultOptions);

                var (html, mjmlErrors) = mjmlRenderer.Render(fixedHjml, DefaultOptions);

                if (mjmlErrors.Count > 0)
                {
                    errors.AddRange(mjmlErrors.Select(x => new EmailFormattingError(x.Error, EmailTemplateType.BodyHtml, x.Line ?? 0)));
                }

                return html;
            }
        }
    }
}
