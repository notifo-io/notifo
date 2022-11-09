﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Areas.Api.Controllers.WebPush.Dtos;

public sealed class RegisterWebTokenDto
{
    public WebPushSubscriptionDto Subscription { get; set; }
}
