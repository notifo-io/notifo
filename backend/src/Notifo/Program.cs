// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;

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
