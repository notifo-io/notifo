// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Notifo.Domain.Channels.MobilePush;
using Notifo.Domain.UserNotifications;

namespace Notifo.Domain.Integrations.Firebase
{
    public sealed class FirebaseMobilePushSender : IMobilePushSender
    {
        private readonly FirebaseMessagingWrapper wrapper;
        private readonly bool sendSilentAndroid;
        private readonly bool sendSilentIOS;

        public FirebaseMobilePushSender(FirebaseMessagingWrapper wrapper,
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

                await wrapper.Messaging.SendAsync(message, ct);
            }
            catch (FirebaseMessagingException ex) when (ex.ErrorCode == ErrorCode.InvalidArgument)
            {
                throw new MobilePushTokenExpiredException();
            }
            catch (FirebaseMessagingException ex) when (ex.ErrorCode == ErrorCode.NotFound)
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
