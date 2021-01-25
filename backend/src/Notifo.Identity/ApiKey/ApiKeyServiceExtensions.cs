// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Identity.ApiKey;

namespace Microsoft.AspNetCore.Authentication
{
    public static class ApiKeyServiceExtensions
    {
        public static AuthenticationBuilder AddApiKey(this AuthenticationBuilder builder)
        {
            return builder.AddScheme<ApiKeyOptions, ApiKeyHandler>(ApiKeyDefaults.AuthenticationScheme, _ => { });
        }
    }
}
