// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Builder;
using Notifo.Areas.Api.Controllers.Notifications;

namespace Notifo.Areas.Api
{
    public static class Startup
    {
        public static void ConfigureApi(this IApplicationBuilder app)
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

            app.UseRouting();

            app.UseAuthentication();
            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<NotificationHub>("/hub");

                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}
