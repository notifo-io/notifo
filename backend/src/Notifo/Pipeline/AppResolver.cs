// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Domain;
using Notifo.Domain.Apps;
using Notifo.Infrastructure.Security;

namespace Notifo.Pipeline
{
    public sealed class AppResolver : IAsyncActionFilter
    {
        private sealed class AppFeature : IAppFeature
        {
            public App App { get; init; }
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var appPermission = context.ActionDescriptor.EndpointMetadata.OfType<AppPermissionAttribute>().FirstOrDefault();

            if (appPermission != null)
            {
                var appId = context.RouteData.Values["appId"]?.ToString();

                if (string.IsNullOrWhiteSpace(appId))
                {
                    appId = context.HttpContext.User.AppId();
                }

                if (!string.IsNullOrWhiteSpace(appId))
                {
                    var appStore = context.HttpContext.RequestServices.GetRequiredService<IAppStore>();

                    var app = await appStore.GetAsync(appId);

                    if (app == null)
                    {
                        context.Result = new NotFoundResult();
                        return;
                    }

                    var currentRoles = GetRoles(app, context.HttpContext.User);

                    if (currentRoles.Count == 0 || !appPermission.RequiredAppRoles.Any(x => currentRoles.Contains(x)))
                    {
                        context.Result = new ForbidResult();
                        return;
                    }

                    context.HttpContext.Features.Set<IAppFeature>(new AppFeature { App = app });
                }
            }

            await next();
        }

        private static HashSet<string> GetRoles(App app, ClaimsPrincipal user)
        {
            var roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var subject = user.Sub();

            if (subject != null)
            {
                if (app.Contributors.TryGetValue(subject, out var role) && !string.IsNullOrWhiteSpace(role))
                {
                    roles.Add(role);
                }
                else if (string.Equals(app.Id, user.AppId(), StringComparison.OrdinalIgnoreCase))
                {
                    roles.Add(Roles.User);
                }
            }

            foreach (var claim in user.Claims)
            {
                if (claim.Type == DefaultClaimTypes.AppRole)
                {
                    roles.Add(claim.Value);
                }
            }

            if (roles.Contains(Roles.Owner))
            {
                roles.Add(Roles.Admin);
            }

            if (roles.Contains(Roles.Admin))
            {
                roles.Add(Roles.User);
                roles.Add(Roles.WebManager);
            }

            return roles;
        }
    }
}
