// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations;

public interface ISmsCallback
{
    Task HandleCallbackAsync(ISmsSender source, string trackingToken, DeliveryResult result, string? details = null);
}
