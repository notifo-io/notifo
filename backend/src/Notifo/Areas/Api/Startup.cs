// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Builder;
using Notifo.Areas.Api.Controllers.Notifications;
using Notifo.Pipeline;

namespace Notifo.Areas.Api
{
    public static class Startup
    {
        public static void ConfigureApi(this IApplicationBuilder app, SignalROptions signalROptions)
        {
            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseOpenApi(settings =>
            {
                settings.Path = "/api/openapi.json";
            });

            app.UseReDoc(settings =>
            {
                settings.Path = "/api/docs";

                settings.DocumentPath = "/api/openapi.json";
            });

            app.UseWhen(x => x.Request.Path.StartsWithSegments("/api"), builder =>
            {
                builder.UseMiddleware<RequestExceptionMiddleware>();
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                if (signalROptions.Enabled)
                {
                    endpoints.MapHub<NotificationHub>("/hub");
                }

                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}
