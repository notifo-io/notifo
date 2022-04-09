// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using NodaTime;
using Notifo.Domain.Integrations;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Domain.Channels.Sms
{
    public sealed class SmsJob : IUserNotification, IIntegrationTarget, IChannelJob
    {
        public Guid Id { get; init; }

        public string PhoneNumber { get; init; }

        public string AppId { get; init; }

        public string EventId { get; init; }

        public string UserId { get; init; }

        public string Topic { get; init; }

        public string PhoneText { get; init; }

        public string TemplateLanguage { get; init; }

        public string? TemplateName { get; init; }

        public NotificationProperties? Properties { get; init; }

        public ActivityContext UserEventActivity { get; init; }

        public ActivityContext EventActivity { get; init; }

        public ActivityContext NotificationActivity { get; init; }

        public bool Test { get; init; }

        public ChannelCondition Condition { get; init; }

        public Duration Delay { get; init; }

        Guid IChannelJob.NotificationId
        {
            get => Id;
        }

        public string Configuration
        {
            get => PhoneNumber;
        }

        public string ScheduleKey
        {
            get => $"{Id}_{PhoneNumber}";
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

        public SmsJob(UserNotification notification, ChannelSetting setting, string phoneNumber)
        {
            SimpleMapper.Map(notification, this);

            Condition = setting.Condition;
            Delay = Duration.FromSeconds(setting.DelayInSeconds ?? 0);
            PhoneNumber = phoneNumber;
            PhoneText = notification.Formatting.Subject.Truncate(140);
            TemplateLanguage = notification.UserLanguage;
            TemplateName = setting.Template;
        }

        public static string ComputeScheduleKey(Guid id)
        {
            return id.ToString();
        }
    }
}
