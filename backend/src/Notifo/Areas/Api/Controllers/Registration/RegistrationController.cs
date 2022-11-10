// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Notifo.Areas.Api.Controllers.Registration.Dto;
using Notifo.Areas.Api.Controllers.Registration.Dtos;
using Notifo.Domain;
using Notifo.Domain.Channels;
using Notifo.Domain.Channels.WebPush;
using Notifo.Domain.Identity;
using Notifo.Domain.Subscriptions;
using Notifo.Domain.Users;
using Notifo.Infrastructure;
using Notifo.Pipeline;

namespace Notifo.Areas.Api.Controllers.Registration;

[ApiExplorerSettings(IgnoreApi = true)]
public sealed class RegistrationController : BaseController
{
    private readonly IUserStore userStore;
    private readonly ISubscriptionStore subscriptionStore;
    private readonly IWebPushService webPushService;

    public RegistrationController(IUserStore userStore, ISubscriptionStore subscriptionStore, IWebPushService webPushService)
    {
        this.userStore = userStore;
        this.subscriptionStore = subscriptionStore;
        this.webPushService = webPushService;
    }

    [HttpPost("api/web/register")]
    [AppPermission(NotifoRoles.AppWebManager, NotifoRoles.AppUser)]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto request)
    {
        string? userId = null;
        string? userToken = null;

        if (request.CreateUser)
        {
            userId = Guid.NewGuid().ToString();

            var update = request.ToUpsert();

            var user = await userStore.UpsertAsync(App.Id, userId, update, HttpContext.RequestAborted);

            if (request.Topics?.Any() == true)
            {
                var command = new Subscribe
                {
                    TopicSettings = new ChannelSettings
                    {
                        [Providers.WebPush] = new ChannelSetting
                        {
                            Send = ChannelSend.Send
                        }
                    }
                };

                if (!string.IsNullOrEmpty(request.EmailAddress))
                {
                    command.TopicSettings[Providers.Email] = new ChannelSetting
                    {
                        Send = ChannelSend.Send
                    };
                }

                foreach (var topic in request.Topics.OrEmpty())
                {
                    await subscriptionStore.UpsertAsync(App.Id, userId, topic, command, HttpContext.RequestAborted);
                }
            }

            userToken = user.ApiKey;
        }

        var response = new RegisteredUserDto
        {
            PublicKey = webPushService.PublicKey,
            UserId = userId,
            UserToken = userToken
        };

        return Ok(response);
    }
}
