// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Identity;

public sealed class NotifoIdentityOptions
{
    public bool AllowPasswordAuth { get; set; }

    public string AdminClientId { get; set; }

    public string AdminClientSecret { get; set; }

    public string GithubClient { get; set; }

    public string GithubSecret { get; set; }

    public string GoogleClient { get; set; }

    public string GoogleSecret { get; set; }

    public string OidcName { get; set; }

    public string OidcClient { get; set; }

    public string OidcSecret { get; set; }

    public string OidcAuthority { get; set; }

    public string OidcMetadataAddress { get; set; }

    public string OidcRoleClaimType { get; set; }

    public string OidcResponseType { get; set; }

    public string OidcOnSignoutRedirectUrl { get; set; }

    public string? OidcPrompt { get; set; }

    public string[] OidcScopes { get; set; }

    public bool OidcGetClaimsFromUserInfoEndpoint { get; set; }

    public bool OidcOverridePermissionsWithCustomClaimsOnLogin { get; set; }

    public bool RequiresHttps { get; set; }

    public NotifoIdentityUser[] Users { get; set; }

    public bool IsOidcConfigured()
    {
        return !string.IsNullOrWhiteSpace(OidcAuthority) && !string.IsNullOrWhiteSpace(OidcClient);
    }

    public bool IsAdminClientConfigured()
    {
        return !string.IsNullOrWhiteSpace(AdminClientId) && !string.IsNullOrWhiteSpace(AdminClientSecret);
    }

    public bool IsGithubAuthConfigured()
    {
        return !string.IsNullOrWhiteSpace(GithubClient) && !string.IsNullOrWhiteSpace(GithubSecret);
    }

    public bool IsGoogleAuthConfigured()
    {
        return !string.IsNullOrWhiteSpace(GoogleClient) && !string.IsNullOrWhiteSpace(GoogleSecret);
    }
}
