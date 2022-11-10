// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Notifo.Areas.Api.Controllers.Users.Dtos;
using Notifo.Domain.Identity;
using Notifo.Domain.Subscriptions;
using Notifo.Domain.Topics;
using Notifo.Domain.Users;
using Notifo.Infrastructure;
using Notifo.Pipeline;

namespace Notifo.Areas.Api.Controllers.Users;

[ApiExplorerSettings(GroupName = "User")]
public class UserController : BaseController
{
    private readonly ISubscriptionStore subscriptionStore;
    private readonly ITopicStore topicStore;
    private readonly IUserStore userStore;

    public UserController(ISubscriptionStore subscriptionStore, ITopicStore topicStore, IUserStore userStore)
    {
        this.subscriptionStore = subscriptionStore;
        this.topicStore = topicStore;
        this.userStore = userStore;
    }

    /// <summary>
    /// Get the current user.
    /// </summary>
    /// <returns>
    /// 200 => User returned.
    /// </returns>
    [HttpGet("api/me")]
    [AppPermission(NotifoRoles.AppUser)]
    [Produces(typeof(ProfileDto))]
    public async Task<IActionResult> GetUser()
    {
        var user = await userStore.GetAsync(App.Id, UserIdOrSub, HttpContext.RequestAborted);

        var response = ProfileDto.FromDomainObject(user!, App);

        return Ok(response);
    }

    /// <summary>
    /// Update the user.
    /// </summary>
    /// <param name="request">The upsert request.</param>
    /// <returns>
    /// 200 => Users upserted.
    /// </returns>
    [HttpPost("api/me")]
    [AppPermission(NotifoRoles.AppUser)]
    [Produces(typeof(ProfileDto))]
    public async Task<IActionResult> PostUser([FromBody] UpdateProfileDto request)
    {
        var update = request.ToUpsert();

        var user = await userStore.UpsertAsync(App.Id, UserIdOrSub, update, HttpContext.RequestAborted);

        var response = ProfileDto.FromDomainObject(user!, App);

        return Ok(response);
    }

    /// <summary>
    /// Query the user topics.
    /// </summary>
    /// <param name="language">The optional language.</param>
    /// <returns>
    /// 200 => User subscriptions returned.
    /// </returns>
    [HttpGet("api/me/topics")]
    [AppPermission(NotifoRoles.AppUser)]
    [Produces(typeof(UserTopicDto[]))]
    public async Task<IActionResult> GetTopics(string? language = null)
    {
        var topics = await topicStore.QueryAsync(App.Id, new TopicQuery { Scope = TopicQueryScope.Explicit }, HttpContext.RequestAborted);

        var response = topics.Select(x => UserTopicDto.FromDomainObject(x, language, App.Language));

        return Ok(response);
    }

    /// <summary>
    /// Query the user subscriptions.
    /// </summary>
    /// <param name="q">The query object.</param>
    /// <returns>
    /// 200 => User subscriptions returned.
    /// </returns>
    [HttpGet("api/me/subscriptions")]
    [AppPermission(NotifoRoles.AppUser)]
    [Produces(typeof(ListResponseDto<SubscriptionDto>))]
    public async Task<IActionResult> GetMySubscriptions([FromQuery] SubscriptionQueryDto q)
    {
        var subscriptions = await subscriptionStore.QueryAsync(App.Id, q.ToQuery(false, UserId), HttpContext.RequestAborted);

        var response = new ListResponseDto<SubscriptionDto>();

        response.Items.AddRange(subscriptions.Select(SubscriptionDto.FromDomainObject));
        response.Total = subscriptions.Total;

        return Ok(response);
    }

    /// <summary>
    /// Gets a user subscription.
    /// </summary>
    /// <param name="topic">The topic path.</param>
    /// <returns>
    /// 200 => Subscription exists.
    /// 404 => Subscription does not exist.
    /// </returns>
    /// <remarks>
    /// User Id and App Id are resolved using the API token.
    /// </remarks>
    [HttpGet("api/me/subscriptions/{*topic}")]
    [AppPermission(NotifoRoles.AppUser)]
    [Produces(typeof(SubscriptionDto))]
    public async Task<IActionResult> GetMySubscription(string topic)
    {
        var subscription = await subscriptionStore.GetAsync(App.Id, UserId, topic, HttpContext.RequestAborted);

        if (subscription == null)
        {
            return NotFound();
        }

        var response = SubscriptionDto.FromDomainObject(subscription);

        return Ok(response);
    }

    /// <summary>
    /// Upserts or deletes my subscriptions.
    /// </summary>
    /// <param name="request">The subscription settings.</param>
    /// <returns>
    /// 204 => User subscribed.
    /// </returns>
    /// <remarks>
    /// User Id and App Id are resolved using the API token.
    /// </remarks>
    [HttpPost("api/me/subscriptions")]
    [AppPermission(NotifoRoles.AppUser)]
    [Produces(typeof(SubscriptionDto))]
    public async Task<IActionResult> PostMySubscriptions([FromBody] SubscribeManyDto request)
    {
        foreach (var dto in request.Subscribe.OrEmpty())
        {
            var update = dto.ToUpdate();

            await subscriptionStore.UpsertAsync(App.Id, UserId, dto.TopicPrefix, update, HttpContext.RequestAborted);
        }

        foreach (var topic in request.Unsubscribe.OrEmpty())
        {
            await subscriptionStore.DeleteAsync(App.Id, UserId, topic, HttpContext.RequestAborted);
        }

        return NoContent();
    }

    /// <summary>
    /// Remove my subscription.
    /// </summary>
    /// <param name="prefix">The topic prefix.</param>
    /// <returns>
    /// 204 => User unsubscribed.
    /// </returns>
    /// <remarks>
    /// User Id and App Id are resolved using the API token.
    /// </remarks>
    [HttpPost("api/me/subscriptions/{*prefix}")]
    [AppPermission(NotifoRoles.AppAdmin)]
    public async Task<IActionResult> DeleteSubscription(string prefix)
    {
        await subscriptionStore.DeleteAsync(App.Id, UserId, Uri.UnescapeDataString(prefix), HttpContext.RequestAborted);

        return NoContent();
    }
}
