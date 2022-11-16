// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Notifo.Areas.Api.Controllers.Users.Dtos;
using Notifo.Domain.Identity;
using Notifo.Domain.Integrated;
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
    private readonly IIntegratedAppService integratedApp;
    private readonly IUserStore userStore;

    public UserController(ISubscriptionStore subscriptionStore, ITopicStore topicStore, IIntegratedAppService integratedApp, IUserStore userStore)
    {
        this.subscriptionStore = subscriptionStore;
        this.topicStore = topicStore;
        this.integratedApp = integratedApp;
        this.userStore = userStore;
    }

    /// <summary>
    /// Get the current user.
    /// </summary>
    /// <response code="200">User returned.</response>.
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
    /// Get the current admin user.
    /// </summary>
    /// <response code="200">User returned.</response>.
    [HttpGet("api/me/admin")]
    [AppPermission]
    [Produces(typeof(AdminProfileDto))]
    public async Task<IActionResult> GetAdminUser()
    {
        var token = await integratedApp.GetTokenAsync(UserIdOrSub, HttpContext.RequestAborted);

        var response = new AdminProfileDto
        {
            Token = token
        };

        return Ok(response);
    }

    /// <summary>
    /// Update the user.
    /// </summary>
    /// <param name="request">The upsert request.</param>
    /// <response code="200">Users upserted.</response>.
    [HttpPost("api/me")]
    [AppPermission(NotifoRoles.AppUser)]
    [Produces(typeof(ProfileDto))]
    public async Task<IActionResult> PostUser([FromBody] UpdateProfileDto request)
    {
        var command = request.ToUpsert(UserId);

        var user = await Mediator.SendAsync(command, HttpContext.RequestAborted);

        var response = ProfileDto.FromDomainObject(user!, App);

        return Ok(response);
    }

    /// <summary>
    /// Query the user topics.
    /// </summary>
    /// <param name="language">The optional language.</param>
    /// <response code="200">User subscriptions returned.</response>.
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
    /// <response code="200">User subscriptions returned.</response>.
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
    /// <response code="200">Subscription exists.</response>.
    /// <response code="404">Subscription does not exist.</response>.
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
    /// <response code="204">User subscribed.</response>.
    /// <remarks>
    /// User Id and App Id are resolved using the API token.
    /// </remarks>
    [HttpPost("api/me/subscriptions")]
    [AppPermission(NotifoRoles.AppUser)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> PostMySubscriptions([FromBody] SubscribeManyDto request)
    {
        foreach (var dto in request.Subscribe.OrEmpty())
        {
            var update = dto.ToUpdate(UserId);

            await Mediator.SendAsync(update, HttpContext.RequestAborted);
        }

        foreach (var topic in request.Unsubscribe.OrEmpty())
        {
            var update = new DeleteSubscription { UserId = UserId, Topic = topic };

            await Mediator.SendAsync(update, HttpContext.RequestAborted);
        }

        return NoContent();
    }

    /// <summary>
    /// Remove my subscription.
    /// </summary>
    /// <param name="prefix">The topic prefix.</param>
    /// <response code="204">User unsubscribed.</response>.
    /// <remarks>
    /// User Id and App Id are resolved using the API token.
    /// </remarks>
    [HttpPost("api/me/subscriptions/{*prefix}")]
    [AppPermission(NotifoRoles.AppAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteSubscription(string prefix)
    {
        var command = new DeleteSubscription { UserId = UserId, Topic = Uri.UnescapeDataString(prefix) };

        await Mediator.SendAsync(command, HttpContext.RequestAborted);

        return NoContent();
    }
}
