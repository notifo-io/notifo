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

namespace Notifo.Domain.Channels.Sms
{
    public sealed class SmsJob : IUserNotification, IIntegrationTarget
    {
        public Guid Id { get; set; }

        public string PhoneNumber { get; set; }

        public string AppId { get; set; }

        public string EventId { get; set; }

        public string UserId { get; set; }

        public string Topic { get; set; }

        public string Text { get; set; }

        public string TemplateLanguage { get; set; }

        public string? TemplateName { get; set; }

        public NotificationProperties? Properties { get; set; }

        public ActivityContext UserEventActivity { get; set; }

        public ActivityContext EventActivity { get; set; }

        public ActivityContext NotificationActivity { get; set; }

        public bool IsImmediate { get; set; }

        public bool Test { get; set; }

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

        public SmsJob(UserNotification notification, string? templateName, string phoneNumber)
        {
            PhoneNumber = phoneNumber;

            SimpleMapper.Map(notification, this);

            Text = notification.Formatting.Subject;

            if (Text.Length > 140)
            {
                Text = Text[..137] + "...";
            }

            TemplateName = templateName;
            TemplateLanguage = notification.UserLanguage;
        }

        public static string ComputeScheduleKey(Guid id)
        {
            return id.ToString();
        }
    }
}
