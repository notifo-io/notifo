// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using FakeItEasy;
using FluentAssertions;
using NodaTime;
using Notifo.Domain.Apps;
using Notifo.Domain.Channels;
using Notifo.Domain.Log;
using Notifo.Domain.UserEvents;
using Notifo.Domain.Users;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Texts;
using Xunit;

namespace Notifo.Domain.UserNotifications
{
    public sealed class UserNotificationFactoryTests
    {
        private readonly IClock clock = A.Fake<IClock>();
        private readonly ILogStore logStore = A.Fake<ILogStore>();
        private readonly App app;
        private readonly User user = new User();
        private readonly Instant now = SystemClock.Instance.GetCurrentInstant();
        private readonly IUserNotificationUrl notificationUrl = A.Fake<IUserNotificationUrl>();
        private readonly IUserNotificationFactory sut;

        public UserNotificationFactoryTests()
        {
            app = new App
            {
                Languages = new[]
                {
                    "en",
                    "de"
                }
            };

            A.CallTo(() => clock.GetCurrentInstant())
                .Returns(now);

            A.CallTo(() => notificationUrl.TrackConfirmed(A<Guid>._, A<string>._))
                .ReturnsLazily(new Func<Guid, string, string>((id, lang) => $"confirm/{id}/?lang={lang}"));

            A.CallTo(() => notificationUrl.TrackSeen(A<Guid>._, A<string>._))
                .ReturnsLazily(new Func<Guid, string, string>((id, lang) => $"seen/{id}/?lang={lang}"));

            A.CallTo(() => notificationUrl.TrackDelivered(A<Guid>._, A<string>._))
                .ReturnsLazily(new Func<Guid, string, string>((id, lang) => $"delivered/{id}/?lang={lang}"));

            sut = new UserNotificationFactory(clock, logStore, notificationUrl);
        }

        [Fact]
        public void Should_not_create_notification_when_app_id_set()
        {
            var userEvent = CreateMinimumUserEvent();

            userEvent.AppId = null!;

            Assert.Null(sut.Create(app, user, userEvent));
        }

        [Fact]
        public void Should_not_create_notification_when_user_id_not_set()
        {
            var userEvent = CreateMinimumUserEvent();

            userEvent.UserId = null!;

            Assert.Null(sut.Create(app, user, userEvent));
        }

        [Fact]
        public void Should_not_create_notification_when_topic_not_set()
        {
            var userEvent = CreateMinimumUserEvent();

            userEvent.Topic = null!;

            Assert.Null(sut.Create(app, user, userEvent));
        }

        [Fact]
        public void Should_not_create_notification_when_event_id_not_set()
        {
            var userEvent = CreateMinimumUserEvent();

            userEvent.EventId = null!;

            Assert.Null(sut.Create(app, user, userEvent));
        }

        [Fact]
        public void Should_not_create_notification_when_formatting_not_set()
        {
            var userEvent = CreateMinimumUserEvent();

            userEvent.Formatting = null!;

            Assert.Null(sut.Create(app, user, userEvent));
        }

        [Fact]
        public void Should_not_create_notification_when_subject_not_set()
        {
            var userEvent = CreateMinimumUserEvent();

            userEvent.Formatting.Subject = null!;

            Assert.Null(sut.Create(app, user, userEvent));
        }

        [Fact]
        public void Should_not_create_notification_when_subject_text_not_set()
        {
            var userEvent = CreateMinimumUserEvent();

            userEvent.Formatting.Subject.Clear();

            Assert.Null(sut.Create(app, user, userEvent));
        }

        [Fact]
        public void Should_map_base_properties()
        {
            var userEvent = CreateMinimumUserEvent();

            var notification = sut.Create(app, user, userEvent)!;

            Assert.Equal(userEvent.AppId, notification.AppId);
            Assert.Equal(userEvent.Created, notification.Created);
            Assert.Equal(userEvent.EventId, notification.EventId);
            Assert.Equal(userEvent.UserId, notification.UserId);
            Assert.Equal(userEvent.Topic, notification.Topic);

            Assert.Equal(now, notification.Updated);
        }

        [Fact]
        public void Should_use_language_from_user_if_supported()
        {
            var userEvent = CreateMinimumUserEvent();

            user.PreferredLanguage = "de";

            var notification = sut.Create(app, user, userEvent)!;

            Assert.Equal("deText", notification.Formatting.Subject);
            Assert.Equal("de", notification.UserLanguage);

            A.CallTo(() => logStore.LogAsync(app.Id, A<string>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public void Should_use_language_from_app_if_user_language_not_supported()
        {
            var userEvent = CreateMinimumUserEvent();

            user.PreferredLanguage = "dk";

            var notification = sut.Create(app, user, userEvent)!;

            Assert.Equal("enText", notification.Formatting.Subject);
            Assert.Equal("en", notification.UserLanguage);

            A.CallTo(() => logStore.LogAsync(app.Id, A<string>._))
                .MustHaveHappened();
        }

        [Fact]
        public void Should_create_confirm_url_with_default_text_when_explicit_mode()
        {
            var userEvent = CreateMinimumUserEvent();

            userEvent.Formatting.ConfirmMode = ConfirmMode.Explicit;

            var notification = sut.Create(app, user, userEvent)!;

            Assert.Equal($"confirm/{notification.Id}/?lang=en", notification.ConfirmUrl);
            Assert.Equal($"Confirm", notification.Formatting.ConfirmText);
        }

        [Fact]
        public void Should_create_confirm_url_with_custom_text_when_explicit_mode()
        {
            var userEvent = CreateMinimumUserEvent();

            userEvent.Formatting.ConfirmMode = ConfirmMode.Explicit;
            userEvent.Formatting.ConfirmText = new LocalizedText
            {
                ["en"] = "Confirmed"
            };

            var notification = sut.Create(app, user, userEvent)!;

            Assert.Equal($"confirm/{notification.Id}/?lang=en", notification.ConfirmUrl);
            Assert.Equal($"Confirmed", notification.Formatting.ConfirmText);
        }

        [Fact]
        public void Should_not_create_confirm_mode_when_not_explicit()
        {
            var userEvent = CreateMinimumUserEvent();

            var notification = sut.Create(app, user, userEvent)!;

            Assert.Null(notification.Formatting.ConfirmText);
        }

        [Fact]
        public void Should_merge_settings()
        {
            var userEvent = CreateMinimumUserEvent();

            user.Settings.CopyFrom(new NotificationSettings
            {
                [Providers.Email] = new NotificationSetting
                {
                    Send = NotificationSend.Send
                },
                [Providers.MobilePush] = new NotificationSetting
                {
                    DelayInSeconds = 100
                }
            });

            userEvent.SubscriptionSettings = new NotificationSettings
            {
                [Providers.Email] = new NotificationSetting
                {
                    Send = NotificationSend.Send
                }
            };

            userEvent.EventSettings = new NotificationSettings
            {
                [Providers.Email] = new NotificationSetting
                {
                    Send = NotificationSend.NotSending
                }
            };

            var notification = sut.Create(app, user, userEvent)!;

            notification.Channels.Should().BeEquivalentTo(
                new Dictionary<string, UserNotificationChannel>
                {
                    [Providers.Email] = new UserNotificationChannel
                    {
                        Setting = new NotificationSetting
                        {
                            Send = NotificationSend.NotSending
                        },
                        Status = new Dictionary<string, ChannelSendInfo>()
                    },

                    [Providers.MobilePush] = new UserNotificationChannel
                    {
                        Setting = new NotificationSetting
                        {
                            DelayInSeconds = 100
                        },
                        Status = new Dictionary<string, ChannelSendInfo>()
                    }
                });
        }

        private UserEventMessage CreateMinimumUserEvent()
        {
            var userEvent = new UserEventMessage
            {
                Formatting = new NotificationFormatting<LocalizedText>
                {
                    Subject = new LocalizedText
                    {
                        ["en"] = "enText",
                        ["de"] = "deText"
                    }
                }
            };

            userEvent.AppId = "app";
            userEvent.Created = now;
            userEvent.EventId = "eventId";
            userEvent.Topic = "topic";
            userEvent.UserId = "user";

            return userEvent;
        }
    }
}
