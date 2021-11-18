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
using Notifo.Pipeline;
using NSwag.Annotations;

namespace Notifo.Areas.Api.Controllers.Users
{
    [OpenApiTag("user")]
    public class UserController : BaseController
    {
        private readonly ISubscriptionStore subscriptionStore;
        private readonly IUserStore userStore;

        public UserController(ISubscriptionStore subscriptionStore, IUserStore userStore)
        {
            this.subscriptionStore = subscriptionStore;
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
            var update = request.ToUpdate();

            var user = await userStore.UpsertAsync(App.Id, UserIdOrSub, update, HttpContext.RequestAborted);

            var response = ProfileDto.FromDomainObject(user!, App);

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
        /// Creates a user subscription.
        /// </summary>
        /// <param name="request">The subscription settings.</param>
        /// <returns>
        /// 204 => Topic created.
        /// </returns>
        /// <remarks>
        /// User Id and App Id are resolved using the API token.
        /// </remarks>
        [HttpPost("api/me/subscriptions")]
        [AppPermission(NotifoRoles.AppUser)]
        [Produces(typeof(SubscriptionDto))]
        public async Task<IActionResult> PostMySubscription([FromBody] SubscriptionDto request)
        {
            var update = request.ToUpdate();

            var subscription = await subscriptionStore.UpsertAsync(App.Id, UserId, request.TopicPrefix, update, HttpContext.RequestAborted);

            var response = SubscriptionDto.FromDomainObject(subscription);

            return Ok(response);
        }

        /// <summary>
        /// Deletes a user subscription.
        /// </summary>
        /// <param name="topic">The topic path.</param>
        /// <returns>
        /// 204 => Topic deleted.
        /// </returns>
        /// <remarks>
        /// User Id and App Id are resolved using the API token.
        /// </remarks>
        [HttpDelete("api/me/subscriptions/{*topic}")]
        [AppPermission(NotifoRoles.AppUser)]
        public async Task<IActionResult> DeleteMySubscription(string topic)
        {
            await subscriptionStore.DeleteAsync(App.Id, UserIdOrSub, topic, HttpContext.RequestAborted);

            return NoContent();
        }
    }
}
