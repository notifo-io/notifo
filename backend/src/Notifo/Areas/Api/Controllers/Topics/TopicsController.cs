// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Notifo.Areas.Api.Controllers.Topics.Dtos;
using Notifo.Domain;
using Notifo.Domain.Subscriptions;
using Notifo.Domain.Topics;
using Notifo.Pipeline;
using NSwag.Annotations;

namespace Notifo.Areas.Api.Controllers.Topics
{
    [OpenApiTag("Topics")]
    public sealed class TopicsController : BaseController
    {
        private readonly ISubscriptionStore subscriptionStore;
        private readonly ITopicStore topicStore;

        public TopicsController(ISubscriptionStore subscriptionStore, ITopicStore topicStore)
        {
            this.subscriptionStore = subscriptionStore;
            this.topicStore = topicStore;
        }

        /// <summary>
        /// Query topics.
        /// </summary>
        /// <param name="appId">The app where the topics belongs to.</param>
        /// <param name="q">The query object.</param>
        /// <returns>
        /// 200 => Topics returned.
        /// </returns>
        [HttpGet("api/apps/{appId}/topics/")]
        [AppPermission(Roles.Admin)]
        [Produces(typeof(ListResponseDto<TopicDto>))]
        public async Task<IActionResult> GetTopics(string appId, [FromQuery] QueryDto q)
        {
            var topics = await topicStore.QueryAsync(appId, q.ToQuery<TopicQuery>(), HttpContext.RequestAborted);

            var response = new ListResponseDto<TopicDto>();

            response.Items.AddRange(topics.Select(TopicDto.FromTopic));
            response.Total = topics.Total;

            return Ok(response);
        }

        /// <summary>
        /// Gets a user subscription.
        /// </summary>
        /// <param name="topic">The topic path.</param>
        /// <returns>
        /// 204 => Subscription exists.
        /// 404 => Subscription does not exist.
        /// </returns>
        /// <remarks>
        /// User Id and App Id are resolved using the API token.
        /// </remarks>
        [HttpGet("api/web/subscriptions/{*topic}")]
        [AppPermission(Roles.User)]
        public async Task<IActionResult> GetSubscription(string topic)
        {
            var subscription = await subscriptionStore.GetAsync(App.Id, UserId!, topic, HttpContext.RequestAborted);

            if (subscription == null)
            {
                return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        /// Creates a user subscription.
        /// </summary>
        /// <param name="topic">The topic path.</param>
        /// <returns>
        /// 204 => Topic created.
        /// </returns>
        /// <remarks>
        /// User Id and App Id are resolved using the API token.
        /// </remarks>
        [HttpPost("api/web/subscriptions/{*topic}")]
        [AppPermission(Roles.User)]
        public async Task<IActionResult> Subscribe(string topic)
        {
            await subscriptionStore.SubscribeWhenNotFoundAsync(App.Id, UserId!, topic, HttpContext.RequestAborted);

            return NoContent();
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
        [HttpDelete("api/web/subscriptions/{*topic}")]
        [AppPermission(Roles.User)]
        public async Task<IActionResult> Unsubscribe(string topic)
        {
            await subscriptionStore.UnsubscribeAsync(App.Id, UserId!, topic, HttpContext.RequestAborted);

            return NoContent();
        }
    }
}
