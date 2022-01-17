// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Identity
{
    public sealed class UserValues
    {
        public string? Password { get; set; }

        public string Email { get; set; }

        public string DisplayName { get; set; }

        public bool? Consent { get; set; }

        public bool? ConsentForEmails { get; set; }

        public bool? Invited { get; set; }

        public HashSet<string>? Roles { get; set; }
    }
}
