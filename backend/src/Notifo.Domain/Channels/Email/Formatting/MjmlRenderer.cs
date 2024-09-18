// ==========================================================================
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
using MjmlInternalRenderer = Mjml.Net.MjmlRenderer;

namespace Notifo.Domain.Channels.Email.Formatting;

internal static class MjmlRenderer
{
    private static readonly MjmlOptions DefaultOptions = new MjmlOptions().WithPostProcessors();

    private static readonly MjmlOptions OptionsOptimized = DefaultOptions with
    {
        KeepComments = false,
    };

    private static readonly MjmlOptions OptionsStrict = DefaultOptions with
    {
        Validator = StrictValidator.Instance,
    };

    private static readonly IMjmlRenderer Renderer =
        new MjmlInternalRenderer()
            .AddHtmlAttributes()
            .AddList();

    public static async ValueTask<(string? Html, List<TemplateError>? Errors)> RenderAsync(string? mjml, bool strict)
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
                var options =
                    strict ?
                    OptionsStrict :
                    OptionsOptimized;

                (rendered, var mjmlErrors) = await Renderer.RenderAsync(mjml, options);

                errors = mjmlErrors?.Select(x => new TemplateError(
                    x.Error,
                    x.Position.LineNumber,
                    x.Position.LinePosition)
                ).ToList();
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
