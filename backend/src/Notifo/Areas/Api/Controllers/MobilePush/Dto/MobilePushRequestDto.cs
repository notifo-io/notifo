// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Notifo.Areas.Api.Controllers.MobilePush.Dto
{
    public sealed class MobilePushRequestDto
    {
        [Required]
        public string Token { get; set; }
    }
}
