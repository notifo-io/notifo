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
            builder.AddSemanticLog();

            builder.Services.AddServices(config);
        }

        private static void AddServices(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<SemanticLogOptions>(config, "logging");

            if (config.GetValue<bool>("logging:human"))
            {
                services.AddSingletonAs(_ => JsonLogWriterFactory.Readable())
                    .As<IObjectWriterFactory>();
            }
            else
            {
                services.AddSingletonAs(_ => JsonLogWriterFactory.Default())
                    .As<IObjectWriterFactory>();
            }

            var loggingFile = config.GetValue<string>("logging:file");

            if (!string.IsNullOrWhiteSpace(loggingFile))
            {
                services.AddSingletonAs(_ => new FileChannel(loggingFile))
                    .As<ILogChannel>();
            }

            var useColors = config.GetValue<bool>("logging:colors");

            services.AddSingletonAs(_ => new ConsoleLogChannel(useColors))
                .As<ILogChannel>();

            services.AddSingletonAs<StackdriverSeverityLogAppender>()
                .As<ILogAppender>();

            services.AddSingletonAs<TimestampLogAppender>()
                .As<ILogAppender>();

            services.AddSingletonAs<DebugLogChannel>()
                .As<ILogChannel>();

            services.AddSingletonAs<SemanticLog>()
                .As<ISemanticLog>();
        }
    }
}
