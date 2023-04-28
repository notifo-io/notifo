﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Mjml.Net;
using Mjml.Net.Validators;
using Notifo.Domain.Resources;
using Notifo.Domain.Utils;
using Notifo.Infrastructure;

namespace Notifo.Domain.Channels.Email.Formatting;

internal static class MjmlRenderer
{
    private static readonly MjmlOptions OptionsOptimized = new MjmlOptions
    {
        KeepComments = false,
    };

    private static readonly MjmlOptions OptionsStrict = new MjmlOptions
    {
        ValidatorFactory = StrictValidatorFactory.Instance,
    };

    private static readonly IMjmlRenderer Renderer = new Mjml.Net.MjmlRenderer();

    public static string FixXML(string mjml)
    {
        return Renderer.FixXML(mjml, OptionsStrict);
    }

    public static (string? Html, List<TemplateError>? Errors) Render(string? mjml, bool strict, bool fix)
    {
        if (string.IsNullOrWhiteSpace(mjml))
        {
            return (null, null);
        }

        List<TemplateError>? errors = null;

        string? rendered = null;

        using (Telemetry.Activities.StartActivity("MongoDbAppRepository/MjmlToHtmlAsync"))
        {
            try
            {
                var options = strict ? OptionsStrict : OptionsOptimized;

                if (fix)
                {
                    mjml = Renderer.FixXML(mjml, options);
                }

                (rendered, var mjmlErrors) = Renderer.Render(mjml, options);

                errors = mjmlErrors?.Select(x => new TemplateError(x.Error, x.Line ?? -1, x.Column ?? -1)).ToList();
            }
            catch (Exception ex)
            {
                var error = new TemplateError(Texts.TemplateError, Exception: ex);

                errors ??= new List<TemplateError>(1);
                errors.Add(error);
            }
        }

        return (rendered, errors);
    }
}
