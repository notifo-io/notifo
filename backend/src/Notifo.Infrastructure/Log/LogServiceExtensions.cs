// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Notifo.Infrastructure.Log;
using Squidex.Log;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class LogServiceExtensions
    {
        public static void ConfigureForMe(this ILoggingBuilder builder, IConfiguration config)
        {
            builder.ClearProviders();
            builder.ConfigureSemanticLog(config);

            builder.Services.AddServices();
        }

        private static void AddServices(this IServiceCollection services)
        {
            services.AddSingletonAs<StackdriverSeverityLogAppender>()
                .As<ILogAppender>();
        }
    }
}
