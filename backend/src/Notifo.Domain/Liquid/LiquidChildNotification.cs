// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Utils;

namespace Notifo.Domain.Liquid;

public sealed class LiquidChildNotification(
    SimpleNotification childNotification,
    string imagePresetSmall,
    string imagePresetLarge,
    IImageFormatter imageFormatter)
    :  LiquidNotificationBase(childNotification.Formatting, childNotification.Properties, imagePresetSmall, imagePresetLarge, imageFormatter)
{
}
