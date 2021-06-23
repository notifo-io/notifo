// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Notifo.Areas.Api.Controllers.WebPush.Dtos;
using Notifo.Domain.Identity;
using Notifo.Domain.Users;
using Notifo.Pipeline;
using NSwag.Annotations;

namespace Notifo.Areas.Api.Controllers.WebPush
{
    [OpenApiIgnore]
    public sealed class WebPushController : BaseController
    {
        private readonly IUserStore userStore;

        public WebPushController(IUserStore userStore)
        {
            this.userStore = userStore;
        }

        [HttpPost("api/me/webpush")]
        [HttpPost("api/webpush")]
        [AppPermission(NotifoRoles.AppUser)]
        public async Task<IActionResult> PostMyToken([FromBody] RegisterWebTokenDto request)
        {
            var command = new AddUserWebPushSubscription
            {
                Subscription = request.Subscription.ToSubscription()
            };

            await userStore.UpsertAsync(App.Id, UserId, command, HttpContext.RequestAborted);

            return NoContent();
        }

        [HttpDelete("api/me/webpush")]
        [HttpDelete("api/webpush")]
        [AppPermission(NotifoRoles.AppUser)]
        public async Task<IActionResult> DeleteMyToken([FromBody] RegisterWebTokenDto request)
        {
            var command = new RemoveUserWebPushSubscription
            {
                Subscription = request.Subscription.ToSubscription()
            };

            await userStore.UpsertAsync(App.Id, UserId, command, HttpContext.RequestAborted);

            return NoContent();
        }
    }
}
