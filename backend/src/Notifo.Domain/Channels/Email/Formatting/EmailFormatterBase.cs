// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Xml;
using Mjml.Net;
using Mjml.Net.Validators;
using Notifo.Infrastructure;

namespace Notifo.Domain.Channels.Email.Formatting
{
    public abstract class EmailFormatterBase
    {
        private static readonly MjmlOptions OptionsOptimized = new MjmlOptions
        {
            // Always keep comments, because they are kept as control structures.
            KeepComments = true,
        };
        private static readonly MjmlOptions OptionsStrict = new MjmlOptions
        {
            // Use a strict validator for preview.
            ValidatorFactory = StrictValidatorFactory.Instance,

            // Always keep comments, because they are kept as control structures.
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

        protected (string? Html, ValidationErrors? Errors) MjmlToHtml(string? mjml, bool strict)
        {
            if (string.IsNullOrWhiteSpace(mjml))
            {
                return (null, null);
            }

            using (Telemetry.Activities.StartActivity("MongoDbAppRepository/MjmlToHtmlAsync"))
            {
                try
                {
                    var options = strict ?
                        OptionsStrict :
                        OptionsOptimized;

                    var fixedHjml = mjmlRenderer.FixXML(mjml, options);

                    var (html, errors) = mjmlRenderer.Render(fixedHjml, options);

                    return (html, errors);
                }
                catch (XmlException ex)
                {
                    var errors = new ValidationErrors
                    {
                        new ValidationError(ex.Message, ValidationErrorType.Other, ex.LineNumber, ex.LinePosition)
                    };

                    return (null, errors);
                }
            }
        }
    }
}
