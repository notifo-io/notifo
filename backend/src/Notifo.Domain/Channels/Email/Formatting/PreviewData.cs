// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.Apps;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Users;

namespace Notifo.Domain.Channels.Email.Formatting;

public static class PreviewData
{
    public static readonly User User;

    public static readonly App App;

    public static readonly IReadOnlyList<EmailJob> Jobs;

    static PreviewData()
    {
        App = new App("1", SystemClock.Instance.GetCurrentInstant())
        {
            Name = "Sample App"
        };

        User = new User(App.Id, "1", SystemClock.Instance.GetCurrentInstant())
        {
            EmailAddress = "john.doe@internet.com",
            PreferredLanguage = "en",
            PreferredTimezone = "UTC",
            FullName = "John Does",
        };

        var context = new ChannelContext
        {
            App = App,
            AppId = App.Id,
            Configuration = new SendConfiguration(),
            ConfigurationId = default,
            IsUpdate = false,
            Setting = new ChannelSetting(),
            User = User,
            UserId = User.Id,
        };

        Jobs = new[]
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
        }.Select(x => new EmailJob(x, context, User.EmailAddress)).ToList();
    }
}
