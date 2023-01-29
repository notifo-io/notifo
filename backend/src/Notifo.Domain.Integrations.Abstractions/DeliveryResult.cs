// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

public record struct DeliveryResult(DeliveryStatus Status, string? Detail = null)
{
    public static readonly DeliveryResult Attempt = new DeliveryResult(DeliveryStatus.Attempt);

    public static readonly DeliveryResult Handled = new DeliveryResult(DeliveryStatus.Handled);

    public static readonly DeliveryResult Sent = new DeliveryResult(DeliveryStatus.Sent);

    public static DeliveryResult Skipped(string? detail = null)
    {
        return new DeliveryResult(DeliveryStatus.Skipped, detail);
    }

    public static DeliveryResult Failed(string? detail = null)
    {
        return new DeliveryResult(DeliveryStatus.Failed, detail);
    }
}
