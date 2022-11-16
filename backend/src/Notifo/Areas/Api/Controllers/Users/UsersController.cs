// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using NodaTime;
using Notifo.Areas.Api.Controllers.Users.Dtos;
using Notifo.Domain.Identity;
using Notifo.Domain.Integrations;
using Notifo.Domain.Subscriptions;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Users;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Validation;
using Notifo.Pipeline;

#pragma warning disable IDE0060 // Remove unused parameter

namespace Notifo.Areas.Api.Controllers.Users;

[ApiExplorerSettings(GroupName = "Users")]
public sealed class UsersController : BaseController
{
    private readonly IIntegrationManager integrationManager;
    private readonly IUserStore userStore;
    private readonly IUserNotificationStore userNotificationStore;
    private readonly ISubscriptionStore subscriptionStore;

    public UsersController(
        IIntegrationManager integrationManager,
        IUserStore userStore,
        IUserNotificationStore userNotificationStore,
        ISubscriptionStore subscriptionStore)
    {
        this.integrationManager = integrationManager;
        this.userStore = userStore;
        this.userNotificationStore = userNotificationStore;
        this.subscriptionStore = subscriptionStore;
    }

    /// <summary>
    /// Query users.
    /// </summary>
    /// <param name="appId">The app where the users belongs to.</param>
    /// <param name="q">The query object.</param>
    /// <param name="withDetails">Provide extra details, might be expensive.</param>
    /// <response code="200">Users returned.</response>.
    /// <response code="404">App not found.</response>.
    [HttpGet("api/apps/{appId:notEmpty}/users/")]
    [AppPermission(NotifoRoles.AppAdmin)]
    [Produces(typeof(ListResponseDto<UserDto>))]
    public async Task<IActionResult> GetUsers(string appId, [FromQuery] QueryDto q, [FromQuery] bool withDetails = false)
    {
        var users = await userStore.QueryAsync(appId, q.ToQuery<UserQuery>(true), HttpContext.RequestAborted);

        IReadOnlyDictionary<string, Instant>? lastUpdates = null;

        if (withDetails)
        {
            lastUpdates = await QueryLastNotificationsAsync(appId, users.Select(x => x.Id));
        }

        var response = new ListResponseDto<UserDto>();

        response.Items.AddRange(users.Select(x => UserDto.FromDomainObject(x, null, lastUpdates)));
        response.Total = users.Total;

        return Ok(response);
    }

    /// <summary>
    /// Get a user.
    /// </summary>
    /// <param name="appId">The app where the user belongs to.</param>
    /// <param name="id">The user ID.</param>
    /// <param name="withDetails">Provide extra details, might be expensive.</param>
    /// <response code="200">User returned.</response>.
    /// <response code="404">User or app not found.</response>.
    [HttpGet("api/apps/{appId:notEmpty}/users/{id:notEmpty}")]
    [AppPermission(NotifoRoles.AppAdmin)]
    [Produces(typeof(UserDto))]
    public async Task<IActionResult> GetUser(string appId, string id, [FromQuery] bool withDetails = false)
    {
        var user = await userStore.GetAsync(appId, id, HttpContext.RequestAborted);

        if (user == null)
        {
            return NotFound();
        }

        IReadOnlyDictionary<string, Instant>? lastUpdates = null;

        if (withDetails)
        {
            lastUpdates = await QueryLastNotificationsAsync(appId, Enumerable.Repeat(user.Id, 1));
        }

        var response = UserDto.FromDomainObject(user, GetUserProperties(), lastUpdates);

        return Ok(response);
    }

    /// <summary>
    /// Query user subscriptions.
    /// </summary>
    /// <param name="appId">The app where the user belongs to.</param>
    /// <param name="id">The user ID.</param>
    /// <param name="q">The query object.</param>
    /// <response code="200">User subscriptions returned.</response>.
    /// <response code="404">User or app not found.</response>.
    [HttpGet("api/apps/{appId:notEmpty}/users/{id:notEmpty}/subscriptions")]
    [AppPermission(NotifoRoles.AppAdmin)]
    [Produces(typeof(ListResponseDto<SubscriptionDto>))]
    public async Task<IActionResult> GetSubscriptions(string appId, string id, [FromQuery] QueryDto q)
    {
        var subscriptions = await subscriptionStore.QueryAsync(appId, ParseQuery(id, q), HttpContext.RequestAborted);

        var response = new ListResponseDto<SubscriptionDto>();

        response.Items.AddRange(subscriptions.Select(SubscriptionDto.FromDomainObject));
        response.Total = subscriptions.Total;

        return Ok(response);
    }

    /// <summary>
    /// Upserts or deletes multiple user subscriptions.
    /// </summary>
    /// <param name="appId">The app where the user belongs to.</param>
    /// <param name="id">The user ID.</param>
    /// <param name="request">The subscription object.</param>
    /// <response code="204">User subscribed.</response>.
    /// <response code="404">User or app not found.</response>.
    [HttpPost("api/apps/{appId:notEmpty}/users/{id:notEmpty}/subscriptions")]
    [AppPermission(NotifoRoles.AppAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> PostSubscriptions(string appId, string id, [FromBody] SubscribeManyDto request)
    {
        var user = await userStore.GetAsync(appId, id, HttpContext.RequestAborted);

        if (user == null)
        {
            return NotFound();
        }

        foreach (var dto in request.Subscribe.OrEmpty())
        {
            var command = dto.ToUpdate(id);

            await Mediator.SendAsync(command, HttpContext.RequestAborted);
        }

        foreach (var topic in request.Unsubscribe.OrEmpty())
        {
            var command = new DeleteSubscription { UserId = id, Topic = topic };

            await Mediator.SendAsync(command, HttpContext.RequestAborted);
        }

        return NoContent();
    }

    /// <summary>
    /// Unsubscribes a user from a subscription.
    /// </summary>
    /// <param name="appId">The app where the user belongs to.</param>
    /// <param name="id">The user ID.</param>
    /// <param name="prefix">The topic prefix.</param>
    /// <response code="204">User unsubscribed.</response>.
    /// <response code="404">User or app not found.</response>.
    [HttpDelete("api/apps/{appId:notEmpty}/users/{id:notEmpty}/subscriptions/{*prefix}")]
    [AppPermission(NotifoRoles.AppAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteSubscription(string appId, string id, string prefix)
    {
        var user = await userStore.GetAsync(appId, id, HttpContext.RequestAborted);

        if (user == null)
        {
            return NotFound();
        }

        var command = new DeleteSubscription { UserId = id, Topic = Uri.UnescapeDataString(prefix) };

        await Mediator.SendAsync(command, HttpContext.RequestAborted);

        return NoContent();
    }

    /// <summary>
    /// Upsert users.
    /// </summary>
    /// <param name="appId">The app where the users belong to.</param>
    /// <param name="request">The upsert request.</param>
    /// <response code="200">Users upserted.</response>.
    /// <response code="404">App not found.</response>.
    [HttpPost("api/apps/{appId:notEmpty}/users/")]
    [AppPermission(NotifoRoles.AppAdmin)]
    [Produces(typeof(List<UserDto>))]
    public async Task<IActionResult> PostUsers(string appId, [FromBody] UpsertUsersDto request)
    {
        var response = new List<UserDto>();

        if (request.Requests.Length > 100)
        {
            throw new ValidationException($"Only 100 users can be update in one request, found: {request.Requests.Length}");
        }

        foreach (var dto in request.Requests.OrEmpty().NotNull())
        {
            var command = dto.ToUpsert();

            var user = await Mediator.SendAsync(command, HttpContext.RequestAborted);

            response.Add(UserDto.FromDomainObject(user!, null, null));
        }

        return Ok(response);
    }

    /// <summary>
    /// Add an allowed topic.
    /// </summary>
    /// <param name="appId">The app where the users belong to.</param>
    /// <param name="id">The user ID.</param>
    /// <param name="request">The upsert request.</param>
    /// <response code="204">User updated.</response>.
    /// <response code="404">User or app not found.</response>.
    [HttpPost("api/apps/{appId:notEmpty}/users/{id:notEmpty}/allowed-topics")]
    [AppPermission(NotifoRoles.AppAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> PostAllowedTopic(string appId, string id, [FromBody] AddAllowedTopicDto request)
    {
        var user = await userStore.GetAsync(appId, id, HttpContext.RequestAborted);

        if (user == null)
        {
            return NotFound();
        }

        var command = request.ToUpdate(id);

        await Mediator.SendAsync(command, HttpContext.RequestAborted);

        return NoContent();
    }

    /// <summary>
    /// Remove an allowed topic.
    /// </summary>
    /// <param name="appId">The app where the users belong to.</param>
    /// <param name="id">The user ID.</param>
    /// <param name="prefix">The topic prefix.</param>
    /// <response code="204">User updated.</response>.
    /// <response code="404">User or app not found.</response>.
    [HttpDelete("api/apps/{appId:notEmpty}/users/{id:notEmpty}/allowed-topics/{*prefix}")]
    [AppPermission(NotifoRoles.AppAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAllowedTopic(string appId, string id, string prefix)
    {
        var user = await userStore.GetAsync(appId, id, HttpContext.RequestAborted);

        if (user == null)
        {
            return NotFound();
        }

        var command = new RemoveUserAllowedTopic { UserId = id, Prefix = prefix };

        await Mediator.SendAsync(command, HttpContext.RequestAborted);

        return NoContent();
    }

    /// <summary>
    /// Remove an web push token.
    /// </summary>
    /// <param name="appId">The app where the users belong to.</param>
    /// <param name="id">The user ID.</param>
    /// <param name="token">The token.</param>
    /// <response code="204">User updated.</response>.
    /// <response code="404">User or app not found.</response>.
    [HttpDelete("api/apps/{appId:notEmpty}/users/{id:notEmpty}/mobilepush/{token}")]
    [AppPermission(NotifoRoles.AppAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteMobilePushToken(string appId, string id, string token)
    {
        var user = await userStore.GetAsync(appId, id, HttpContext.RequestAborted);

        if (user == null)
        {
            return NotFound();
        }

        var command = new RemoveUserMobileToken { UserId = id, Token = token };

        await Mediator.SendAsync(command, HttpContext.RequestAborted);

        return NoContent();
    }

    /// <summary>
    /// Remove an web push subscription.
    /// </summary>
    /// <param name="appId">The app where the users belong to.</param>
    /// <param name="id">The user ID.</param>
    /// <param name="endpoint">The endpoint.</param>
    /// <response code="204">User updated.</response>.
    /// <response code="404">User or app not found.</response>.
    [HttpDelete("api/apps/{appId:notEmpty}/users/{id:notEmpty}/webpush/{endpoint}")]
    [AppPermission(NotifoRoles.AppAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteWebPushSubscription(string appId, string id, string endpoint)
    {
        var user = await userStore.GetAsync(appId, id, HttpContext.RequestAborted);

        if (user == null)
        {
            return NotFound();
        }

        var command = new RemoveUserWebPushSubscription { UserId = id, Endpoint = endpoint };

        await Mediator.SendAsync(command, HttpContext.RequestAborted);

        return NoContent();
    }

    /// <summary>
    /// Delete a user.
    /// </summary>
    /// <param name="appId">The app where the users belongs to.</param>
    /// <param name="id">The user id to delete.</param>
    /// <response code="204">User deleted.</response>.
    /// <response code="404">App not found.</response>.
    [HttpDelete("api/apps/{appId:notEmpty}/users/{id:notEmpty}")]
    [AppPermission(NotifoRoles.AppAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteUser(string appId, string id)
    {
        var command = new DeleteUser { UserId = id };

        await Mediator.SendAsync(command, HttpContext.RequestAborted);

        return NoContent();
    }

    private Task<IReadOnlyDictionary<string, Instant>> QueryLastNotificationsAsync(string appId, IEnumerable<string> userIds)
    {
        return userNotificationStore.QueryLastNotificationsAsync(appId, userIds, HttpContext.RequestAborted);
    }

    private static SubscriptionQuery ParseQuery(string id, QueryDto query)
    {
        var queryObject = query.ToQuery<SubscriptionQuery>(true);

        queryObject.UserId = id;

        return queryObject;
    }

    private List<UserProperty> GetUserProperties()
    {
        var properties = new Dictionary<string, UserProperty>();

        foreach (var (_, configured) in App.Integrations)
        {
            var integration = integrationManager.Definitions.FirstOrDefault(x => x.Type == configured.Type);

            if (integration != null)
            {
                foreach (var userProperty in integration.UserProperties)
                {
                    properties[userProperty.Name] = userProperty;
                }
            }
        }

        return properties.Values.OrderBy(x => x.Name).ToList();
    }
}
