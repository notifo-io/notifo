// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations;

[Serializable]
public class MobilePushTokenExpiredException : Exception
{
    public MobilePushTokenExpiredException()
    {
    }

    public MobilePushTokenExpiredException(string message)
        : base(message)
    {
    }

    public MobilePushTokenExpiredException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
