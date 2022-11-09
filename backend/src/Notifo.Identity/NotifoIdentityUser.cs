// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Identity;

public sealed class NotifoIdentityUser
{
    public string Email { get; set; }

    public string Password { get; set; }

    public string? Role { get; set; }

    public bool PasswordReset { get; set; }

    public bool IsConfigured()
    {
        return !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
    }
}
