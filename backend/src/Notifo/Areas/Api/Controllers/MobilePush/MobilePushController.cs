// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Notifo.Areas.Api.Controllers.MobilePush.Dto;
using Notifo.Domain;
using Notifo.Domain.Users;
using Notifo.Pipeline;
using NSwag.Annotations;

namespace Notifo.Areas.Api.Controllers.MobilePush
{
    [OpenApiIgnore]
    public sealed class MobilePushController : BaseController
    {
        private readonly IUserStore userStore;

        public MobilePushController(IUserStore userStore)
        {
            this.userStore = userStore;
        }

        [HttpPost("api/mobilepush")]
        [AppPermission(Roles.User)]
        public async Task<IActionResult> PostToken([FromBody] MobilePushRequestDto request)
        {
            var command = new AddUserMobileToken
            {
                Token = request.Token
            };

            await userStore.UpsertAsync(App.Id, UserId, command, HttpContext.RequestAborted);

            return NoContent();
        }

        [HttpDelete("api/mobilepush")]
        [AppPermission(Roles.User)]
        public async Task<IActionResult> DeleteToken([FromBody] MobilePushRequestDto request)
        {
            var command = new RemoveUserMobileToken
            {
                Token = request.Token
            };

            await userStore.UpsertAsync(App.Id, UserId, command, HttpContext.RequestAborted);

            return NoContent();
        }
    }
}
