// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Areas.Api.Controllers.Registration.Dto
{
    public sealed class RegisteredUserDto
    {
        public string PublicKey { get; set; }

        public string? UserId { get; set; }

        public string? UserToken { get; set; }
    }
}
