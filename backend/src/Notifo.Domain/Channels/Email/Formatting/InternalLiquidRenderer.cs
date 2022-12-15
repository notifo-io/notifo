// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Fluid;
using Notifo.Domain.Resources;
using Notifo.Domain.Utils;
using Notifo.Infrastructure;

namespace Notifo.Domain.Channels.Email.Formatting;

internal static class InternalLiquidRenderer
{
    private static readonly TemplateOptions Options = new TemplateOptions();

    static InternalLiquidRenderer()
    {
        Options.MemberAccessStrategy = new UnsafeMemberAccessStrategy
        {
            IgnoreCasing = true
        };
    }

    public static (string? Result, List<TemplateError>? Errors) RenderLiquid(string? liquid, Dictionary<string, object> vars, bool noCache)
    {
        if (string.IsNullOrWhiteSpace(liquid))
        {
            return (null, null);
        }

        List<TemplateError>? errors = null;

        string? rendered = null;

        using (Telemetry.Activities.StartActivity("Email/RenderLiquid"))
        {
            var (fluidTemplate, fluidError) = LiquidCache.Parse(liquid, noCache);

            if (fluidError != null)
            {
                errors = new List<TemplateError>(2) { fluidError };
            }

            if (fluidTemplate != null)
            {
                try
                {
                    var templateContext = new TemplateContext(Options);

                    foreach (var (key, value) in vars)
                    {
                        templateContext.SetValue(key, value);
                    }

                    rendered = fluidTemplate.Render(templateContext);
                }
                catch (Exception ex)
                {
                    var error = new TemplateError(Texts.TemplateError, Exception: ex);

                    errors ??= new List<TemplateError>(1);
                    errors.Add(error);
                }
            }
        }

        return (rendered, errors);
    }
}
