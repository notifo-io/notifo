// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using NSwag;
using NSwag.Generation.Processors.Security;

namespace Squidex.Areas.Api.Config.OpenApi
{
    public sealed class SecurityProcessor : SecurityDefinitionAppender
    {
        public SecurityProcessor()
            : base("notifo-api", Enumerable.Empty<string>(), CreateOAuthSchema())
        {
        }

        private static OpenApiSecurityScheme CreateOAuthSchema()
        {
            var security = new OpenApiSecurityScheme
            {
                Type = OpenApiSecuritySchemeType.ApiKey
            };

            return security;
        }
    }
}
