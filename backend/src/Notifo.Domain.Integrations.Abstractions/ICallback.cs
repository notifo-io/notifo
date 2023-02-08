// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations;

public interface ICallback<T> where T : IIntegration
{
    Task UpdateStatusAsync(T source, string trackingToken, DeliveryResult result);
}
