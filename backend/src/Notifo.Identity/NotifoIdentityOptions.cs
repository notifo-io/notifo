// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Identity
{
    public sealed class NotifoIdentityOptions
    {
        public bool AllowPasswordAuth { get; set; }

        public string GithubClient { get; set; }

        public string GithubSecret { get; set; }

        public string GoogleClient { get; set; }

        public string GoogleSecret { get; set; }

        public NotifoIdentityUser[] Users { get; set; }

        public bool IsGithubAuthConfigured()
        {
            return !string.IsNullOrWhiteSpace(GithubClient) && !string.IsNullOrWhiteSpace(GithubSecret);
        }

        public bool IsGoogleAuthConfigured()
        {
            return !string.IsNullOrWhiteSpace(GoogleClient) && !string.IsNullOrWhiteSpace(GoogleSecret);
        }
    }
}
