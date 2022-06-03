// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;
using Notifo.Pipeline;

namespace Notifo
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((context, builder) =>
                {
                    builder.ConfigureForMe(context.Configuration);
                })
                .ConfigureServices(services =>
                {
                    // Step 0: Log all configuration.
                    services.AddHostedService<LogConfigurationHost>();
                })
                .ConfigureWebHostDefaults(builder =>
                {
                    builder.ConfigureKestrel((context, serverOptions) =>
                    {
                        if (context.HostingEnvironment.IsDevelopment())
                        {
                            serverOptions.Listen(
                                IPAddress.Any,
                                5003);

                            serverOptions.Listen(
                                IPAddress.Any,
                                5002,
                                listenOptions => listenOptions.UseHttps("../../../dev/squidex-dev.pfx", "password"));
                        }
                    });

                    builder.UseStartup<Startup>();
                });
    }
}
