// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FirebaseAdmin.Messaging;

namespace Notifo.Domain.Integrations.Firebase;

public sealed partial class FirebaseIntegration : IMobilePushSender
{
    private const int Attempts = 5;

    public async Task<DeliveryResult> SendAsync(IntegrationContext context, MobilePushMessage message,
        CancellationToken ct)
    {
        if (!ShouldSend(context, message.Silent, message.DeviceType))
        {
            return DeliveryResult.Skipped();
        }

        try
        {
            var firebaseProject = ProjectIdProperty.GetString(context.Properties);
            var firebaseCredentials = CredentialsProperty.GetString(context.Properties);
            var firebaseMessage = message.ToFirebaseMessage(DateTimeOffset.UtcNow);

            // Try a few attempts to get a non-disposed messaging service.
            for (var i = 1; i <= Attempts; i++)
            {
                try
                {
                    var wrapper = messagingPool.GetMessaging(firebaseProject, firebaseCredentials);

                    await wrapper.Messaging.SendAsync(firebaseMessage, ct);
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

        return DeliveryResult.Handled;
    }

    private static bool ShouldSend(IntegrationContext context, bool isSilent, MobileDeviceType deviceType)
    {
        if (!isSilent)
        {
            return true;
        }

        var sendSilentIOS = SilentISOProperty.GetBoolean(context.Properties);
        var sendSilentAndroid = SilentAndroidProperty.GetBoolean(context.Properties);

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
