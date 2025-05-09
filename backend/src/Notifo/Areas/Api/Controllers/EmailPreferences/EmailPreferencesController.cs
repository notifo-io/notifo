﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Notifo.Domain;
using Notifo.Domain.Identity;
using Notifo.Domain.Integrations;
using Notifo.Domain.Subscriptions;
using Notifo.Domain.Topics;
using Notifo.Domain.Users;
using Notifo.Infrastructure;
using Notifo.Pipeline;

namespace Notifo.Areas.Api.Controllers.EmailPreferences;

[ApiExplorerSettings(IgnoreApi = true)]
public sealed class EmailPreferencesController(ISubscriptionStore subscriptionStore, ITopicStore topicStore, IUserStore userStore) : BaseController
{
    private static readonly TopicQuery TopicQuery = new TopicQuery { Scope = TopicQueryScope.Explicit };

    [HttpGet("api/email-preferences")]
    [AppPermission(NotifoRoles.AppUser)]
    public async Task<IActionResult> EmailPreferences()
    {
        var vm = new EmailPreferencesVM
        {
            AppName = App.Name
        };

        var user = await userStore.GetAsync(App.Id, UserId, HttpContext.RequestAborted);

        if (user == null || user.Settings.GetOrDefault(Providers.Email)?.Send == ChannelSend.NotSending)
        {
            return View("EmailPreferencesSaved", vm);
        }

        vm.Topics = await GetEmailTopicsAsync(HttpContext.RequestAborted);

        // There is nothing to configure.
        if (vm.Topics.Count == 0)
        {
            return View("EmailPreferencesSaved", vm);
        }

        return View(vm);
    }

    [HttpPost("api/email-preferences")]
    [AppPermission(NotifoRoles.AppUser)]
    public async Task<IActionResult> Unsubscribe([FromForm] EmailPreferencesModel request)
    {
        if (!request.All)
        {
            await UnsubscribeUserAsync(HttpContext.RequestAborted);
        }

        var topics = await GetEmailTopicsAsync(HttpContext.RequestAborted);

        foreach (var topic in topics.Where(x => request.Topics?.GetOrDefault(x.Key.Path) != true))
        {
            await UnsubscribeAsync(topic.Key.Path, HttpContext.RequestAborted);
        }

        var vm = new EmailPreferencesVM
        {
            AppName = App.Name
        };

        return View("EmailPreferencesSaved", vm);
    }

    private async Task<Dictionary<Topic, bool>> GetEmailTopicsAsync(
        CancellationToken ct)
    {
        var result = new Dictionary<Topic, bool>();

        var topics = await topicStore.QueryAsync(App.Id, TopicQuery, ct);

        // Only handled topics where emails are explicitly allowed.
        var topicsWithEmail = topics.Where(x => x.Channels.GetOrDefault(Providers.Email) == TopicChannel.Allowed);

        if (!topicsWithEmail.Any())
        {
            return result;
        }

        var subscriptions = await subscriptionStore.QueryAsync(App.Id, new SubscriptionQuery { UserId = UserId }, ct);

        foreach (var topic in topicsWithEmail)
        {
            var subscription = subscriptions.FirstOrDefault(x => x.TopicPrefix == topic.Path);

            if (subscription?.TopicSettings?.GetOrDefault(Providers.Email)?.Send == ChannelSend.Send)
            {
                result[topic] = true;
            }
        }

        return result;
    }

    private async Task UnsubscribeUserAsync(
        CancellationToken ct)
    {
        try
        {
            var command = new DisableUserChannel { UserId = UserId, Channel = Providers.Email };

            await Mediator.SendAsync(command, ct);
        }
        catch (DomainObjectNotFoundException)
        {
            return;
        }
    }

    private async Task UnsubscribeAsync(string path,
        CancellationToken ct)
    {
        try
        {
            var command = new DisableSubscriptionChannel { UserId = UserId, Topic = path, Channel = Providers.Email };

            await Mediator.SendAsync(command, ct);
        }
        catch (DomainObjectNotFoundException)
        {
            return;
        }
    }
}
