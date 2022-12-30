// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using Notifo.Domain.Integrations;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Domain.Channels.Sms;

public sealed class SmsJob : ChannelJob, IIntegrationTarget
{
    public string PhoneNumber { get; init; }

    public string Text { get; init; }

    public string TemplateLanguage { get; init; }

    public string? TemplateName { get; init; }

    public NotificationProperties? Properties { get; init; }

    public ActivityContext EventActivity { get; init; }

    public ActivityContext UserEventActivity { get; init; }

    public ActivityContext UserNotificationActivity { get; init; }

    public bool Test { get; init; }

    public string ScheduleKey
    {
        get => ComputeScheduleKey(Tracking.UserNotificationId, PhoneNumber);
    }

    IEnumerable<KeyValuePair<string, object>>? IIntegrationTarget.Properties
    {
        get
        {
            if (Properties != null)
            {
                yield return new KeyValuePair<string, object>("properties", Properties);
            }

            yield return new KeyValuePair<string, object>("phoneNumber", PhoneNumber);
        }
    }

    public SmsJob()
    {
    }

    public SmsJob(UserNotification notification, ChannelSetting setting, Guid configurationId, string phoneNumber)
        : base(notification, setting, configurationId, false, Providers.Sms)
    {
        SimpleMapper.Map(notification, this);

        PhoneNumber = phoneNumber;
        TemplateLanguage = notification.UserLanguage;
        TemplateName = setting.Template;
        Text = notification.Formatting.Subject;
    }

    public static string ComputeScheduleKey(Guid notificationId, string phoneNumber)
    {
        return $"{notificationId}_{phoneNumber}";
    }
}
