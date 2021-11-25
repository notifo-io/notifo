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
using Notifo.Domain.Users;
using Notifo.Infrastructure.Validation;
using Notifo.Pipeline;
using NSwag.Annotations;

namespace Notifo.Areas.Api.Controllers.Users
{
    [OpenApiTag("Users")]
    public sealed class UsersController : BaseController
    {
        private readonly IUserStore userStore;
        private readonly ISubscriptionStore subscriptionStore;

        public UsersController(IUserStore userStore, ISubscriptionStore subscriptionStore)
        {
            this.userStore = userStore;
            this.subscriptionStore = subscriptionStore;
        }

        /// <summary>
        /// Query users.
        /// </summary>
        /// <param name="appId">The app where the users belongs to.</param>
        /// <param name="q">The query object.</param>
        /// <returns>
        /// 200 => Users returned.
        /// 404 => App not found.
        /// </returns>
        [HttpGet("api/apps/{appId}/users/")]
        [AppPermission(NotifoRoles.AppAdmin)]
        [Produces(typeof(ListResponseDto<UserDto>))]
        public async Task<IActionResult> GetUsers(string appId, [FromQuery] QueryDto q)
        {
            var users = await userStore.QueryAsync(appId, q.ToQuery<UserQuery>(true), HttpContext.RequestAborted);

            var response = new ListResponseDto<UserDto>();

            response.Items.AddRange(users.Select(UserDto.FromDomainObject));
            response.Total = users.Total;

            return Ok(response);
        }

        /// <summary>
        /// Get a user.
        /// </summary>
        /// <param name="appId">The app where the user belongs to.</param>
        /// <param name="id">The user ID.</param>
        /// <returns>
        /// 200 => User returned.
        /// 404 => User or app not found.
        /// </returns>
        [HttpGet("api/apps/{appId}/users/{id}")]
        [AppPermission(NotifoRoles.AppAdmin)]
        [Produces(typeof(UserDto))]
        public async Task<IActionResult> GetUser(string appId, string id)
        {
            var user = await userStore.GetAsync(appId, id, HttpContext.RequestAborted);

            if (user == null)
            {
                return NotFound();
            }

            var response = UserDto.FromDomainObject(user);

            return Ok(response);
        }

        /// <summary>
        /// Query user subscriptions.
        /// </summary>
        /// <param name="appId">The app where the user belongs to.</param>
        /// <param name="id">The user ID.</param>
        /// <param name="q">The query object.</param>
        /// <returns>
        /// 200 => User subscriptions returned.
        /// 404 => User or app not found.
        /// </returns>
        [HttpGet("api/apps/{appId}/users/{id}/subscriptions")]
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
        /// Upsert a user subscriptions.
        /// </summary>
        /// <param name="appId">The app where the user belongs to.</param>
        /// <param name="id">The user ID.</param>
        /// <param name="request">The subscription object.</param>
        /// <returns>
        /// 204 => User subscribed.
        /// 404 => User or app not found.
        /// </returns>
        [HttpPost("api/apps/{appId}/users/{id}/subscriptions")]
        [AppPermission(NotifoRoles.AppAdmin)]
        public async Task<IActionResult> PostSubscription(string appId, string id, [FromBody] SubscriptionDto request)
        {
            var user = await userStore.GetAsync(appId, id, HttpContext.RequestAborted);

            if (user == null)
            {
                return NotFound();
            }

            var update = request.ToUpdate();

            await subscriptionStore.UpsertAsync(appId, id, request.TopicPrefix, update, HttpContext.RequestAborted);

            return NoContent();
        }

        /// <summary>
        /// Remove a user subscriptions.
        /// </summary>
        /// <param name="appId">The app where the user belongs to.</param>
        /// <param name="id">The user ID.</param>
        /// <param name="prefix">The topic prefix.</param>
        /// <returns>
        /// 204 => User subscribed.
        /// 404 => User or app not found.
        /// </returns>
        [HttpDelete("api/apps/{appId}/users/{id}/subscriptions/{*prefix}")]
        [AppPermission(NotifoRoles.AppAdmin)]
        public async Task<IActionResult> DeleteSubscription(string appId, string id, string prefix)
        {
            var user = await userStore.GetAsync(appId, id, HttpContext.RequestAborted);

            if (user == null)
            {
                return NotFound();
            }

            await subscriptionStore.DeleteAsync(appId, id, prefix, HttpContext.RequestAborted);

            return NoContent();
        }

        /// <summary>
        /// Upsert users.
        /// </summary>
        /// <param name="appId">The app where the users belong to.</param>
        /// <param name="request">The upsert request.</param>
        /// <returns>
        /// 200 => Users upserted.
        /// 404 => App not found.
        /// </returns>
        [HttpPost("api/apps/{appId}/users/")]
        [AppPermission(NotifoRoles.AppAdmin)]
        [Produces(typeof(List<UserDto>))]
        public async Task<IActionResult> PostUsers(string appId, [FromBody] UpsertUsersDto request)
        {
            var response = new List<UserDto>();

            if (request?.Requests != null)
            {
                if (request.Requests.Length > 100)
                {
                    throw new ValidationException($"Only 100 users can be update in one request, found: {request.Requests.Length}");
                }

                foreach (var dto in request.Requests)
                {
                    if (dto != null)
                    {
                        var update = dto.ToUpsert();

                        var user = await userStore.UpsertAsync(appId, dto.Id, update, HttpContext.RequestAborted);

                        response.Add(UserDto.FromDomainObject(user));
                    }
                }
            }

            return Ok(response);
        }

        /// <summary>
        /// Add an allowed topic.
        /// </summary>
        /// <param name="appId">The app where the users belong to.</param>
        /// <param name="id">The user ID.</param>
        /// <param name="request">The upsert request.</param>
        /// <returns>
        /// 204 => User updated.
        /// 404 => User or app not found.
        /// </returns>
        [HttpPost("api/apps/{appId}/users/{id}/allowed-topics")]
        [AppPermission(NotifoRoles.AppAdmin)]
        [Produces(typeof(List<UserDto>))]
        public async Task<IActionResult> PostAllowedTopic(string appId, string id, [FromBody] AddAllowedTopicDto request)
        {
            var user = await userStore.GetAsync(appId, id, HttpContext.RequestAborted);

            if (user == null)
            {
                return NotFound();
            }

            var update = request.ToUpdate();

            await userStore.UpsertAsync(appId, id, update, HttpContext.RequestAborted);

            return NoContent();
        }

        /// <summary>
        /// Remove an allowed topic.
        /// </summary>
        /// <param name="appId">The app where the users belong to.</param>
        /// <param name="id">The user ID.</param>
        /// <param name="prefix">The topic prefix.</param>
        /// <returns>
        /// 204 => User updated.
        /// 404 => User or app not found.
        /// </returns>
        [HttpDelete("api/apps/{appId}/users/{id}/allowed-topics/{*prefix}")]
        [AppPermission(NotifoRoles.AppAdmin)]
        [Produces(typeof(List<UserDto>))]
        public async Task<IActionResult> DeleteAllowedTopic(string appId, string id, string prefix)
        {
            var user = await userStore.GetAsync(appId, id, HttpContext.RequestAborted);

            if (user == null)
            {
                return NotFound();
            }

            var update = new RemoveUserAllowedTopic { Prefix = prefix };

            await userStore.UpsertAsync(appId, id, update, HttpContext.RequestAborted);

            return NoContent();
        }

        /// <summary>
        /// Delete a user.
        /// </summary>
        /// <param name="appId">The app where the users belongs to.</param>
        /// <param name="id">The user id to delete.</param>
        /// <returns>
        /// 200 => User deleted.
        /// 404 => App not found.
        /// </returns>
        [HttpDelete("api/apps/{appId}/users/{id}")]
        [AppPermission(NotifoRoles.AppAdmin)]
        [Produces(typeof(ListResponseDto<UserDto>))]
        public async Task<IActionResult> DeleteUser(string appId, string id)
        {
            await userStore.DeleteAsync(appId, id, HttpContext.RequestAborted);

            return NoContent();
        }

        private static SubscriptionQuery ParseQuery(string id, QueryDto query)
        {
            var queryObject = query.ToQuery<SubscriptionQuery>(true);

            queryObject.UserId = id;

            return queryObject;
        }
    }
}
