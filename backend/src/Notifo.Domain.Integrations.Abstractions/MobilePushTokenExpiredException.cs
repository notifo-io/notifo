// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.Serialization;

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

    protected MobilePushTokenExpiredException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
