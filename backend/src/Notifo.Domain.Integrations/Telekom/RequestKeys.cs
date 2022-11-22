// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations.Telekom;

public static class RequestKeys
{
    public static readonly string From = nameof(From);

    public static readonly string To = nameof(To);

    public static readonly string Body = nameof(Body);

    public static readonly string StatusCallback = nameof(StatusCallback);

    public static readonly string MessageStatus = nameof(MessageStatus);

    public static readonly string ReferenceValue = "reference";

    public static readonly string ReferenceNumber = "reference_number";
}
