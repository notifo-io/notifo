// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FirebaseAdmin.Messaging;

namespace Notifo.Domain.Integrations.Firebase;

public sealed class FirebaseMobilePushSender : IMobilePushSender
{
    private const int Attempts = 5;
    private readonly Func<FirebaseMessagingWrapper> wrapper;
    private readonly bool sendSilentAndroid;
    private readonly bool sendSilentIOS;

    public string Name => "Firebase";

    public FirebaseMobilePushSender(Func<FirebaseMessagingWrapper> wrapper,
        bool sendSilentIOS,
        bool sendSilentAndroid)
    {
        this.wrapper = wrapper;
        this.sendSilentAndroid = sendSilentAndroid;
        this.sendSilentIOS = sendSilentIOS;
    }

    public async Task SendAsync(MobilePushMessage message,
        CancellationToken ct)
    {
        if (!ShouldSend(message.Silent, message.DeviceType))
        {
            return;
        }

        try
        {
            var firebaseMessage = message.ToFirebaseMessage(DateTimeOffset.UtcNow);

            // Try a few attempts to get a non-disposed messaging service.
            for (var i = 1; i <= Attempts; i++)
            {
                try
                {
                    await wrapper().Messaging.SendAsync(firebaseMessage, ct);
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
