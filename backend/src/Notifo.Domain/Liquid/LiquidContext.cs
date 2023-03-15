// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Fluid;
using Notifo.Domain.Apps;
using Notifo.Domain.Resources;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Users;
using Notifo.Domain.Utils;
using Notifo.Infrastructure;

namespace Notifo.Domain.Liquid;

public sealed class LiquidContext : TemplateContext
{
    private static readonly TemplateOptions DefaultOptions = new TemplateOptions
    {
        MemberAccessStrategy = new UnsafeMemberAccessStrategy
        {
            IgnoreCasing = true
        }
    };

    private LiquidContext()
        : base(DefaultOptions)
    {
    }

    public static LiquidContext Create(IEnumerable<(BaseUserNotification Notification, Guid ConfigurationId)> items, App app, User user,
        string channel,
        string imagePresetSmall,
        string imagePresetLarge,
        IImageFormatter imageFormatter)
    {
        var context = new LiquidContext();

        var notifications = new List<LiquidNotification>();

        foreach (var (notification, configurationId) in items)
        {
            notifications.Add(
                new LiquidNotification(
                    notification,
                    configurationId,
                    channel,
                    imagePresetSmall,
                    imagePresetLarge,
                    imageFormatter));

            var jobProperties = notification.Properties;

            if (jobProperties != null)
            {
                foreach (var (key, value) in jobProperties)
                {
                    context.SetValue($"notification.custom.{key}", value);
                }
            }
        }

        context.SetValue("app", new LiquidApp(app));
        context.SetValue("notification", notifications[0]);
        context.SetValue("notifications", notifications);
        context.SetValue("user", new LiquidUser(user));

        return context;
    }

    public (string? Result, List<TemplateError>? Errors) Render(string? liquid, bool noCache)
    {
        if (string.IsNullOrWhiteSpace(liquid))
        {
            return (null, null);
        }

        List<TemplateError>? errors = null;

        string? rendered = null;

        using (Telemetry.Activities.StartActivity("RenderLiquid"))
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
                    rendered = fluidTemplate.Render(this);
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
