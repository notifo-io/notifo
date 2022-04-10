// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure;

namespace Notifo.Domain.Channels.Email
{
    public sealed class EmailJob : IChannelJob
    {
        public static readonly List<EmailJob> ForPreview = new[]
        {
            new UserNotification
            {
                Formatting = new NotificationFormatting<string>
                {
                    Subject = "A notification"
                }
            },
            new UserNotification
            {
                Formatting = new NotificationFormatting<string>
                {
                    Subject = "A notification with body",
                    Body = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr",
                    ImageLarge = string.Empty,
                    ImageSmall = string.Empty
                }
            },
            new UserNotification
            {
                Formatting = new NotificationFormatting<string>
                {
                    Subject = "A notification with body and image",
                    Body = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr",
                    ImageLarge = string.Empty,
                    ImageSmall = "https://via.placeholder.com/150"
                }
            },
            new UserNotification
            {
                Formatting = new NotificationFormatting<string>
                {
                    Subject = "A notification with body and image and button",
                    Body = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr",
                    ImageLarge = string.Empty,
                    ImageSmall = "https://via.placeholder.com/150",
                    ConfirmText = "Confirm",
                    ConfirmMode = ConfirmMode.Explicit
                },
                ConfirmUrl = "/url/to/confirm"
            },
            new UserNotification
            {
                Formatting = new NotificationFormatting<string>
                {
                    Subject = "A notification with body and image and link",
                    Body = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr",
                    ImageLarge = string.Empty,
                    ImageSmall = "https://via.placeholder.com/150",
                    LinkText = "Follow Link",
                    LinkUrl = "/url/to/link"
                }
            }
        }.Select(x => new EmailJob(x, new ChannelSetting(), "john@internet.com")).ToList();

        public BaseUserNotification Notification { get; init; }

        public string EmailAddress { get; init; }

        public string? EmailTemplate { get; init; }

        public string? FromEmail { get; init; }

        public string? FromName { get; init; }

        public ChannelCondition Condition { get; init; }

        public Duration Delay { get; init; }

        Guid IChannelJob.NotificationId
        {
            get => Notification.Id;
        }

        public string Configuration
        {
            get => EmailAddress;
        }

        public string ScheduleKey
        {
            get => string.Join("_",
                Notification.AppId,
                Notification.UserId,
                Notification.UserLanguage,
                Notification.Test,
                EmailAddress,
                EmailTemplate,
                FromEmail,
                FromName);
        }

        public EmailJob()
        {
        }

        public EmailJob(BaseUserNotification notification, ChannelSetting setting, string emailAddress)
        {
            Delay = Duration.FromSeconds(setting.DelayInSeconds ?? 0);
            EmailAddress = emailAddress;
            EmailTemplate = setting.Template;
            Condition = setting.Condition;
            FromEmail = setting.Properties?.GetOrDefault(nameof(FromEmail));
            FromName = setting.Properties?.GetOrDefault(nameof(FromName));
            Notification = notification;
        }
    }
}
