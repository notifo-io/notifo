// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Notifo.Domain.Channels.MobilePush;
using Notifo.Domain.UserNotifications;

namespace Notifo.Domain.Integrations.Firebase
{
    public sealed class FirebaseMobilePushSender : IMobilePushSender
    {
        private const int Attempts = 5;
        private readonly Func<FirebaseMessagingWrapper> wrapper;
        private readonly bool sendSilentAndroid;
        private readonly bool sendSilentIOS;

        public FirebaseMobilePushSender(Func<FirebaseMessagingWrapper> wrapper,
            bool sendSilentIOS,
            bool sendSilentAndroid)
        {
            this.wrapper = wrapper;
            this.sendSilentAndroid = sendSilentAndroid;
            this.sendSilentIOS = sendSilentIOS;
        }

        public async Task SendAsync(UserNotification userNotification, MobilePushOptions options,
            CancellationToken ct)
        {
            if (!ShouldSend(userNotification.Silent, options.DeviceType))
            {
                return;
            }

            try
            {
                var message = userNotification.ToFirebaseMessage(options.DeviceToken, options.Wakeup);

                // Try a few attempts to get a non-disposed messaging service.
                for (var i = 1; i <= Attempts; i++)
                {
                    try
                    {
                        await wrapper().Messaging.SendAsync(message, ct);
                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        if (i == Attempts)
                        {
                            throw;
                        }
                    }
                }
            }
            catch (FirebaseMessagingException ex) when (ex.MessagingErrorCode == MessagingErrorCode.Unregistered)
            {
                throw new MobilePushTokenExpiredException();
            }
        }

        private bool ShouldSend(bool isSilent, MobileDeviceType deviceType)
        {
            if (!isSilent)
            {
                return true;
            }

            switch (deviceType)
            {
                case MobileDeviceType.Android:
                    return sendSilentAndroid;
                case MobileDeviceType.iOS:
                    return sendSilentIOS;
                default:
                    return false;
            }
        }
    }
}
